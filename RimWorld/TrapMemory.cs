using System;
using Verse;

namespace RimWorld
{
	public class TrapMemory : IExposable
	{
		private const int TrapRecordTicksBeforeExpiry = 1680000;

		public IntVec3 loc;

		public Map map;

		public int tick;

		public IntVec3 Cell
		{
			get
			{
				return this.loc;
			}
		}

		public int Tick
		{
			get
			{
				return this.tick;
			}
		}

		public int Age
		{
			get
			{
				return Find.TickManager.TicksGame - this.tick;
			}
		}

		public bool Expired
		{
			get
			{
				return this.Age > 1680000;
			}
		}

		public float PowerPercent
		{
			get
			{
				return 1f - (float)this.Age / 1680000f;
			}
		}

		public TrapMemory()
		{
		}

		public TrapMemory(IntVec3 cell, Map map, int tick)
		{
			this.loc = cell;
			this.map = map;
			this.tick = tick;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.loc, "loc", default(IntVec3), false);
			Scribe_References.LookReference<Map>(ref this.map, "map", false);
			Scribe_Values.LookValue<int>(ref this.tick, "tick", 0, false);
		}
	}
}
