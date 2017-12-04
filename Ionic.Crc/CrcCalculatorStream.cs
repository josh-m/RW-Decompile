using System;
using System.IO;

namespace Ionic.Crc
{
	public class CrcCalculatorStream : Stream, IDisposable
	{
		private static readonly long UnsetLengthLimit = -99L;

		private readonly Stream _innerStream;

		private readonly CRC32 _crc32;

		private readonly long _lengthLimit = -99L;

		private bool _leaveOpen;

		public long TotalBytesSlurped
		{
			get
			{
				return this._crc32.TotalBytesRead;
			}
		}

		public int Crc
		{
			get
			{
				return this._crc32.Crc32Result;
			}
		}

		public bool LeaveOpen
		{
			get
			{
				return this._leaveOpen;
			}
			set
			{
				this._leaveOpen = value;
			}
		}

		public override bool CanRead
		{
			get
			{
				return this._innerStream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this._innerStream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				if (this._lengthLimit == CrcCalculatorStream.UnsetLengthLimit)
				{
					return this._innerStream.Length;
				}
				return this._lengthLimit;
			}
		}

		public override long Position
		{
			get
			{
				return this._crc32.TotalBytesRead;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public CrcCalculatorStream(Stream stream) : this(true, CrcCalculatorStream.UnsetLengthLimit, stream, null)
		{
		}

		public CrcCalculatorStream(Stream stream, bool leaveOpen) : this(leaveOpen, CrcCalculatorStream.UnsetLengthLimit, stream, null)
		{
		}

		public CrcCalculatorStream(Stream stream, long length) : this(true, length, stream, null)
		{
			if (length < 0L)
			{
				throw new ArgumentException("length");
			}
		}

		public CrcCalculatorStream(Stream stream, long length, bool leaveOpen) : this(leaveOpen, length, stream, null)
		{
			if (length < 0L)
			{
				throw new ArgumentException("length");
			}
		}

		public CrcCalculatorStream(Stream stream, long length, bool leaveOpen, CRC32 crc32) : this(leaveOpen, length, stream, crc32)
		{
			if (length < 0L)
			{
				throw new ArgumentException("length");
			}
		}

		private CrcCalculatorStream(bool leaveOpen, long length, Stream stream, CRC32 crc32)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			this._innerStream = stream;
			this._crc32 = (crc32 ?? new CRC32());
			this._lengthLimit = length;
			this._leaveOpen = leaveOpen;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int count2 = count;
			if (this._lengthLimit != CrcCalculatorStream.UnsetLengthLimit)
			{
				if (this._crc32.TotalBytesRead >= this._lengthLimit)
				{
					return 0;
				}
				long num = this._lengthLimit - this._crc32.TotalBytesRead;
				if (num < (long)count)
				{
					count2 = (int)num;
				}
			}
			int num2 = this._innerStream.Read(buffer, offset, count2);
			if (num2 > 0)
			{
				this._crc32.SlurpBlock(buffer, offset, num2);
			}
			return num2;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count > 0)
			{
				this._crc32.SlurpBlock(buffer, offset, count);
			}
			this._innerStream.Write(buffer, offset, count);
		}

		public override void Flush()
		{
			this._innerStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		void IDisposable.Dispose()
		{
			this.Close();
		}

		public override void Close()
		{
			base.Close();
			if (!this._leaveOpen)
			{
				this._innerStream.Close();
			}
		}
	}
}
