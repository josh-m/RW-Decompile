using Ionic.Crc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Ionic.Zlib
{
	public class ParallelDeflateOutputStream : Stream
	{
		[Flags]
		private enum TraceBits : uint
		{
			None = 0u,
			NotUsed1 = 1u,
			EmitLock = 2u,
			EmitEnter = 4u,
			EmitBegin = 8u,
			EmitDone = 16u,
			EmitSkip = 32u,
			EmitAll = 58u,
			Flush = 64u,
			Lifecycle = 128u,
			Session = 256u,
			Synch = 512u,
			Instance = 1024u,
			Compress = 2048u,
			Write = 4096u,
			WriteEnter = 8192u,
			WriteTake = 16384u,
			All = 4294967295u
		}

		private static readonly int IO_BUFFER_SIZE_DEFAULT = 65536;

		private static readonly int BufferPairsPerCore = 4;

		private List<WorkItem> _pool;

		private bool _leaveOpen;

		private bool emitting;

		private Stream _outStream;

		private int _maxBufferPairs;

		private int _bufferSize = ParallelDeflateOutputStream.IO_BUFFER_SIZE_DEFAULT;

		private AutoResetEvent _newlyCompressedBlob;

		private object _outputLock = new object();

		private bool _isClosed;

		private bool _firstWriteDone;

		private int _currentlyFilling;

		private int _lastFilled;

		private int _lastWritten;

		private int _latestCompressed;

		private int _Crc32;

		private CRC32 _runningCrc;

		private object _latestLock = new object();

		private Queue<int> _toWrite;

		private Queue<int> _toFill;

		private long _totalBytesProcessed;

		private CompressionLevel _compressLevel;

		private volatile Exception _pendingException;

		private bool _handlingException;

		private object _eLock = new object();

		private ParallelDeflateOutputStream.TraceBits _DesiredTrace = ParallelDeflateOutputStream.TraceBits.EmitLock | ParallelDeflateOutputStream.TraceBits.EmitEnter | ParallelDeflateOutputStream.TraceBits.EmitBegin | ParallelDeflateOutputStream.TraceBits.EmitDone | ParallelDeflateOutputStream.TraceBits.EmitSkip | ParallelDeflateOutputStream.TraceBits.Session | ParallelDeflateOutputStream.TraceBits.Compress | ParallelDeflateOutputStream.TraceBits.WriteEnter | ParallelDeflateOutputStream.TraceBits.WriteTake;

		public CompressionStrategy Strategy
		{
			get;
			private set;
		}

		public int MaxBufferPairs
		{
			get
			{
				return this._maxBufferPairs;
			}
			set
			{
				if (value < 4)
				{
					throw new ArgumentException("MaxBufferPairs", "Value must be 4 or greater.");
				}
				this._maxBufferPairs = value;
			}
		}

		public int BufferSize
		{
			get
			{
				return this._bufferSize;
			}
			set
			{
				if (value < 1024)
				{
					throw new ArgumentOutOfRangeException("BufferSize", "BufferSize must be greater than 1024 bytes");
				}
				this._bufferSize = value;
			}
		}

		public int Crc32
		{
			get
			{
				return this._Crc32;
			}
		}

		public long BytesProcessed
		{
			get
			{
				return this._totalBytesProcessed;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanRead
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
				return this._outStream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				return this._outStream.Position;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public ParallelDeflateOutputStream(Stream stream) : this(stream, CompressionLevel.Default, CompressionStrategy.Default, false)
		{
		}

		public ParallelDeflateOutputStream(Stream stream, CompressionLevel level) : this(stream, level, CompressionStrategy.Default, false)
		{
		}

		public ParallelDeflateOutputStream(Stream stream, bool leaveOpen) : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
		{
		}

		public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, bool leaveOpen) : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
		{
		}

		public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, CompressionStrategy strategy, bool leaveOpen)
		{
			this._outStream = stream;
			this._compressLevel = level;
			this.Strategy = strategy;
			this._leaveOpen = leaveOpen;
			this.MaxBufferPairs = 16;
		}

		private void _InitializePoolOfWorkItems()
		{
			this._toWrite = new Queue<int>();
			this._toFill = new Queue<int>();
			this._pool = new List<WorkItem>();
			int num = ParallelDeflateOutputStream.BufferPairsPerCore * Environment.ProcessorCount;
			num = Math.Min(num, this._maxBufferPairs);
			for (int i = 0; i < num; i++)
			{
				this._pool.Add(new WorkItem(this._bufferSize, this._compressLevel, this.Strategy, i));
				this._toFill.Enqueue(i);
			}
			this._newlyCompressedBlob = new AutoResetEvent(false);
			this._runningCrc = new CRC32();
			this._currentlyFilling = -1;
			this._lastFilled = -1;
			this._lastWritten = -1;
			this._latestCompressed = -1;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			bool mustWait = false;
			if (this._isClosed)
			{
				throw new InvalidOperationException();
			}
			if (this._pendingException != null)
			{
				this._handlingException = true;
				Exception pendingException = this._pendingException;
				this._pendingException = null;
				throw pendingException;
			}
			if (count == 0)
			{
				return;
			}
			if (!this._firstWriteDone)
			{
				this._InitializePoolOfWorkItems();
				this._firstWriteDone = true;
			}
			while (true)
			{
				this.EmitPendingBuffers(false, mustWait);
				mustWait = false;
				int num;
				if (this._currentlyFilling >= 0)
				{
					num = this._currentlyFilling;
					goto IL_AF;
				}
				if (this._toFill.Count != 0)
				{
					num = this._toFill.Dequeue();
					this._lastFilled++;
					goto IL_AF;
				}
				mustWait = true;
				IL_173:
				if (count <= 0)
				{
					return;
				}
				continue;
				IL_AF:
				WorkItem workItem = this._pool[num];
				int num2 = (workItem.buffer.Length - workItem.inputBytesAvailable <= count) ? (workItem.buffer.Length - workItem.inputBytesAvailable) : count;
				workItem.ordinal = this._lastFilled;
				Buffer.BlockCopy(buffer, offset, workItem.buffer, workItem.inputBytesAvailable, num2);
				count -= num2;
				offset += num2;
				workItem.inputBytesAvailable += num2;
				if (workItem.inputBytesAvailable == workItem.buffer.Length)
				{
					if (!ThreadPool.QueueUserWorkItem(new WaitCallback(this._DeflateOne), workItem))
					{
						break;
					}
					this._currentlyFilling = -1;
				}
				else
				{
					this._currentlyFilling = num;
				}
				if (count > 0)
				{
					goto IL_173;
				}
				goto IL_173;
			}
			throw new Exception("Cannot enqueue workitem");
		}

		private void _FlushFinish()
		{
			byte[] array = new byte[128];
			ZlibCodec zlibCodec = new ZlibCodec();
			int num = zlibCodec.InitializeDeflate(this._compressLevel, false);
			zlibCodec.InputBuffer = null;
			zlibCodec.NextIn = 0;
			zlibCodec.AvailableBytesIn = 0;
			zlibCodec.OutputBuffer = array;
			zlibCodec.NextOut = 0;
			zlibCodec.AvailableBytesOut = array.Length;
			num = zlibCodec.Deflate(FlushType.Finish);
			if (num != 1 && num != 0)
			{
				throw new Exception("deflating: " + zlibCodec.Message);
			}
			if (array.Length - zlibCodec.AvailableBytesOut > 0)
			{
				this._outStream.Write(array, 0, array.Length - zlibCodec.AvailableBytesOut);
			}
			zlibCodec.EndDeflate();
			this._Crc32 = this._runningCrc.Crc32Result;
		}

		private void _Flush(bool lastInput)
		{
			if (this._isClosed)
			{
				throw new InvalidOperationException();
			}
			if (this.emitting)
			{
				return;
			}
			if (this._currentlyFilling >= 0)
			{
				WorkItem wi = this._pool[this._currentlyFilling];
				this._DeflateOne(wi);
				this._currentlyFilling = -1;
			}
			if (lastInput)
			{
				this.EmitPendingBuffers(true, false);
				this._FlushFinish();
			}
			else
			{
				this.EmitPendingBuffers(false, false);
			}
		}

		public override void Flush()
		{
			if (this._pendingException != null)
			{
				this._handlingException = true;
				Exception pendingException = this._pendingException;
				this._pendingException = null;
				throw pendingException;
			}
			if (this._handlingException)
			{
				return;
			}
			this._Flush(false);
		}

		public override void Close()
		{
			if (this._pendingException != null)
			{
				this._handlingException = true;
				Exception pendingException = this._pendingException;
				this._pendingException = null;
				throw pendingException;
			}
			if (this._handlingException)
			{
				return;
			}
			if (this._isClosed)
			{
				return;
			}
			this._Flush(true);
			if (!this._leaveOpen)
			{
				this._outStream.Close();
			}
			this._isClosed = true;
		}

		public new void Dispose()
		{
			this.Close();
			this._pool = null;
			this.Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void Reset(Stream stream)
		{
			if (!this._firstWriteDone)
			{
				return;
			}
			this._toWrite.Clear();
			this._toFill.Clear();
			foreach (WorkItem current in this._pool)
			{
				this._toFill.Enqueue(current.index);
				current.ordinal = -1;
			}
			this._firstWriteDone = false;
			this._totalBytesProcessed = 0L;
			this._runningCrc = new CRC32();
			this._isClosed = false;
			this._currentlyFilling = -1;
			this._lastFilled = -1;
			this._lastWritten = -1;
			this._latestCompressed = -1;
			this._outStream = stream;
		}

		private void EmitPendingBuffers(bool doAll, bool mustWait)
		{
			if (this.emitting)
			{
				return;
			}
			this.emitting = true;
			if (doAll || mustWait)
			{
				this._newlyCompressedBlob.WaitOne();
			}
			do
			{
				int num = -1;
				int num2 = (!doAll) ? ((!mustWait) ? 0 : -1) : 200;
				int num3 = -1;
				do
				{
					if (Monitor.TryEnter(this._toWrite, num2))
					{
						num3 = -1;
						try
						{
							if (this._toWrite.Count > 0)
							{
								num3 = this._toWrite.Dequeue();
							}
						}
						finally
						{
							Monitor.Exit(this._toWrite);
						}
						if (num3 >= 0)
						{
							WorkItem workItem = this._pool[num3];
							if (workItem.ordinal != this._lastWritten + 1)
							{
								object toWrite = this._toWrite;
								lock (toWrite)
								{
									this._toWrite.Enqueue(num3);
								}
								if (num == num3)
								{
									this._newlyCompressedBlob.WaitOne();
									num = -1;
								}
								else if (num == -1)
								{
									num = num3;
								}
							}
							else
							{
								num = -1;
								this._outStream.Write(workItem.compressed, 0, workItem.compressedBytesAvailable);
								this._runningCrc.Combine(workItem.crc, workItem.inputBytesAvailable);
								this._totalBytesProcessed += (long)workItem.inputBytesAvailable;
								workItem.inputBytesAvailable = 0;
								this._lastWritten = workItem.ordinal;
								this._toFill.Enqueue(workItem.index);
								if (num2 == -1)
								{
									num2 = 0;
								}
							}
						}
					}
					else
					{
						num3 = -1;
					}
				}
				while (num3 >= 0);
			}
			while (doAll && this._lastWritten != this._latestCompressed);
			this.emitting = false;
		}

		private void _DeflateOne(object wi)
		{
			WorkItem workItem = (WorkItem)wi;
			try
			{
				CRC32 cRC = new CRC32();
				cRC.SlurpBlock(workItem.buffer, 0, workItem.inputBytesAvailable);
				this.DeflateOneSegment(workItem);
				workItem.crc = cRC.Crc32Result;
				object latestLock = this._latestLock;
				lock (latestLock)
				{
					if (workItem.ordinal > this._latestCompressed)
					{
						this._latestCompressed = workItem.ordinal;
					}
				}
				object toWrite = this._toWrite;
				lock (toWrite)
				{
					this._toWrite.Enqueue(workItem.index);
				}
				this._newlyCompressedBlob.Set();
			}
			catch (Exception pendingException)
			{
				object eLock = this._eLock;
				lock (eLock)
				{
					if (this._pendingException != null)
					{
						this._pendingException = pendingException;
					}
				}
			}
		}

		private bool DeflateOneSegment(WorkItem workitem)
		{
			ZlibCodec compressor = workitem.compressor;
			compressor.ResetDeflate();
			compressor.NextIn = 0;
			compressor.AvailableBytesIn = workitem.inputBytesAvailable;
			compressor.NextOut = 0;
			compressor.AvailableBytesOut = workitem.compressed.Length;
			do
			{
				compressor.Deflate(FlushType.None);
			}
			while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);
			compressor.Deflate(FlushType.Sync);
			workitem.compressedBytesAvailable = (int)compressor.TotalBytesOut;
			return true;
		}

		[Conditional("Trace")]
		private void TraceOutput(ParallelDeflateOutputStream.TraceBits bits, string format, params object[] varParams)
		{
			if ((bits & this._DesiredTrace) != ParallelDeflateOutputStream.TraceBits.None)
			{
				object outputLock = this._outputLock;
				lock (outputLock)
				{
					int hashCode = Thread.CurrentThread.GetHashCode();
					Console.ForegroundColor = hashCode % 8 + ConsoleColor.DarkGray;
					Console.Write("{0:000} PDOS ", hashCode);
					Console.WriteLine(format, varParams);
					Console.ResetColor();
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}
	}
}
