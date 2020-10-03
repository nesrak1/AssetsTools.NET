// //////////////////////////////////////////////////////////
// smallz4.h
// Copyright (c) 2016-2020 Stephan Brumme. All rights reserved.
// see https://create.stephan-brumme.com/smallz4/
//
// "MIT License":
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//c# port notes (tbh it's amazing this thing even works)
//all types have been left exactly as they were originally
//they are applied in order from inside out to make c# happy
//I wouldn't be surprised if a bad cast breaks something
//performance is not really a goal as long as it's reasonable

using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET.Extra.Decompressors.LZ4
{
    public class SmaLLz4
    {
        ////// constants
        /// greedy mode for short chains (compression level <= 3) instead of optimal parsing / lazy evaluation
        private const int ShortChainsGreedy = 3;
        /// lazy evaluation for medium-sized chains (compression level > 3 and <= 6)
        private const int ShortChainsLazy = 6;

        /// each match's length must be >= 4
        private const int MinMatch = 4;
        /// a literal needs one byte
        private const int JustLiteral = 1;
        /// last match must not be closer than 12 bytes to the end
        private const int BlockEndNoMatch = 12;
        /// last 5 bytes must be literals, no matching allowed
        private const int BlockEndLiterals =  5;

        /// match finder's hash table size (2^HashBits entries, must be less than 32)
        private const int HashBits = 20;
        private const int HashSize = 1 << HashBits;

        /// input buffer size, can be any number but zero ;-)
        private const int BufferSize = 64*1024;

        /// maximum match distance, must be power of 2 minus 1
        private const int MaxDistance = 65535;
        /// marker for "no match"
        private const int EndOfChain = 0;
        /// stop match finding after MaxChainLength steps (default is unlimited => optimal parsing)
        private const int MaxChainLength = MaxDistance;

        /// significantly speed up parsing if the same byte is repeated a lot, may cause sub-optimal compression
        private const int MaxSameLetter = 19 + 255*256; // was: 19 + 255,

        /// maximum block size as defined in LZ4 spec: { 0,0,0,0,64*1024,256*1024,1024*1024,4*1024*1024 }
        /// I only work with the biggest maximum block size (7)
        //  note: xxhash header checksum is precalculated only for 7, too
        private const int MaxBlockSizeId = 7;
        private const int MaxBlockSize = 4*1024*1024;

        /// legacy format has a fixed block size of 8 MB
        private const int MaxBlockSizeLegacy = 8*1024*1024;

        /// number of literals and match length is encoded in several bytes, max. 255 per byte
        private const int MaxLengthCode = 255;
        //////

        private ushort maxChainLength;

        public struct Match
        {
            public ulong length;
            public ushort distance;
        }

        public SmaLLz4(ushort newMaxChainLength = MaxChainLength)
        {
            maxChainLength = newMaxChainLength;
        }

        /// compress everything in input stream (accessed via getByte) and write to output stream (via send)
        public static void Lz4(BinaryReader getBytes, BinaryWriter sendBytes, ushort maxChainLength = MaxChainLength, bool useLegacyFormat = false)
        {
            Lz4(getBytes, sendBytes, maxChainLength, new List<byte>(), useLegacyFormat);
        }

        /// compress everything in input stream (accessed via getByte) and write to output stream (via send)
        public static void Lz4(BinaryReader getBytes, BinaryWriter sendBytes, ushort maxChainLength, List<byte> dictionary, bool useLegacyFormat = false)
        {
            SmaLLz4 lz4 = new SmaLLz4(maxChainLength);
            lz4.Compress(getBytes, sendBytes, new List<byte>(), useLegacyFormat);
        }

        /// version string
        public static string GetVersion()
        {
            return "1.5";
        }

        /// return true, if the four bytes at *a and *b match
        private static bool Match4(byte[] data, long a, long b)
        {
            return BitConverter.ToInt32(data, (int)a) == BitConverter.ToInt32(data, (int)b);
        }

        /// simple hash function, input: 32 bits, output: HashBits bits (by default: 20)
        private static uint GetHash32(uint fourBytes)
        {
            // taken from https://en.wikipedia.org/wiki/Linear_congruential_generator
            uint HashMultiplier = 48271;
            return ((fourBytes * HashMultiplier) >> (32 - HashBits)) & (HashSize - 1);
        }

        /// find longest match of data[pos] between data[begin] and data[end], use match chain
        private Match FindLongestMatch(byte[] data, ulong pos, ulong begin, ulong end, List<ushort> chain)
        {
            Match result = new Match();
            result.length = JustLiteral; // assume a literal => one byte

            // compression level: look only at the first n entries of the match chain
            ushort stepsLeft = maxChainLength;
            // findLongestMatch() shouldn't be called when maxChainLength = 0 (uncompressed)

            // pointer to position that is currently analyzed (which we try to find a great match for)
            ulong current = pos - begin;
            // don't match beyond this point
            ulong stop = current + end - pos;

            // get distance to previous match, abort if 0 => not existing
            ushort distance = chain[(int)pos & MaxDistance];
            long totalDistance = 0;
            while (distance != EndOfChain)
            {
                // chain goes too far back ?
                totalDistance += distance;
                if (totalDistance > MaxDistance)
                    break; // can't match beyond 64k

                // prepare next position
                distance = chain[(int)((long)pos - totalDistance) & MaxDistance];

                // let's introduce a new pointer atLeast that points to the first "new" byte of a potential longer match
                ulong atLeast = current + result.length + 1;
                // impossible to find a longer match because not enough bytes left ?
                if (atLeast > stop)
                    break;

                // the idea is to split the comparison algorithm into 2 phases
                // (1) scan backward from atLeast to current, abort if mismatch
                // (2) scan forward  until a mismatch is found and store length/distance of this new best match
                // current                  atLeast
                //    |                        |
                //    -<<<<<<<< phase 1 <<<<<<<<
                //                              >>> phase 2 >>>
                // main reason for phase 1:
                // - both byte sequences start with the same bytes, quite likely they are very similar
                // - there is a good chance that if they differ, then their last bytes differ
                // => checking the last first increases the probability that a mismatch is detected as early as possible

                // compare 4 bytes at once
                const ulong CheckAtOnce = 4;

                // all bytes between current and atLeast shall be identical
                ulong phase1 = atLeast - CheckAtOnce; // minus 4 because match4 checks 4 bytes
                while (phase1 > current && Match4(data, (long)phase1, (long)phase1 - totalDistance))
                    phase1 -= CheckAtOnce;
                // note: - the first four bytes always match
                //       - in the last iteration, phase1 points either at current + 1 or current + 2 or current + 3
                //       - therefore we compare a few bytes twice => but a check to skip these checks is more expensive

                // mismatch ? (the while-loop was aborted)
                if (phase1 > current)
                    continue;

                // we have a new best match, now scan forward
                ulong phase2 = atLeast;

                // fast loop: check four bytes at once
                while (phase2 + CheckAtOnce <= stop && Match4(data, (long)phase2, (long)phase2 - totalDistance))
                    phase2 += CheckAtOnce;
                // slow loop: check the last 1/2/3 bytes
                while (phase2 < stop && BitConverter.ToInt32(data, (int)phase2) == BitConverter.ToInt32(data, (int)((long)phase2 - totalDistance)))
                    phase2++;

                // store new best match
                result.distance = (ushort)totalDistance;
                result.length = phase2 - current;

                // stop searching on lower compression levels
                if (--stepsLeft == 0)
                    break;
            }

            return result;
        }

        /// create shortest output
        /** data points to block's begin; we need it to extract literals **/
        private static List<byte> SelectBestMatches(List<Match> matches, byte[] data, uint dataOffset)
        {
            // store encoded data
            List<byte> result = new List<byte>(matches.Count);

            // indices of current run of literals
            uint literalsFrom = 0;
            uint numLiterals = 0;

            bool lastToken = false;

            // walk through the whole block
            for (uint offset = 0; offset < matches.Count;) // increment inside of loop
            {
                // get best cost-weighted match
                Match match = matches[(int)offset];

                // if no match, then count literals instead
                if (match.length <= JustLiteral)
                {
                    // first literal ? need to reset pointers of current sequence of literals
                    if (numLiterals == 0)
                        literalsFrom = offset;

                    // add one more literal to current sequence
                    numLiterals++;

                    // next match
                    offset++;

                    // continue unless it's the last literal
                    if (offset < matches.Count)
                        continue;

                    lastToken = true;
                }
                else
                {
                    // skip unused matches
                    offset += (uint)match.length;
                }

                // store match length (4 is implied because it's the minimum match length)
                int matchLength = (int)match.length - MinMatch;

                // last token has zero length
                if (lastToken)
                matchLength = 0;

                // token consists of match length and number of literals, let's start with match length ...
                byte token = (matchLength < 15) ? (byte)matchLength : (byte)15;

                // >= 15 literals ? (extra bytes to store length)
                if (numLiterals < 15)
                {
                    // add number of literals in higher four bits
                    token |= (byte)(numLiterals << 4);
                    result.Add(token);
                }
                else
                {
                    // set all higher four bits, the following bytes with determine the exact number of literals
                    result.Add((byte)(token | 0xF0));

                    // 15 is already encoded in token
                    int encodeNumLiterals = (int)numLiterals - 15;

                    // emit 255 until remainder is below 255
                    while (encodeNumLiterals >= MaxLengthCode)
                    {
                        result.Add(MaxLengthCode);
                        encodeNumLiterals -= MaxLengthCode;
                    }
                    // and the last byte (can be zero, too)
                    result.Add((byte)encodeNumLiterals);
                }
                // copy literals
                if (numLiterals > 0)
                {
                    byte[] temp = new byte[numLiterals];
                    Array.Copy(data, dataOffset + literalsFrom, temp, 0, numLiterals);
                    result.AddRange(temp);

                    // last token doesn't have a match
                    if (lastToken)
                        break;

                    // reset
                    numLiterals = 0;
                }

                // distance stored in 16 bits / little endian
                result.Add((byte)(match.distance & 0xFF));
                result.Add((byte)(match.distance >> 8));

                // >= 15+4 bytes matched
                if (matchLength >= 15)
                {
                    // 15 is already encoded in token
                    matchLength -= 15;
                    // emit 255 until remainder is below 255
                    while (matchLength >= MaxLengthCode)
                    {
                        result.Add(MaxLengthCode);
                        matchLength -= MaxLengthCode;
                    }
                    // and the last byte (can be zero, too)
                    result.Add((byte)matchLength);
                }
            }

            return result;
        }

        /// walk backwards through all matches and compute number of compressed bytes from current position to the end of the block
        /** note: matches are modified (shortened length) if necessary **/
        private static void EstimateCosts(List<Match> matches)
        {
            uint blockEnd = (uint)matches.Count;

            // equals the number of bytes after compression
            // minimum cost from this position to the end of the current block
            int costCount = matches.Count;
            List<uint> cost = new List<uint>(matches.Count);
            for (int i = 0; i < costCount; i++)
                cost.Add(0);
            // "cost" represents the number of bytes needed

            // the last bytes must always be literals
            ulong numLiterals = BlockEndLiterals;
            // backwards optimal parsing
            for (long i = (long)blockEnd - (1 + BlockEndLiterals); i >= 0; i--) // ignore the last 5 bytes, they are always literals
            {
                // if encoded as a literal
                numLiterals++;
                ulong bestLength = JustLiteral;
                // such a literal "costs" 1 byte
                uint minCost = cost[(int)(i + 1)] + JustLiteral;

                // an extra length byte is required for every 255 literals
                if (numLiterals >= 15)
                {
                    // same as: if ((numLiterals - 15) % MaxLengthCode == 0)
                    // but I try hard to avoid the slow modulo function
                    if (numLiterals == 15 || (numLiterals >= 15 + MaxLengthCode && (numLiterals - 15) % MaxLengthCode == 0))
                        minCost++;
                }

                // let's look at the longest match, almost always more efficient that the plain literals
                Match match = matches[(int)i];

                // very long self-referencing matches can slow down the program A LOT
                if (match.length >= MaxSameLetter && match.distance == 1)
                {
                    // assume that longest match is always the best match
                    // NOTE: this assumption might not be optimal !
                    bestLength = match.length;
                    minCost = cost[(int)(i + (long)match.length)] + 1 + 2 + 1 + (uint)(match.length - 19) / 255;
                }
                else
                {
                    // this is the core optimization loop

                    // overhead of encoding a match: token (1 byte) + offset (2 bytes) + sometimes extra bytes for long matches
                    uint extraCost = 1 + 2;
                    ulong nextCostIncrease = 18; // need one more byte for 19+ long matches (next increase: 19+255*x)

                    // try all match lengths (start with short ones)
                    for (ulong length = MinMatch; length <= match.length; length++)
                    {
                        // token (1 byte) + offset (2 bytes) + extra bytes for long matches
                        uint currentCost = cost[(int)(i + (long)length)] + extraCost;
                        // better choice ?
                        if (currentCost <= minCost)
                        {
                            // regarding the if-condition:
                            // "<"  prefers literals and shorter matches
                            // "<=" prefers longer matches
                            // they should produce the same number of bytes (because of the same cost)
                            // ... but every now and then it doesn't !
                            // that's why: too many consecutive literals require an extra length byte
                            // (which we took into consideration a few lines above)
                            // but we only looked at literals beyond the current position
                            // if there are many literal in front of the current position
                            // then it may be better to emit a match with the same cost as the literals at the current position
                            // => it "breaks" the long chain of literals and removes the extra length byte
                            minCost = currentCost;
                            bestLength = length;
                            // performance-wise, a long match is usually faster during decoding than multiple short matches
                            // on the other hand, literals are faster than short matches as well (assuming same cost)
                        }

                        // very long matches need extra bytes for encoding match length
                        if (length == nextCostIncrease)
                        {
                            extraCost++;
                            nextCostIncrease += MaxLengthCode;
                        }
                    }
                }

                // store lowest cost so far
                cost[(int)i] = minCost;

                // and adjust best match
                Match bestMatch = matches[(int)i];
                bestMatch.length = bestLength;
                matches[(int)i] = bestMatch;

                // reset number of literals if a match was chosen
                if (bestLength != JustLiteral)
                    numLiterals = 0;

                // note: if bestLength is smaller than the previous matches[i].length then there might be a closer match
                //       which could be more cache-friendly (=> faster decoding)
            }
        }

        private void Resize(List<byte> data, uint size, byte defaultValue = 0)
        {
            int cur = data.Count;
            if (size < cur)
            {
                data.RemoveRange((int)size, (int)(cur - size));
            }
            else if (size > cur)
            {
                if (size > data.Capacity)
                    data.Capacity = (int)size;
                for (int i = 0; i < size - cur; i++)
                    data.Add(defaultValue);
            }
        }

        private uint ToUInt32List(List<byte> data, int pos)
        {
            byte[] data2 = new byte[] { data[pos], data[pos + 1], data[pos + 2], data[pos + 3] };
            return BitConverter.ToUInt32(data2, 0);
        }

        /// compress everything in input stream (accessed via getByte) and write to output stream (via send), improve compression with a predefined dictionary
        public void Compress(BinaryReader getBytes, BinaryWriter sendBytes, List<byte> dictionary, bool useLegacyFormat)
        {
            // ==================== write header ====================
            if (useLegacyFormat)
            {
                // magic bytes
                byte[] header = { 0x02, 0x21, 0x4C, 0x18 };
                sendBytes.Write(header);
            }
            else
            {
                // frame header
                byte[] header =
                {
                    0x04, 0x22, 0x4D, 0x18, // magic bytes
                    1 << 6,                 // flags: no checksums, blocks depend on each other and no dictionary ID
                    MaxBlockSizeId << 4,    // max blocksize
                    0xDF                    // header checksum (precomputed)
                };
                sendBytes.Write(header);
            }

            // ==================== declarations ====================
            // change read buffer size as you like
            byte[] buffer = new byte[BufferSize];

            // read the file in chunks/blocks, data will contain only bytes which are relevant for the current block
            List<byte> data = new List<byte>();

            // file position corresponding to data[0]
            uint dataZero = 0;
            // last already read position
            uint numRead = 0;

            // passthru data ? (but still wrap it in LZ4 format)
            bool uncompressed = maxChainLength == 0;

            // last time we saw a hash
            ulong NoLastHash = unchecked((ulong)-1); // = -1
            List<ulong> lastHash = new List<ulong>(HashSize);
            for (int i = 0; i < HashSize; i++)
                lastHash.Add(NoLastHash);

            // previous position which starts with the same bytes
            List<ushort> previousHash = new List<ushort>(MaxDistance + 1); // long chains based on my simple hash
            for (int i = 0; i < MaxDistance + 1; i++)
                previousHash.Add(EndOfChain);
            List<ushort> previousExact = new List<ushort>(MaxDistance + 1); // shorter chains based on exact matching of the first four bytes
            for (int i = 0; i < MaxDistance + 1; i++)
                previousExact.Add(EndOfChain);
            // these two containers are essential for match finding:
            // 1. I compute a hash of four byte
            // 2. in lastHash is the location of the most recent block of four byte with that same hash
            // 3. due to hash collisions, several groups of four bytes may yield the same hash
            // 4. so for each location I can look up the previous location of the same hash in previousHash
            // 5. basically it's a chain of memory locations where potential matches start
            // 5. I follow this hash chain until I find exactly the same four bytes I was looking for
            // 6. then I switch to a sparser chain: previousExact
            // 7. it's basically the same idea as previousHash but this time not the hash but the first four bytes must be identical
            // 8. previousExact will be used by findLongestMatch: it compare all such strings a figures out which is the longest match

            // And why do I have to do it in such a complicated way ?
            // - well, there are 2^32 combinations of four bytes
            // - so that there are 2^32 potential chains
            // - most combinations just don't occur and occupy no space but I still have to keep their "entry point" (which are empty/invalid)
            // - that would be at least 16 GBytes RAM (2^32 x 4 bytes)
            // - my hashing algorithm reduces the 2^32 combinations to 2^20 hashes (see hashBits), that's about 8 MBytes RAM
            // - thus only 2^20 entry points and at most 2^20 hash chains which is easily manageable
            // ... in the end it's all about conserving memory !
            // (total memory consumption of smallz4 is about 64 MBytes)

            // first and last offset of a block (nextBlock is end-of-block plus 1)
            ulong lastBlock = 0;
            ulong nextBlock = 0;
            bool parseDictionary = dictionary.Count > 0;

            // main loop, processes one block per iteration
            while (true)
            {
                // ==================== start new block ====================
                // first byte of the currently processed block (std::vector data may contain the last 64k of the previous block, too)
                uint dataBlock;

                // prepend dictionary
                if (parseDictionary)
                {
                    // resize dictionary to 64k (minus 1 because we can only match the last 65535 bytes of the dictionary => MaxDistance)
                    if (dictionary.Count < MaxDistance)
                    {
                        // dictionary is smaller than 64k, prepend garbage data
                        uint unused = (uint)(MaxDistance - dictionary.Count);
                        Resize(data, unused, 0);
                        data.AddRange(dictionary);
                    }
                    else
                    {
                        // copy only the most recent 64k of the dictionary
                        byte[] temp = new byte[MaxDistance];
                        Array.Copy(dictionary.ToArray(), dictionary.Count - MaxDistance, temp, 0, MaxDistance);
                        data.AddRange(temp);
                    }

                    nextBlock = (ulong)data.Count;
                    numRead = (uint)data.Count;
                }

                // read more bytes from input
                uint maxBlockSize = (uint)(useLegacyFormat ? MaxBlockSizeLegacy : MaxBlockSize);
                while (numRead - nextBlock<maxBlockSize)
                {
                    // buffer can be significantly smaller than MaxBlockSize, that's the only reason for this while-block
                    uint incoming = (uint)getBytes.BaseStream.Read(buffer, 0, BufferSize);
                    // no more data ?
                    if (incoming == 0)
                        break;

                    // add bytes to buffer
                    numRead += incoming;
                    byte[] temp = new byte[incoming];
                    Array.Copy(buffer, 0, temp, 0, incoming);
                    data.AddRange(temp);
                }

                // no more data ? => WE'RE DONE !
                if (nextBlock == numRead)
                    break;

                // determine block borders
                lastBlock  = nextBlock;
                nextBlock += maxBlockSize;
                // not beyond end-of-file
                if (nextBlock > numRead)
                    nextBlock = numRead;

                // pointer to first byte of the currently processed block (the std::vector container named data may contain the last 64k of the previous block, too)
                dataBlock = (uint)(lastBlock - dataZero);

                ulong blockSize = nextBlock - lastBlock;

                // ==================== full match finder ====================

                // greedy mode is much faster but produces larger output
                bool isGreedy = (maxChainLength <= ShortChainsGreedy);
                // lazy evaluation: if there is a match, then try running match finder on next position, too, but not after that
                bool isLazy = !isGreedy && (maxChainLength <= ShortChainsLazy);
                // skip match finding on the next x bytes in greedy mode
                ulong skipMatches = 0;
                // allow match finding on the next byte but skip afterwards (in lazy mode)
                bool lazyEvaluation = false;

                // the last literals of the previous block skipped matching, so they are missing from the hash chains
                long lookback = (long)dataZero;
                if (lookback > BlockEndNoMatch && !parseDictionary)
                    lookback = BlockEndNoMatch;
                if (parseDictionary)
                    lookback = (long)dictionary.Count;
                // so let's go back a few bytes
                lookback = -lookback;
                // ... but not in legacy mode
                if (useLegacyFormat || uncompressed)
                    lookback = 0;

                int matchesSize = uncompressed ? 0 : (int)blockSize;
                List<Match> matches = new List<Match>(matchesSize);
                for (int j = 0; j < matchesSize; j++)
                    matches.Add(new Match());
                // find longest matches for each position (skip if level=0 which means "uncompressed")
                long i;
                for (i = lookback; i + BlockEndNoMatch <= (long)blockSize && !uncompressed; i++)
                {
                    // detect self-matching
                    if (i > 0 && data[(int)(dataBlock + i)] == data[(int)(dataBlock + i - 1)])
                    {
                        Match prevMatch = matches[(int)(i - 1)];
                        // predecessor had the same match ?
                        if (prevMatch.distance == 1 && prevMatch.length > MaxSameLetter) // TODO: handle very long self-referencing matches
                        {
                            // just copy predecessor without further (expensive) optimizations
                            Match match = matches[(int)i];
                            match.distance = 1;
                            match.length = prevMatch.length - 1;
                            matches[(int)i] = match;
                            continue;
                        }
                    }

                    // read next four bytes
                    uint four = ToUInt32List(data, (int)(dataBlock + i));
                    // convert to a shorter hash
                    uint hash = GetHash32(four);

                    // get most recent position of this hash
                    ulong lastHashMatch = lastHash[(int)hash];
                    // and store current position
                    lastHash[(int)hash] = (ulong)i + lastBlock;

                    // remember: i could be negative, too
                    ushort prevIndex = (ushort)((int)(i + (long)MaxDistance + 1) & MaxDistance); // actually the same as i & MaxDistance

                    // no predecessor / no hash chain available ?
                    if (lastHashMatch == NoLastHash)
                    {
                        previousHash[prevIndex] = EndOfChain;
                        previousExact[prevIndex] = EndOfChain;
                        continue;
                    }

                    // most recent hash match too far away ?
                    ulong distance = lastHash[(int)hash] - lastHashMatch;
                    if (distance > MaxDistance)
                    {
                        previousHash[prevIndex] = EndOfChain;
                        previousExact[prevIndex] = EndOfChain;
                        continue;
                    }

                    // build hash chain, i.e. store distance to last pseudo-match
                    previousHash[prevIndex] = (ushort)distance;

                    // skip pseudo-matches (hash collisions) and build a second chain where the first four bytes must match exactly
                    uint currentFour;
                    // check the hash chain
                    while (true)
                    {
                        // read four bytes
                        currentFour = ToUInt32List(data, (int)(lastHashMatch - dataZero)); // match may be found in the previous block, too
                        // match chain found, first 4 bytes are identical
                        if (currentFour == four)
                            break;

                        // prevent from accidently hopping on an old, wrong hash chain
                        if (hash != GetHash32(currentFour))
                        break;

                        // try next pseudo-match
                        ushort next = previousHash[(int)(lastHashMatch & MaxDistance)];
                        // end of the hash chain ?
                        if (next == EndOfChain)
                            break;

                        // too far away ?
                        distance += next;
                        if (distance > MaxDistance)
                            break;

                        // take another step along the hash chain ...
                        lastHashMatch -= next;
                        // closest match is out of range ?
                        if (lastHashMatch<dataZero)
                            break;
                    }

                    // search aborted / failed ?
                    if (four != currentFour)
                    {
                        // no matches for the first four bytes
                        previousExact[prevIndex] = EndOfChain;
                        continue;
                    }

                    // store distance to previous match
                    previousExact[prevIndex] = (ushort)distance;

                    // no matching if crossing block boundary, just update hash tables
                    if (i < 0)
                        continue;

                    // skip match finding if in greedy mode
                    if (skipMatches > 0)
                    {
                        skipMatches--;
                        if (!lazyEvaluation)
                            continue;
                        lazyEvaluation = false;
                    }

                    // and after all that preparation ... finally look for the longest match
                    matches[(int)i] = FindLongestMatch(data.ToArray(), (ulong)i + lastBlock, dataZero, nextBlock - BlockEndLiterals, previousExact);

                    // no match finding needed for the next few bytes in greedy/lazy mode
                    if ((isLazy || isGreedy) && matches[(int)i].length != JustLiteral)
                    {
                        lazyEvaluation = (skipMatches == 0);
                        skipMatches = matches[(int)i].length;
                    }
                }
                // last bytes are always literals
                while (i < matches.Count)
                {
                    Match match = matches[(int)i];
                    match.length = JustLiteral;
                    matches[(int)i] = match;
                    i++;
                }

                // dictionary is valid only to the first block
                parseDictionary = false;

                // ==================== estimate costs (number of compressed bytes) ====================

                // not needed in greedy mode and/or very short blocks
                if (matches.Count > BlockEndNoMatch && maxChainLength > ShortChainsGreedy)
                    EstimateCosts(matches);

                // ==================== select best matches ====================

                List<byte> compressed = SelectBestMatches(matches, data.ToArray(), (uint)lastBlock - dataZero);

                // ==================== output ====================

                // did compression do harm ?
                bool useCompression = (ulong)compressed.Count < blockSize && !uncompressed;
                // legacy format is always compressed
                useCompression |= useLegacyFormat;

                // block size
                uint numBytes = (uint)(useCompression ? (ulong)compressed.Count : blockSize);
                uint numBytesTagged = numBytes | (useCompression ? 0 : 0x80000000);
                sendBytes.Write(numBytesTagged);

                byte[] buf1 = new byte[numBytes];

                if (useCompression)
                {
                    Array.Copy(compressed.ToArray(), 0, buf1, 0, numBytes);
                    sendBytes.Write(buf1);
                }
                else // uncompressed ? => copy input data
                {
                    //todo wat to do here
                    Array.Copy(data.ToArray(), (uint)lastBlock - dataZero, buf1, 0, numBytes);
                    sendBytes.Write(buf1);
                }

                // legacy format: no matching across blocks
                if (useLegacyFormat)
                {
                    dataZero += (uint)data.Count;
                    data.Clear();

                    // clear hash tables
                    for (uint j = 0; j < previousHash.Count; j++)
                        previousHash[(int)j] = EndOfChain;
                    for (uint j = 0; j < previousExact.Count; j++)
                        previousExact[(int)j] = EndOfChain;
                    for (uint j = 0; j < lastHash.Count; j++)
                        lastHash[(int)j] = NoLastHash;
                }
                else
                {
                    // remove already processed data except for the last 64kb which could be used for intra-block matches
                    if (data.Count > MaxDistance)
                    {
                        uint remove = (uint)(data.Count - MaxDistance);
                        dataZero += remove;
                        data.RemoveRange(0, (int)remove);
                    }
                }
            }

            // add an empty block
            if (!useLegacyFormat)
            {
                sendBytes.Write(0);
            }
        }
    }
}
