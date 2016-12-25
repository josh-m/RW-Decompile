using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		private const float HivePoints = 400f;

		protected override bool CanFireNowSub()
		{
			IntVec3 intVec;
			return base.CanFireNowSub() && HivesUtility.TotalSpawnedHivesCount < 30 && InfestationCellFinder.TryFindCell(out intVec);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Hive t = null;
			int num;
			for (int i = Mathf.Max(GenMath.RoundRandom(parms.points / 400f), 1); i > 0; i -= num)
			{
				num = Mathf.Min(3, i);
				t = this.SpawnHiveCluster(num);
			}
			base.SendStandardLetter(t);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}

		private Hive SpawnHiveCluster(int hiveCount)
		{
			IntVec3 loc;
			if (!InfestationCellFinder.TryFindCell(out loc))
			{
				return null;
			}
			Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive, null), loc);
			hive.SetFaction(Faction.OfInsects, null);
			IncidentWorker_Infestation.MakeHiveSpawnInitialEssentials(hive);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				Hive hive2;
				if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(false, out hive2))
				{
					IncidentWorker_Infestation.MakeHiveSpawnInitialEssentials(hive2);
					hive = hive2;
				}
			}
			return hive;
		}

		private static void MakeHiveSpawnInitialEssentials(Hive hive)
		{
			hive.StartInitialPawnSpawnCountdown();
			hive.TryGetComp<CompSpawner>().TryDoSpawn();
		}
	}
}
