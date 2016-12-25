using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToilData_MarriageCeremony : LordToilData
	{
		public CellRect spectateRect;

		public SpectateRectSide spectateRectAllowedSides = SpectateRectSide.All;

		public override void ExposeData()
		{
			Scribe_Values.LookValue<CellRect>(ref this.spectateRect, "spectateRect", default(CellRect), false);
			Scribe_Values.LookValue<SpectateRectSide>(ref this.spectateRectAllowedSides, "spectateRectAllowedSides", SpectateRectSide.None, false);
		}
	}
}
