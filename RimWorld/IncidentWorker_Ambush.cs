using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class IncidentWorker_Ambush : IncidentWorker
	{
		private const float CaravanPawnsScorePerColonistUndowned = 1f;

		private const float CaravanPawnsScorePerColonistDowned = 0.35f;

		private const float CaravanPawnsScorePerPrisoner = 0.35f;

		public const float CaravanPawnsScorePerAnimalBodySize = 0.2f;

		private const int MapCellsPerCaravanPawnsCountScore = 900;

		private const int MinMapSize = 75;

		private const int MaxMapSize = 110;

		private const float MinEnemyPoints = 45f;

		private static List<Pawn> tmpCaravanPawns = new List<Pawn>();

		private static readonly FloatRange EnemyPointsPerCaravanPawnsScoreRange = new FloatRange(35f, 90f);

		protected abstract List<Pawn> GeneratePawns(Caravan caravan, float points, Map map);

		protected virtual void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
		{
		}

		protected virtual LordJob CreateLordJob(List<Pawn> generatedPawns, out Faction faction)
		{
			faction = null;
			return null;
		}

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			return CaravanRandomEncounterUtility.CanMeetRandomCaravanAt(target.Tile);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				this.DoExecute((Caravan)parms.target);
			}, "GeneratingMapForNewEncounter", false, null);
			return true;
		}

		private void DoExecute(Caravan caravan)
		{
			float num = this.CaravanPawnsCountScore(caravan);
			int num2 = Mathf.RoundToInt(num * 900f);
			int num3 = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt((float)num2)), 75, 110);
			float points = Mathf.Max(num * IncidentWorker_Ambush.EnemyPointsPerCaravanPawnsScoreRange.RandomInRange, 45f);
			IncidentWorker_Ambush.tmpCaravanPawns.Clear();
			IncidentWorker_Ambush.tmpCaravanPawns.AddRange(caravan.PawnsListForReading);
			Map map = CaravanTargetIncidentUtility.GenerateOrGetMapForIncident(num3, num3, caravan, CaravanEnterMode.None, WorldObjectDefOf.Ambush, null, false);
			List<Pawn> list = this.GeneratePawns(caravan, points, map);
			IntVec3 playerStartingSpot;
			IntVec3 root;
			MultipleCaravansCellFinder.FindStartingCellsFor2Groups(map, out playerStartingSpot, out root);
			CaravanEnterMapUtility.Enter(caravan, map, (Pawn x) => CellFinder.RandomSpawnCellForPawnNear(playerStartingSpot, map), CaravanDropInventoryMode.DoNotDrop, true);
			for (int i = 0; i < list.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(root, map);
				GenSpawn.Spawn(list[i], loc, map, Rot4.Random);
			}
			IncidentWorker_Ambush.tmpCaravanPawns.Clear();
			this.PostProcessGeneratedPawnsAfterSpawning(list);
			Faction faction;
			LordJob lordJob = this.CreateLordJob(list, out faction);
			if (lordJob != null)
			{
				LordMaker.MakeNewLord(faction, lordJob, map, list);
			}
			this.SendAmbushLetter(list[0], faction);
			Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
		}

		protected abstract void SendAmbushLetter(Pawn anyPawn, Faction enemyFaction);

		private float CaravanPawnsCountScore(Caravan caravan)
		{
			float num = 0f;
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				if (pawnsListForReading[i].IsColonist)
				{
					num += (pawnsListForReading[i].Downed ? 0.35f : 1f);
				}
				else if (pawnsListForReading[i].RaceProps.Humanlike)
				{
					num += 0.35f;
				}
				else
				{
					num += 0.2f * pawnsListForReading[i].BodySize;
				}
			}
			return num;
		}
	}
}
