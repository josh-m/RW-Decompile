using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Ambush_Edge : GenStep_Ambush
	{
		protected override SignalAction_Ambush MakeAmbushSignalAction(CellRect rectToDefend, IntVec3 root)
		{
			SignalAction_Ambush signalAction_Ambush = base.MakeAmbushSignalAction(rectToDefend, root);
			signalAction_Ambush.spawnPawnsOnEdge = true;
			return signalAction_Ambush;
		}
	}
}
