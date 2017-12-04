using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI.Group;

namespace Verse
{
	public static class ArenaUtility
	{
		public struct ArenaResult
		{
			public enum Winner
			{
				Other,
				Lhs,
				Rhs
			}

			public ArenaUtility.ArenaResult.Winner winner;

			public int tickDuration;
		}

		private class ArenaSetState
		{
			public int live;
		}

		private const int liveSimultaneous = 15;

		public static void BeginArenaFight(List<PawnKindDef> lhs, List<PawnKindDef> rhs, Action<ArenaUtility.ArenaResult> callback)
		{
			MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Debug_Arena);
			mapParent.Tile = TileFinder.RandomFactionBaseTileFor(Faction.OfPlayer, true, (int tile) => lhs.Concat(rhs).Any((PawnKindDef pawnkind) => Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, pawnkind.race)));
			mapParent.SetFaction(Faction.OfPlayer);
			Find.WorldObjects.Add(mapParent);
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
			IntVec3 spot;
			IntVec3 spot2;
			MultipleCaravansCellFinder.FindStartingCellsFor2Groups(orGenerateMap, out spot, out spot2);
			List<Pawn> lhs2 = ArenaUtility.SpawnPawnSet(orGenerateMap, lhs, spot, Faction.OfSpacer);
			List<Pawn> rhs2 = ArenaUtility.SpawnPawnSet(orGenerateMap, rhs, spot2, Faction.OfSpacerHostile);
			DebugArena component = mapParent.GetComponent<DebugArena>();
			component.lhs = lhs2;
			component.rhs = rhs2;
			component.callback = callback;
		}

		public static List<Pawn> SpawnPawnSet(Map map, List<PawnKindDef> kinds, IntVec3 spot, Faction faction)
		{
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < kinds.Count; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(kinds[i], faction);
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(spot, map, 12, null);
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random, false);
				list.Add(pawn);
			}
			LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(map.Center), map, list);
			return list;
		}

		private static bool ArenaFightQueue(List<PawnKindDef> lhs, List<PawnKindDef> rhs, Action<ArenaUtility.ArenaResult> callback, ArenaUtility.ArenaSetState state)
		{
			if (state.live < 15)
			{
				ArenaUtility.BeginArenaFight(lhs, rhs, delegate(ArenaUtility.ArenaResult result)
				{
					state.live--;
					callback(result);
				});
				state.live++;
				return true;
			}
			return false;
		}

		public static void BeginArenaFightSet(int count, List<PawnKindDef> lhs, List<PawnKindDef> rhs, Action<ArenaUtility.ArenaResult> callback, Action report)
		{
			int remaining = count;
			ArenaUtility.ArenaSetState state = new ArenaUtility.ArenaSetState();
			for (int i = 0; i < count; i++)
			{
				Current.Game.GetComponent<GameComponent_DebugTools>().AddPerFrameCallback(() => ArenaUtility.ArenaFightQueue(lhs, rhs, delegate(ArenaUtility.ArenaResult result)
				{
					callback(result);
					remaining--;
					if (remaining % 10 == 0)
					{
						report();
					}
				}, state));
			}
		}
	}
}
