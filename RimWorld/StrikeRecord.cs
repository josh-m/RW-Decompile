using System;
using Verse;

namespace RimWorld
{
	internal struct StrikeRecord : IExposable
	{
		private const int StrikeRecordExpiryDays = 15;

		public IntVec3 cell;

		public int ticksGame;

		public ThingDef def;

		public bool Expired
		{
			get
			{
				return Find.TickManager.TicksGame > this.ticksGame + 900000;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.cell, "cell", default(IntVec3), false);
			Scribe_Values.LookValue<int>(ref this.ticksGame, "ticksGame", 0, false);
			Scribe_Defs.LookDef<ThingDef>(ref this.def, "def");
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.cell,
				", ",
				this.def,
				", ",
				this.ticksGame,
				")"
			});
		}
	}
}
