using NAudio.Wave;
using System;
using System.IO;

namespace NVorbis.NAudioSupport
{
	internal class VorbisWaveReader : WaveStream, IDisposable, IWaveProvider, ISampleProvider
	{
		private VorbisReader _reader;

		private WaveFormat _waveFormat;

		[ThreadStatic]
		private static float[] _conversionBuffer;

		public override WaveFormat WaveFormat
		{
			get
			{
				return this._waveFormat;
			}
		}

		public override long Length
		{
			get
			{
				return (long)(this._reader.TotalTime.TotalSeconds * (double)this._waveFormat.SampleRate * (double)this._waveFormat.Channels * 4.0);
			}
		}

		public override long Position
		{
			get
			{
				return (long)(this._reader.DecodedTime.TotalMilliseconds * (double)this._reader.SampleRate * (double)this._reader.Channels * 4.0);
			}
			set
			{
				if (value < 0L || value > this.Length)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this._reader.DecodedTime = TimeSpan.FromSeconds((double)value / (double)this._reader.SampleRate / (double)this._reader.Channels / 4.0);
			}
		}

		public bool IsParameterChange
		{
			get
			{
				return this._reader.IsParameterChange;
			}
		}

		public int StreamCount
		{
			get
			{
				return this._reader.StreamCount;
			}
		}

		public int? NextStreamIndex
		{
			get;
			set;
		}

		public int CurrentStream
		{
			get
			{
				return this._reader.StreamIndex;
			}
			set
			{
				if (!this._reader.SwitchStreams(value))
				{
					throw new NVorbis.InvalidDataException("The selected stream is not a valid Vorbis stream!");
				}
				if (this.NextStreamIndex.HasValue && value == this.NextStreamIndex.Value)
				{
					this.NextStreamIndex = null;
				}
			}
		}

		public int UpperBitrate
		{
			get
			{
				return this._reader.UpperBitrate;
			}
		}

		public int NominalBitrate
		{
			get
			{
				return this._reader.NominalBitrate;
			}
		}

		public int LowerBitrate
		{
			get
			{
				return this._reader.LowerBitrate;
			}
		}

		public string Vendor
		{
			get
			{
				return this._reader.Vendor;
			}
		}

		public string[] Comments
		{
			get
			{
				return this._reader.Comments;
			}
		}

		public long ContainerOverheadBits
		{
			get
			{
				return this._reader.ContainerOverheadBits;
			}
		}

		public IVorbisStreamStatus[] Stats
		{
			get
			{
				return this._reader.Stats;
			}
		}

		public VorbisWaveReader(string fileName)
		{
			this._reader = new VorbisReader(fileName);
			this._waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(this._reader.SampleRate, this._reader.Channels);
		}

		public VorbisWaveReader(Stream sourceStream)
		{
			this._reader = new VorbisReader(sourceStream, false);
			this._waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(this._reader.SampleRate, this._reader.Channels);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this._reader != null)
			{
				this._reader.Dispose();
				this._reader = null;
			}
			base.Dispose(disposing);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			count /= 4;
			count -= count % this._reader.Channels;
			float[] arg_2E_0;
			if ((arg_2E_0 = VorbisWaveReader._conversionBuffer) == null)
			{
				arg_2E_0 = (VorbisWaveReader._conversionBuffer = new float[count]);
			}
			float[] array = arg_2E_0;
			if (array.Length < count)
			{
				array = (VorbisWaveReader._conversionBuffer = new float[count]);
			}
			int num = this.Read(array, 0, count) * 4;
			Buffer.BlockCopy(array, 0, buffer, offset, num);
			return num;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			return this._reader.ReadSamples(buffer, offset, count);
		}

		public void ClearParameterChange()
		{
			this._reader.ClearParameterChange();
		}

		public bool GetNextStreamIndex()
		{
			if (!this.NextStreamIndex.HasValue)
			{
				int streamCount = this._reader.StreamCount;
				if (this._reader.FindNextStream())
				{
					this.NextStreamIndex = new int?(streamCount);
					return true;
				}
			}
			return false;
		}
	}
}
