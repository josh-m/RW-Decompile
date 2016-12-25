using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class PawnsFinder
	{
		public static IEnumerable<Pawn> AllMapsAndWorld_AliveOrDead
		{
			get
			{
				foreach (Pawn alive in PawnsFinder.AllMapsAndWorld_Alive)
				{
					yield return alive;
				}
				if (Find.World != null)
				{
					foreach (Pawn p in Find.WorldPawns.AllPawnsDead)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsAndWorld_Alive
		{
			get
			{
				List<Pawn> makingPawns = PawnGroupMakerUtility.PawnsBeingGeneratedNow;
				for (int i = 0; i < makingPawns.Count; i++)
				{
					yield return makingPawns[i];
				}
				if (Find.World != null)
				{
					foreach (Pawn p in Find.WorldPawns.AllPawnsAlive)
					{
						yield return p;
					}
					foreach (Pawn p2 in PawnsFinder.AllMaps)
					{
						yield return p2;
					}
				}
				if (Current.ProgramState != ProgramState.Playing && Find.GameInitData != null && Find.GameInitData != null)
				{
					List<Pawn> startingPawns = Find.GameInitData.startingPawns;
					for (int j = 0; j < startingPawns.Count; j++)
					{
						if (startingPawns[j] != null)
						{
							yield return startingPawns[j];
						}
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn p in maps[i].mapPawns.AllPawns)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_Spawned
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Pawn> spawned = maps[i].mapPawns.AllPawnsSpawned;
					for (int j = 0; j < spawned.Count; j++)
					{
						yield return spawned[j];
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMaps)
				{
					yield return p;
				}
				if (Find.World != null)
				{
					List<Caravan> caravans = Find.WorldObjects.Caravans;
					for (int i = 0; i < caravans.Count; i++)
					{
						List<Pawn> pawns = caravans[i].PawnsListForReading;
						for (int j = 0; j < pawns.Count; j++)
						{
							yield return pawns[j];
						}
					}
					List<TravelingTransportPods> travelingTransportPods = Find.WorldObjects.TravelingTransportPods;
					for (int k = 0; k < travelingTransportPods.Count; k++)
					{
						foreach (Pawn p2 in travelingTransportPods[k].Pawns)
						{
							yield return p2;
						}
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Colonists
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods)
				{
					if (p.IsColonist)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_FreeColonists
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods)
				{
					if (p.IsColonist && p.HostFaction == null)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_PrisonersOfColony
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods)
				{
					if (p.IsPrisonerOfColony)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_PrisonersOfColonySpawned
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Pawn> prisonersOfColonySpawned = maps[i].mapPawns.PrisonersOfColonySpawned;
					for (int j = 0; j < prisonersOfColonySpawned.Count; j++)
					{
						yield return prisonersOfColonySpawned[j];
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_PrisonersOfColony
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn p in maps[i].mapPawns.PrisonersOfColony)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonists
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn p in maps[i].mapPawns.FreeColonists)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsSpawned
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn p in maps[i].mapPawns.FreeColonistsSpawned)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsAndPrisonersSpawned
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn p in maps[i].mapPawns.FreeColonistsAndPrisonersSpawned)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsAndPrisoners
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn p in maps[i].mapPawns.FreeColonistsAndPrisoners)
					{
						yield return p;
					}
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Pawn> AllMaps_SpawnedPawnsInFaction(Faction faction)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Pawn> spawnedPawnsInFaction = maps[i].mapPawns.SpawnedPawnsInFaction(faction);
				for (int j = 0; j < spawnedPawnsInFaction.Count; j++)
				{
					yield return spawnedPawnsInFaction[j];
				}
			}
		}
	}
}
