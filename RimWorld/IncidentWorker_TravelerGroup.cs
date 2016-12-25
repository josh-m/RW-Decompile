using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_TravelerGroup : IncidentWorker_NeutralGroup
	{
		public override bool TryExecute(IncidentParms parms)
		{
			if (!this.TryResolveParms(parms))
			{
				return false;
			}
			IntVec3 travelDest;
			if (!RCellFinder.TryFindTravelDestFrom(parms.spawnCenter, out travelDest))
			{
				Log.Warning("Failed to do traveler incident from " + parms.spawnCenter + ": couldn't find anywhere for the traveler to go.");
				return false;
			}
			List<Pawn> list = base.SpawnPawns(parms);
			if (list.Count == 0)
			{
				return false;
			}
			string text;
			if (list.Count == 1)
			{
				text = "SingleTravelerPassing".Translate(new object[]
				{
					list[0].story.adulthood.title.ToLower(),
					parms.faction.Name,
					list[0].Name
				});
				text = text.AdjustedFor(list[0]);
			}
			else
			{
				text = "GroupTravelersPassing".Translate(new object[]
				{
					parms.faction.Name
				});
			}
			Messages.Message(text, list[0], MessageSound.Standard);
			LordJob_TravelAndExit lordJob = new LordJob_TravelAndExit(travelDest);
			LordMaker.MakeNewLord(parms.faction, lordJob, list);
			return true;
		}
	}
}
