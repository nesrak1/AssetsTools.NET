using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetsTools.NET.Texture
{
    // One PS5 resource contained an extra repeated block, but its texture offsets
    // did not include it. Reading those offsets made later textures look broken.
    // Skip that block in memory only when the references cover the whole resource
    // and there is exactly one matching boundary. This is a file workaround,
    // not a PS5 swizzle rule. Repeated data alone cannot prove an offset is wrong.
    public static class Ps5ResourceCompatibility
    {
        public static byte[] TryFillPictureData(TextureFile texture, AssetsFileInstance file, AssetsManager manager)
        {
            if (file.file.Metadata.TargetPlatform != 44 || file.parentBundle != null
                || !texture.m_IsPreProcessed || texture.m_TextureDimension != 2 || texture.m_ImageCount != 1
                || texture.m_StreamingMipmaps || texture.m_StreamData.offset > long.MaxValue
                || string.IsNullOrEmpty(texture.m_StreamData.path) || texture.m_StreamData.size == 0)
                return null;
            if (texture.pictureData != null && texture.pictureData.Length > 0)
                return texture.pictureData;

            string path = ResolvePath(file, texture.m_StreamData.path);
            if (!File.Exists(path))
                return null;

            var ranges = new List<(long Offset, long Size)>();
            lock (file.LockReader)
            {
                foreach (var info in file.file.GetAssetsOfType(AssetClassID.Texture2D)
                    .Concat(file.file.GetAssetsOfType(AssetClassID.Cubemap)))
                {
                    var other = TextureFile.ReadTextureFile(manager.GetBaseField(file, info));
                    if (other.m_StreamData.size == 0 || string.IsNullOrEmpty(other.m_StreamData.path)
                        || !string.Equals(ResolvePath(file, other.m_StreamData.path), path, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!other.m_IsPreProcessed || other.m_TextureDimension != 2 || other.m_ImageCount != 1
                        || other.m_StreamingMipmaps || other.m_StreamData.offset > long.MaxValue)
                        return null;
                    try
                    {
                        // Unknown layouts cannot be used to check the stored ranges.
                        Ps5MipChain.ForUnity(other.m_Width, other.m_Height, (TextureFormat)other.m_TextureFormat,
                            other.m_MipCount, checked((int)other.m_StreamData.size));
                    }
                    catch (Exception ex) when (ex is NotSupportedException || ex is InvalidDataException
                        || ex is ArgumentException || ex is OverflowException)
                    {
                        return null;
                    }
                    ranges.Add(((long)other.m_StreamData.offset, other.m_StreamData.size));
                }
            }

            long offset = (long)texture.m_StreamData.offset;
            if (!ranges.Contains((offset, texture.m_StreamData.size)))
                return null;

            using var stream = File.OpenRead(path);
            if (!TryDetectDuplicate(stream, ranges, out long boundary, out long extra))
                return null;

            stream.Position = checked(offset + (offset >= boundary ? extra : 0));
            var data = new byte[texture.m_StreamData.size];
            ReadExactly(stream, data);
            texture.pictureData = data;
            return data;
        }

        private static string ResolvePath(AssetsFileInstance file, string path)
        {
            return Path.GetFullPath(Path.IsPathRooted(path)
                ? path : Path.Combine(Path.GetDirectoryName(file.path), path));
        }

        // Look for one repeated block at a texture boundary. Gaps, overlaps,
        // uniform padding and multiple matches leave the original offsets alone.
        public static bool TryDetectDuplicate(Stream stream, IEnumerable<(long Offset, long Size)> records,
            out long boundary, out long extra)
        {
            boundary = 0;
            extra = 0;
            if (!stream.CanRead || !stream.CanSeek)
                return false;
            var ranges = records.Distinct().OrderBy(r => r.Offset).ToArray();
            if (ranges.Length < 2)
                return false;

            long end = 0;
            foreach (var range in ranges)
            {
                if (range.Offset != end || range.Size <= 0 || range.Size > long.MaxValue - end)
                    return false;
                end += range.Size;
            }
            long excess = stream.Length - end;
            // Limit the scan to small, aligned blocks.
            if (excess < 256 || excess > 16 * 1024 * 1024 || excess % 256 != 0)
                return false;

            long position = stream.Position;
            try
            {
                var left = new byte[(int)excess];
                var right = new byte[(int)excess];
                long candidate = -1;
                for (int i = 1; i < ranges.Length; i++)
                {
                    long offset = ranges[i].Offset;
                    if (ranges[i - 1].Size < excess)
                        continue;
                    stream.Position = offset - excess;
                    ReadExactly(stream, left);
                    ReadExactly(stream, right);
                    if (!left.AsSpan().SequenceEqual(right) || left.All(b => b == left[0]))
                        continue;
                    if (candidate >= 0)
                        return false;
                    candidate = offset;
                }
                if (candidate < 0)
                    return false;
                boundary = candidate;
                extra = excess;
                return true;
            }
            finally
            {
                stream.Position = position;
            }
        }

        private static void ReadExactly(Stream stream, byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                int read = stream.Read(data, offset, data.Length - offset);
                if (read == 0)
                    throw new EndOfStreamException();
                offset += read;
            }
        }
    }
}
