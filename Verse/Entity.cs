using System;

namespace Verse
{
	public abstract class Entity
	{
		public abstract string LabelCap
		{
			get;
		}

		public abstract string Label
		{
			get;
		}

		public virtual string LabelShort
		{
			get
			{
				return this.LabelCap;
			}
		}

		public virtual string LabelMouseover
		{
			get
			{
				return this.LabelCap;
			}
		}

		public abstract void SpawnSetup(Map map);

		public abstract void DeSpawn();

		public virtual void Tick()
		{
			throw new NotImplementedException();
		}

		public virtual void TickRare()
		{
			throw new NotImplementedException();
		}

		public virtual void TickLong()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return this.LabelCap;
		}
	}
}
