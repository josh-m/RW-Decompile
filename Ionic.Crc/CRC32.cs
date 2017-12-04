using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ionic.Crc
{
	[ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
	public class CRC32
	{
		private uint dwPolynomial;

		private long _TotalBytesRead;

		private bool reverseBits;

		private uint[] crc32Table;

		private const int BUFFER_SIZE = 8192;

		private uint _register = 4294967295u;

		public long TotalBytesRead
		{
			get
			{
				return this._TotalBytesRead;
			}
		}

		public int Crc32Result
		{
			get
			{
				return (int)(~(int)this._register);
			}
		}

		public CRC32() : this(false)
		{
		}

		public CRC32(bool reverseBits) : this(-306674912, reverseBits)
		{
		}

		public CRC32(int polynomial, bool reverseBits)
		{
			this.reverseBits = reverseBits;
			this.dwPolynomial = (uint)polynomial;
			this.GenerateLookupTable();
		}

		public int GetCrc32(Stream input)
		{
			return this.GetCrc32AndCopy(input, null);
		}

		public int GetCrc32AndCopy(Stream input, Stream output)
		{
			if (input == null)
			{
				throw new Exception("The input stream must not be null.");
			}
			byte[] array = new byte[8192];
			int count = 8192;
			this._TotalBytesRead = 0L;
			int i = input.Read(array, 0, count);
			if (output != null)
			{
				output.Write(array, 0, i);
			}
			this._TotalBytesRead += (long)i;
			while (i > 0)
			{
				this.SlurpBlock(array, 0, i);
				i = input.Read(array, 0, count);
				if (output != null)
				{
					output.Write(array, 0, i);
				}
				this._TotalBytesRead += (long)i;
			}
			return (int)(~(int)this._register);
		}

		public int ComputeCrc32(int W, byte B)
		{
			return this._InternalComputeCrc32((uint)W, B);
		}

		internal int _InternalComputeCrc32(uint W, byte B)
		{
			return (int)(this.crc32Table[(int)((UIntPtr)((W ^ (uint)B) & 255u))] ^ W >> 8);
		}

		public void SlurpBlock(byte[] block, int offset, int count)
		{
			if (block == null)
			{
				throw new Exception("The data buffer must not be null.");
			}
			for (int i = 0; i < count; i++)
			{
				int num = offset + i;
				byte b = block[num];
				if (this.reverseBits)
				{
					uint num2 = this._register >> 24 ^ (uint)b;
					this._register = (this._register << 8 ^ this.crc32Table[(int)((UIntPtr)num2)]);
				}
				else
				{
					uint num3 = (this._register & 255u) ^ (uint)b;
					this._register = (this._register >> 8 ^ this.crc32Table[(int)((UIntPtr)num3)]);
				}
			}
			this._TotalBytesRead += (long)count;
		}

		public void UpdateCRC(byte b)
		{
			if (this.reverseBits)
			{
				uint num = this._register >> 24 ^ (uint)b;
				this._register = (this._register << 8 ^ this.crc32Table[(int)((UIntPtr)num)]);
			}
			else
			{
				uint num2 = (this._register & 255u) ^ (uint)b;
				this._register = (this._register >> 8 ^ this.crc32Table[(int)((UIntPtr)num2)]);
			}
		}

		public void UpdateCRC(byte b, int n)
		{
			while (n-- > 0)
			{
				if (this.reverseBits)
				{
					uint num = this._register >> 24 ^ (uint)b;
					this._register = (this._register << 8 ^ this.crc32Table[(int)((UIntPtr)((num < 0u) ? (num + 256u) : num))]);
				}
				else
				{
					uint num2 = (this._register & 255u) ^ (uint)b;
					this._register = (this._register >> 8 ^ this.crc32Table[(int)((UIntPtr)((num2 < 0u) ? (num2 + 256u) : num2))]);
				}
			}
		}

		private static uint ReverseBits(uint data)
		{
			uint num = (data & 1431655765u) << 1 | (data >> 1 & 1431655765u);
			num = ((num & 858993459u) << 2 | (num >> 2 & 858993459u));
			num = ((num & 252645135u) << 4 | (num >> 4 & 252645135u));
			return num << 24 | (num & 65280u) << 8 | (num >> 8 & 65280u) | num >> 24;
		}

		private static byte ReverseBits(byte data)
		{
			uint num = (uint)data * 131586u;
			uint num2 = 17055760u;
			uint num3 = num & num2;
			uint num4 = num << 2 & num2 << 1;
			return (byte)(16781313u * (num3 + num4) >> 24);
		}

		private void GenerateLookupTable()
		{
			this.crc32Table = new uint[256];
			byte b = 0;
			do
			{
				uint num = (uint)b;
				for (byte b2 = 8; b2 > 0; b2 -= 1)
				{
					if ((num & 1u) == 1u)
					{
						num = (num >> 1 ^ this.dwPolynomial);
					}
					else
					{
						num >>= 1;
					}
				}
				if (this.reverseBits)
				{
					this.crc32Table[(int)CRC32.ReverseBits(b)] = CRC32.ReverseBits(num);
				}
				else
				{
					this.crc32Table[(int)b] = num;
				}
				b += 1;
			}
			while (b != 0);
		}

		private uint gf2_matrix_times(uint[] matrix, uint vec)
		{
			uint num = 0u;
			int num2 = 0;
			while (vec != 0u)
			{
				if ((vec & 1u) == 1u)
				{
					num ^= matrix[num2];
				}
				vec >>= 1;
				num2++;
			}
			return num;
		}

		private void gf2_matrix_square(uint[] square, uint[] mat)
		{
			for (int i = 0; i < 32; i++)
			{
				square[i] = this.gf2_matrix_times(mat, mat[i]);
			}
		}

		public void Combine(int crc, int length)
		{
			uint[] array = new uint[32];
			uint[] array2 = new uint[32];
			if (length == 0)
			{
				return;
			}
			uint num = ~this._register;
			array2[0] = this.dwPolynomial;
			uint num2 = 1u;
			for (int i = 1; i < 32; i++)
			{
				array2[i] = num2;
				num2 <<= 1;
			}
			this.gf2_matrix_square(array, array2);
			this.gf2_matrix_square(array2, array);
			uint num3 = (uint)length;
			do
			{
				this.gf2_matrix_square(array, array2);
				if ((num3 & 1u) == 1u)
				{
					num = this.gf2_matrix_times(array, num);
				}
				num3 >>= 1;
				if (num3 == 0u)
				{
					break;
				}
				this.gf2_matrix_square(array2, array);
				if ((num3 & 1u) == 1u)
				{
					num = this.gf2_matrix_times(array2, num);
				}
				num3 >>= 1;
			}
			while (num3 != 0u);
			num ^= (uint)crc;
			this._register = ~num;
		}

		public void Reset()
		{
			this._register = 4294967295u;
		}
	}
}
