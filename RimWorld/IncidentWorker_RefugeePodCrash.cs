using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RefugeePodCrash : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<Thing> things = ItemCollectionGeneratorDefOf.RefugeePod.Worker.Generate(default(ItemCollectionGeneratorParams));
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			Pawn pawn = this.FindPawn(things);
			string label = "LetterLabelRefugeePodCrash".Translate();
			string text = "RefugeePodCrash".Translate();
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map, false), null);
			ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
			activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things, true, false);
			activeDropPodInfo.openDelay = 180;
			activeDropPodInfo.leaveSlag = true;
			DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo, true);
			return true;
		}

		private Pawn FindPawn(List<Thing> things)
		{
			for (int i = 0; i < things.Count; i++)
			{
				Pawn pawn = things[i] as Pawn;
				if (pawn != null)
				{
					return pawn;
				}
				Corpse corpse = things[i] as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
			}
			return null;
		}
	}
}
