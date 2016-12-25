using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Verse.Noise
{
	public abstract class ModuleBase : IDisposable
	{
		protected ModuleBase[] modules;

		[XmlIgnore]
		[NonSerialized]
		private bool m_disposed;

		public int SourceModuleCount
		{
			get
			{
				return (this.modules != null) ? this.modules.Length : 0;
			}
		}

		public virtual ModuleBase this[int index]
		{
			get
			{
				if (index < 0 || index >= this.modules.Length)
				{
					throw new ArgumentOutOfRangeException("Index out of valid module range");
				}
				if (this.modules[index] == null)
				{
					throw new ArgumentNullException("Desired element is null");
				}
				return this.modules[index];
			}
			set
			{
				if (index < 0 || index >= this.modules.Length)
				{
					throw new ArgumentOutOfRangeException("Index out of valid module range");
				}
				if (value == null)
				{
					throw new ArgumentNullException("Value should not be null");
				}
				this.modules[index] = value;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return this.m_disposed;
			}
		}

		protected ModuleBase(int count)
		{
			if (count > 0)
			{
				this.modules = new ModuleBase[count];
			}
		}

		public abstract double GetValue(double x, double y, double z);

		public float GetValue(IntVec2 coordinate)
		{
			return (float)this.GetValue((double)coordinate.x, 0.0, (double)coordinate.z);
		}

		public float GetValue(IntVec3 coordinate)
		{
			return (float)this.GetValue((double)coordinate.x, (double)coordinate.y, (double)coordinate.z);
		}

		public float GetValue(Vector3 coordinate)
		{
			return (float)this.GetValue((double)coordinate.x, (double)coordinate.y, (double)coordinate.z);
		}

		public void Dispose()
		{
			if (!this.m_disposed)
			{
				this.m_disposed = this.Disposing();
			}
			GC.SuppressFinalize(this);
		}

		protected virtual bool Disposing()
		{
			if (this.modules != null)
			{
				for (int i = 0; i < this.modules.Length; i++)
				{
					this.modules[i].Dispose();
					this.modules[i] = null;
				}
				this.modules = null;
			}
			return true;
		}
	}
}
