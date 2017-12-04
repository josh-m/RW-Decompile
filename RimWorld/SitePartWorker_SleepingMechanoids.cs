using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class SitePartWorker_SleepingMechanoids : SitePartWorker
	{
		public override void PostMapGenerate(Map map)
		{
			base.PostMapGenerate(map);
			IEnumerable<Pawn> source = from x in map.mapPawns.AllPawnsSpawned
			where x.RaceProps.IsMechanoid
			select x;
			Pawn pawn = (from x in source
			where x.GetLord() != null && x.GetLord().LordJob is LordJob_SleepThenAssaultColony
			select x).FirstOrDefault<Pawn>();
			if (pawn == null)
			{
				pawn = source.FirstOrDefault<Pawn>();
			}
			Find.LetterStack.ReceiveLetter("LetterLabelSleepingMechanoids".Translate(), "LetterSleepingMechanoids".Translate(), LetterDefOf.NegativeEvent, pawn, null);
		}
	}
}
