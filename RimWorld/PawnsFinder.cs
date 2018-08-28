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
				if (Current.ProgramState != ProgramState.Entry)
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
		}

		public static IEnumerable<Pawn> AllMaps_Spawned
		{
			get
			{
				if (Current.ProgramState != ProgramState.Entry)
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
		}

		public static IEnumerable<Pawn> All_AliveOrDead
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
				{
					yield return p;
				}
				foreach (Pawn p2 in PawnsFinder.AllCaravansAndTravelingTransportPods_AliveOrDead)
				{
					yield return p2;
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
				List<List<Thing>> makingThingsList = ThingSetMaker.thingsBeingGeneratedNow;
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
					List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;
					for (int m = 0; m < startingAndOptionalPawns.Count; m++)
					{
						if (startingAndOptionalPawns[m] != null)
						{
							yield return startingAndOptionalPawns[m];
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

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMaps)
				{
					yield return p;
				}
				foreach (Pawn p2 in PawnsFinder.AllCaravansAndTravelingTransportPods_Alive)
				{
					yield return p2;
				}
			}
		}

		public static IEnumerable<Pawn> AllCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllCaravansAndTravelingTransportPods_AliveOrDead)
				{
					if (!p.Dead)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllCaravansAndTravelingTransportPods_AliveOrDead
		{
			get
			{
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
						foreach (Pawn p in travelingTransportPods[k].Pawns)
						{
							yield return p;
						}
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (p.IsColonist)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (p.IsFreeColonist)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (p.IsFreeColonist && !p.Suspended)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction
		{
			get
			{
				Faction playerFaction = Faction.OfPlayer;
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (p.Faction == playerFaction)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep
		{
			get
			{
				Faction playerFaction = Faction.OfPlayer;
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (p.Faction == playerFaction && !p.Suspended)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (p.IsPrisonerOfColony)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners
		{
			get
			{
				return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Concat(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony);
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					if (!p.Suspended)
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
				if (Current.ProgramState != ProgramState.Entry)
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
		}

		public static IEnumerable<Pawn> AllMaps_PrisonersOfColony
		{
			get
			{
				if (Current.ProgramState != ProgramState.Entry)
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
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonists
		{
			get
			{
				if (Current.ProgramState != ProgramState.Entry)
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
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsSpawned
		{
			get
			{
				if (Current.ProgramState != ProgramState.Entry)
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
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsAndPrisonersSpawned
		{
			get
			{
				if (Current.ProgramState != ProgramState.Entry)
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
