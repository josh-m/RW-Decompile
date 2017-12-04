using System;

namespace Ionic.Zlib
{
	internal sealed class InflateCodes
	{
		private const int START = 0;

		private const int LEN = 1;

		private const int LENEXT = 2;

		private const int DIST = 3;

		private const int DISTEXT = 4;

		private const int COPY = 5;

		private const int LIT = 6;

		private const int WASH = 7;

		private const int END = 8;

		private const int BADCODE = 9;

		internal int mode;

		internal int len;

		internal int[] tree;

		internal int tree_index;

		internal int need;

		internal int lit;

		internal int bitsToGet;

		internal int dist;

		internal byte lbits;

		internal byte dbits;

		internal int[] ltree;

		internal int ltree_index;

		internal int[] dtree;

		internal int dtree_index;

		internal InflateCodes()
		{
		}

		internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
		{
			this.mode = 0;
			this.lbits = (byte)bl;
			this.dbits = (byte)bd;
			this.ltree = tl;
			this.ltree_index = tl_index;
			this.dtree = td;
			this.dtree_index = td_index;
			this.tree = null;
		}

		internal int Process(InflateBlocks blocks, int r)
		{
			ZlibCodec codec = blocks._codec;
			int num = codec.NextIn;
			int num2 = codec.AvailableBytesIn;
			int num3 = blocks.bitb;
			int i = blocks.bitk;
			int num4 = blocks.writeAt;
			int num5 = (num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1);
			while (true)
			{
				int num6;
				switch (this.mode)
				{
				case 0:
					if (num5 >= 258 && num2 >= 10)
					{
						blocks.bitb = num3;
						blocks.bitk = i;
						codec.AvailableBytesIn = num2;
						codec.TotalBytesIn += (long)(num - codec.NextIn);
						codec.NextIn = num;
						blocks.writeAt = num4;
						r = this.InflateFast((int)this.lbits, (int)this.dbits, this.ltree, this.ltree_index, this.dtree, this.dtree_index, blocks, codec);
						num = codec.NextIn;
						num2 = codec.AvailableBytesIn;
						num3 = blocks.bitb;
						i = blocks.bitk;
						num4 = blocks.writeAt;
						num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
						if (r != 0)
						{
							this.mode = ((r != 1) ? 9 : 7);
							continue;
						}
					}
					this.need = (int)this.lbits;
					this.tree = this.ltree;
					this.tree_index = this.ltree_index;
					this.mode = 1;
					goto IL_1C4;
				case 1:
					goto IL_1C4;
				case 2:
					num6 = this.bitsToGet;
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_3A2;
						}
						r = 0;
						num2--;
						num3 |= (int)(codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					this.len += (num3 & InternalInflateConstants.InflateMask[num6]);
					num3 >>= num6;
					i -= num6;
					this.need = (int)this.dbits;
					this.tree = this.dtree;
					this.tree_index = this.dtree_index;
					this.mode = 3;
					goto IL_471;
				case 3:
					goto IL_471;
				case 4:
					num6 = this.bitsToGet;
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_618;
						}
						r = 0;
						num2--;
						num3 |= (int)(codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					this.dist += (num3 & InternalInflateConstants.InflateMask[num6]);
					num3 >>= num6;
					i -= num6;
					this.mode = 5;
					goto IL_6C3;
				case 5:
					goto IL_6C3;
				case 6:
					if (num5 == 0)
					{
						if (num4 == blocks.end && blocks.readAt != 0)
						{
							num4 = 0;
							num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
						}
						if (num5 == 0)
						{
							blocks.writeAt = num4;
							r = blocks.Flush(r);
							num4 = blocks.writeAt;
							num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
							if (num4 == blocks.end && blocks.readAt != 0)
							{
								num4 = 0;
								num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
							}
							if (num5 == 0)
							{
								goto Block_44;
							}
						}
					}
					r = 0;
					blocks.window[num4++] = (byte)this.lit;
					num5--;
					this.mode = 0;
					continue;
				case 7:
					goto IL_9B8;
				case 8:
					goto IL_A7A;
				case 9:
					goto IL_AC7;
				}
				break;
				continue;
				IL_1C4:
				num6 = this.need;
				while (i < num6)
				{
					if (num2 == 0)
					{
						goto IL_1DF;
					}
					r = 0;
					num2--;
					num3 |= (int)(codec.InputBuffer[num++] & 255) << i;
					i += 8;
				}
				int num7 = (this.tree_index + (num3 & InternalInflateConstants.InflateMask[num6])) * 3;
				num3 >>= this.tree[num7 + 1];
				i -= this.tree[num7 + 1];
				int num8 = this.tree[num7];
				if (num8 == 0)
				{
					this.lit = this.tree[num7 + 2];
					this.mode = 6;
					continue;
				}
				if ((num8 & 16) != 0)
				{
					this.bitsToGet = (num8 & 15);
					this.len = this.tree[num7 + 2];
					this.mode = 2;
					continue;
				}
				if ((num8 & 64) == 0)
				{
					this.need = num8;
					this.tree_index = num7 / 3 + this.tree[num7 + 2];
					continue;
				}
				if ((num8 & 32) != 0)
				{
					this.mode = 7;
					continue;
				}
				goto IL_325;
				IL_471:
				num6 = this.need;
				while (i < num6)
				{
					if (num2 == 0)
					{
						goto IL_48C;
					}
					r = 0;
					num2--;
					num3 |= (int)(codec.InputBuffer[num++] & 255) << i;
					i += 8;
				}
				num7 = (this.tree_index + (num3 & InternalInflateConstants.InflateMask[num6])) * 3;
				num3 >>= this.tree[num7 + 1];
				i -= this.tree[num7 + 1];
				num8 = this.tree[num7];
				if ((num8 & 16) != 0)
				{
					this.bitsToGet = (num8 & 15);
					this.dist = this.tree[num7 + 2];
					this.mode = 4;
					continue;
				}
				if ((num8 & 64) == 0)
				{
					this.need = num8;
					this.tree_index = num7 / 3 + this.tree[num7 + 2];
					continue;
				}
				goto IL_59B;
				IL_6C3:
				int j;
				for (j = num4 - this.dist; j < 0; j += blocks.end)
				{
				}
				while (this.len != 0)
				{
					if (num5 == 0)
					{
						if (num4 == blocks.end && blocks.readAt != 0)
						{
							num4 = 0;
							num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
						}
						if (num5 == 0)
						{
							blocks.writeAt = num4;
							r = blocks.Flush(r);
							num4 = blocks.writeAt;
							num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
							if (num4 == blocks.end && blocks.readAt != 0)
							{
								num4 = 0;
								num5 = ((num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1));
							}
							if (num5 == 0)
							{
								goto Block_32;
							}
						}
					}
					blocks.window[num4++] = blocks.window[j++];
					num5--;
					if (j == blocks.end)
					{
						j = 0;
					}
					this.len--;
				}
				this.mode = 0;
			}
			r = -2;
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_1DF:
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_325:
			this.mode = 9;
			codec.Message = "invalid literal/length code";
			r = -3;
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_3A2:
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_48C:
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_59B:
			this.mode = 9;
			codec.Message = "invalid distance code";
			r = -3;
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_618:
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			Block_32:
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			Block_44:
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_9B8:
			if (i > 7)
			{
				i -= 8;
				num2++;
				num--;
			}
			blocks.writeAt = num4;
			r = blocks.Flush(r);
			num4 = blocks.writeAt;
			int arg_A11_0 = (num4 >= blocks.readAt) ? (blocks.end - num4) : (blocks.readAt - num4 - 1);
			if (blocks.readAt != blocks.writeAt)
			{
				blocks.bitb = num3;
				blocks.bitk = i;
				codec.AvailableBytesIn = num2;
				codec.TotalBytesIn += (long)(num - codec.NextIn);
				codec.NextIn = num;
				blocks.writeAt = num4;
				return blocks.Flush(r);
			}
			this.mode = 8;
			IL_A7A:
			r = 1;
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
			IL_AC7:
			r = -3;
			blocks.bitb = num3;
			blocks.bitk = i;
			codec.AvailableBytesIn = num2;
			codec.TotalBytesIn += (long)(num - codec.NextIn);
			codec.NextIn = num;
			blocks.writeAt = num4;
			return blocks.Flush(r);
		}

		internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
		{
			int num = z.NextIn;
			int num2 = z.AvailableBytesIn;
			int num3 = s.bitb;
			int i = s.bitk;
			int num4 = s.writeAt;
			int num5 = (num4 >= s.readAt) ? (s.end - num4) : (s.readAt - num4 - 1);
			int num6 = InternalInflateConstants.InflateMask[bl];
			int num7 = InternalInflateConstants.InflateMask[bd];
			int num10;
			int num11;
			while (true)
			{
				while (i < 20)
				{
					num2--;
					num3 |= (int)(z.InputBuffer[num++] & 255) << i;
					i += 8;
				}
				int num8 = num3 & num6;
				int num9 = (tl_index + num8) * 3;
				if ((num10 = tl[num9]) == 0)
				{
					num3 >>= tl[num9 + 1];
					i -= tl[num9 + 1];
					s.window[num4++] = (byte)tl[num9 + 2];
					num5--;
				}
				else
				{
					while (true)
					{
						num3 >>= tl[num9 + 1];
						i -= tl[num9 + 1];
						if ((num10 & 16) != 0)
						{
							break;
						}
						if ((num10 & 64) != 0)
						{
							goto IL_50A;
						}
						num8 += tl[num9 + 2];
						num8 += (num3 & InternalInflateConstants.InflateMask[num10]);
						num9 = (tl_index + num8) * 3;
						if ((num10 = tl[num9]) == 0)
						{
							goto Block_20;
						}
					}
					num10 &= 15;
					num11 = tl[num9 + 2] + (num3 & InternalInflateConstants.InflateMask[num10]);
					num3 >>= num10;
					for (i -= num10; i < 15; i += 8)
					{
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & 255) << i;
					}
					num8 = (num3 & num7);
					num9 = (td_index + num8) * 3;
					num10 = td[num9];
					while (true)
					{
						num3 >>= td[num9 + 1];
						i -= td[num9 + 1];
						if ((num10 & 16) != 0)
						{
							break;
						}
						if ((num10 & 64) != 0)
						{
							goto IL_400;
						}
						num8 += td[num9 + 2];
						num8 += (num3 & InternalInflateConstants.InflateMask[num10]);
						num9 = (td_index + num8) * 3;
						num10 = td[num9];
					}
					num10 &= 15;
					while (i < num10)
					{
						num2--;
						num3 |= (int)(z.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					int num12 = td[num9 + 2] + (num3 & InternalInflateConstants.InflateMask[num10]);
					num3 >>= num10;
					i -= num10;
					num5 -= num11;
					int num13;
					if (num4 >= num12)
					{
						num13 = num4 - num12;
						if (num4 - num13 > 0 && 2 > num4 - num13)
						{
							s.window[num4++] = s.window[num13++];
							s.window[num4++] = s.window[num13++];
							num11 -= 2;
						}
						else
						{
							Array.Copy(s.window, num13, s.window, num4, 2);
							num4 += 2;
							num13 += 2;
							num11 -= 2;
						}
					}
					else
					{
						num13 = num4 - num12;
						do
						{
							num13 += s.end;
						}
						while (num13 < 0);
						num10 = s.end - num13;
						if (num11 > num10)
						{
							num11 -= num10;
							if (num4 - num13 > 0 && num10 > num4 - num13)
							{
								do
								{
									s.window[num4++] = s.window[num13++];
								}
								while (--num10 != 0);
							}
							else
							{
								Array.Copy(s.window, num13, s.window, num4, num10);
								num4 += num10;
								num13 += num10;
							}
							num13 = 0;
						}
					}
					if (num4 - num13 > 0 && num11 > num4 - num13)
					{
						do
						{
							s.window[num4++] = s.window[num13++];
						}
						while (--num11 != 0);
					}
					else
					{
						Array.Copy(s.window, num13, s.window, num4, num11);
						num4 += num11;
						num13 += num11;
					}
					goto IL_62B;
					Block_20:
					num3 >>= tl[num9 + 1];
					i -= tl[num9 + 1];
					s.window[num4++] = (byte)tl[num9 + 2];
					num5--;
				}
				IL_62B:
				if (num5 < 258 || num2 < 10)
				{
					goto IL_640;
				}
			}
			IL_400:
			z.Message = "invalid distance code";
			num11 = z.AvailableBytesIn - num2;
			num11 = ((i >> 3 >= num11) ? num11 : (i >> 3));
			num2 += num11;
			num -= num11;
			i -= num11 << 3;
			s.bitb = num3;
			s.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			s.writeAt = num4;
			return -3;
			IL_50A:
			if ((num10 & 32) != 0)
			{
				num11 = z.AvailableBytesIn - num2;
				num11 = ((i >> 3 >= num11) ? num11 : (i >> 3));
				num2 += num11;
				num -= num11;
				i -= num11 << 3;
				s.bitb = num3;
				s.bitk = i;
				z.AvailableBytesIn = num2;
				z.TotalBytesIn += (long)(num - z.NextIn);
				z.NextIn = num;
				s.writeAt = num4;
				return 1;
			}
			z.Message = "invalid literal/length code";
			num11 = z.AvailableBytesIn - num2;
			num11 = ((i >> 3 >= num11) ? num11 : (i >> 3));
			num2 += num11;
			num -= num11;
			i -= num11 << 3;
			s.bitb = num3;
			s.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			s.writeAt = num4;
			return -3;
			IL_640:
			num11 = z.AvailableBytesIn - num2;
			num11 = ((i >> 3 >= num11) ? num11 : (i >> 3));
			num2 += num11;
			num -= num11;
			i -= num11 << 3;
			s.bitb = num3;
			s.bitk = i;
			z.AvailableBytesIn = num2;
			z.TotalBytesIn += (long)(num - z.NextIn);
			z.NextIn = num;
			s.writeAt = num4;
			return 0;
		}
	}
}
