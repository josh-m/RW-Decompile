using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RefugeeChased : IncidentWorker
	{
		private static readonly IntRange RaidDelay = new IntRange(1000, 4000);

		private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1f, 1.6f);

		private const float RelationWithColonistWeight = 20f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 intVec;
			Faction faction;
			return this.TryFindSpawnSpot(map, out intVec) && this.TryFindEnemyFaction(out faction);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 spawnSpot;
			if (!this.TryFindSpawnSpot(map, out spawnSpot))
			{
				return false;
			}
			Faction faction;
			if (!this.TryFindEnemyFaction(out faction))
			{
				return false;
			}
			int @int = Rand.Int;
			IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
			raidParms.forced = true;
			raidParms.faction = faction;
			raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			raidParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			raidParms.spawnCenter = spawnSpot;
			raidParms.points = Mathf.Max(raidParms.points * IncidentWorker_RefugeeChased.RaidPointsFactorRange.RandomInRange, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			raidParms.pawnGroupMakerSeed = new int?(@int);
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms, false);
			defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, raidParms.raidArrivalMode, raidParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
			IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, null, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null);
			Pawn refugee = PawnGenerator.GeneratePawn(request);
			refugee.relations.everSeenByPlayer = true;
			string text = "RefugeeChasedInitial".Translate(refugee.Name.ToStringFull, refugee.story.Title, faction.def.pawnsPlural, faction.Name, refugee.ageTracker.AgeBiologicalYears, PawnUtility.PawnKindsToCommaList(pawnKinds, true), refugee.Named("PAWN"));
			text = text.AdjustedFor(refugee, "PAWN");
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, refugee);
			DiaNode diaNode = new DiaNode(text);
			DiaOption diaOption = new DiaOption("RefugeeChasedInitial_Accept".Translate());
			diaOption.action = delegate
			{
				GenSpawn.Spawn(refugee, spawnSpot, map, WipeMode.Vanish);
				refugee.SetFaction(Faction.OfPlayer, null);
				CameraJumper.TryJump(refugee);
				QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, raidParms), Find.TickManager.TicksGame + IncidentWorker_RefugeeChased.RaidDelay.RandomInRange, 0);
				Find.Storyteller.incidentQueue.Add(qi);
			};
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			string text2 = "RefugeeChasedRejected".Translate(refugee.LabelShort, refugee);
			DiaNode diaNode2 = new DiaNode(text2);
			DiaOption diaOption2 = new DiaOption("OK".Translate());
			diaOption2.resolveTree = true;
			diaNode2.options.Add(diaOption2);
			DiaOption diaOption3 = new DiaOption("RefugeeChasedInitial_Reject".Translate());
			diaOption3.action = delegate
			{
				Find.WorldPawns.PassToWorld(refugee, PawnDiscardDecideMode.Decide);
			};
			diaOption3.link = diaNode2;
			diaNode.options.Add(diaOption3);
			string title = "RefugeeChasedTitle".Translate(map.Parent.Label);
			Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, faction, true, true, title));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, title, faction));
			return true;
		}

		private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
		{
			Predicate<IntVec3> validator = (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map);
			return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
		}

		private bool TryFindEnemyFaction(out Faction enemyFac)
		{
			return (from f in Find.FactionManager.AllFactions
			where !f.def.hidden && !f.defeated && f.HostileTo(Faction.OfPlayer)
			select f).TryRandomElement(out enemyFac);
		}
	}
}
