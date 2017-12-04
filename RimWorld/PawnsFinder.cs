using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnsFinder
	{
		public static IEnumerable<Pawn> AllMapsWorldAndTemporary_AliveOrDead
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsWorldAndTemporary_Alive)
				{
					yield return p;
				}
				if (Find.World != null)
				{
					foreach (Pawn p2 in Find.WorldPawns.AllPawnsDead)
					{
						yield return p2;
					}
				}
				foreach (Pawn p3 in PawnsFinder.Temporary_Dead)
				{
					yield return p3;
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsWorldAndTemporary_Alive
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMaps)
				{
					yield return p;
				}
				if (Find.World != null)
				{
					foreach (Pawn p2 in Find.WorldPawns.AllPawnsAlive)
					{
						yield return p2;
					}
				}
				foreach (Pawn p3 in PawnsFinder.Temporary_Alive)
				{
					yield return p3;
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

		public static IEnumerable<Pawn> Temporary
		{
			get
			{
				List<List<Pawn>> makingPawnsList = PawnGroupKindWorker.pawnsBeingGeneratedNow;
				for (int i = 0; i < makingPawnsList.Count; i++)
				{
					List<Pawn> makingPawns = makingPawnsList[i];
					for (int j = 0; j < makingPawns.Count; j++)
					{
						yield return makingPawns[j];
					}
				}
				List<List<Thing>> makingThingsList = ItemCollectionGenerator.thingsBeingGeneratedNow;
				for (int k = 0; k < makingThingsList.Count; k++)
				{
					List<Thing> makingThings = makingThingsList[k];
					for (int l = 0; l < makingThings.Count; l++)
					{
						Pawn p = makingThings[l] as Pawn;
						if (p != null)
						{
							yield return p;
						}
					}
				}
				if (Current.ProgramState != ProgramState.Playing && Find.GameInitData != null)
				{
					List<Pawn> startingPawns = Find.GameInitData.startingPawns;
					for (int m = 0; m < startingPawns.Count; m++)
					{
						if (startingPawns[m] != null)
						{
							yield return startingPawns[m];
						}
					}
				}
			}
		}

		public static IEnumerable<Pawn> Temporary_Alive
		{
			get
			{
				foreach (Pawn p in PawnsFinder.Temporary)
				{
					if (!p.Dead)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> Temporary_Dead
		{
			get
			{
				foreach (Pawn p in PawnsFinder.Temporary)
				{
					if (p.Dead)
					{
						yield return p;
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
					if (p.IsFreeColonist)
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

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_FreeColonistsAndPrisoners
		{
			get
			{
				return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists.Concat(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_PrisonersOfColony);
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
