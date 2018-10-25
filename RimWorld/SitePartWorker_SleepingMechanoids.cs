using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class SitePartWorker_SleepingMechanoids : SitePartWorker
	{
		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
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
			lookTargets = pawn;
			return arrivedLetterPart;
		}

		public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), this.GetMechanoidsCount(site, siteCoreOrPart.parms));
		}

		public override string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return string.Concat(new object[]
			{
				base.GetPostProcessedThreatLabel(site, siteCoreOrPart),
				" (",
				this.GetMechanoidsCount(site, siteCoreOrPart.parms),
				")"
			});
		}

		public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
			siteCoreOrPartParams.threatPoints = Mathf.Max(siteCoreOrPartParams.threatPoints, FactionDefOf.Mechanoid.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			return siteCoreOrPartParams;
		}

		private int GetMechanoidsCount(Site site, SiteCoreOrPartParams parms)
		{
			return PawnGroupMakerUtility.GeneratePawnKindsExample(new PawnGroupMakerParms
			{
				tile = site.Tile,
				faction = Faction.OfMechanoids,
				groupKind = PawnGroupKindDefOf.Combat,
				points = parms.threatPoints,
				seed = new int?(SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms))
			}).Count<PawnKindDef>();
		}
	}
}
