/*
Copyright (c) 2015 Harm Hanemaaijer <fgenfb@yahoo.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

namespace AssetsTools.NET.Extra
{
	internal class BitReader
	{
		public int index;
		public ulong data0;
		public ulong data1;
		public void Reset(ulong data0, ulong data1)
		{
			index = 0;
			this.data0 = data0;
			this.data1 = data1;
		}
		public int ReadNumber(int bitCount)
		{
			ulong value = 0;
			for (int i = 0; i < bitCount; i++)
			{
				if (index < 64)
				{
					int shift = index - i;
					if (shift < 0)
						value |= (data0 & ((ulong)1 << index)) << (-shift);
					else
						value |= (data0 & ((ulong)1 << index)) >> shift;
				}
				else
				{
					int shift = (index - 64) - i;
					if (shift < 0)
						value |= (data1 & ((ulong)1 << (index - 64))) << (-shift);
					else
						value |= (data1 & ((ulong)1 << (index - 64))) >> shift;
				}
				index++;
			}
			return (int)value;
		}
	}
}
