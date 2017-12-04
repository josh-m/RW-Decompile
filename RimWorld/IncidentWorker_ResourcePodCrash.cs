using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ResourcePodCrash : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			List<Thing> things = ItemCollectionGeneratorDefOf.ResourcePod.Worker.Generate(default(ItemCollectionGeneratorParams));
			IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
			DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true, true);
			Find.LetterStack.ReceiveLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null);
			return true;
		}
	}
}
