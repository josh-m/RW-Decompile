using System;
using Verse;

namespace RimWorld
{
	public class DirectPawnRelation : IExposable
	{
		public PawnRelationDef def;

		public Pawn otherPawn;

		public int startTicks;

		public DirectPawnRelation()
		{
		}

		public DirectPawnRelation(PawnRelationDef def, Pawn otherPawn, int startTicks)
		{
			this.def = def;
			this.otherPawn = otherPawn;
			this.startTicks = startTicks;
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<PawnRelationDef>(ref this.def, "def");
			Scribe_References.LookReference<Pawn>(ref this.otherPawn, "otherPawn", true);
			Scribe_Values.LookValue<int>(ref this.startTicks, "startTicks", 0, false);
		}
	}
}
