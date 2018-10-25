using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanExitMapUtility
	{
		private static List<int> tmpNeighbors = new List<int>();

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<int> retTiles = new List<int>();

		private static List<int> tileCandidates = new List<int>();

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, Direction8Way dir, int destinationTile, bool sendMessage = true)
		{
			int directionTile = CaravanExitMapUtility.FindRandomStartingTileBasedOnExitDir(exitFromTile, dir);
			return CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, exitFromTile, directionTile, destinationTile, sendMessage);
		}

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile, int destinationTile, bool sendMessage = true)
		{
			if (!GenWorldClosest.TryFindClosestPassableTile(exitFromTile, out exitFromTile))
			{
				Log.Error("Could not find any passable tile for a new caravan.", false);
				return null;
			}
			if (Find.World.Impassable(directionTile))
			{
				directionTile = exitFromTile;
			}
			CaravanExitMapUtility.tmpPawns.Clear();
			CaravanExitMapUtility.tmpPawns.AddRange(pawns);
			Map map = null;
			for (int i = 0; i < CaravanExitMapUtility.tmpPawns.Count; i++)
			{
				CaravanExitMapUtility.AddCaravanExitTaleIfShould(CaravanExitMapUtility.tmpPawns[i]);
				map = CaravanExitMapUtility.tmpPawns[i].MapHeld;
				if (map != null)
				{
					break;
				}
			}
			Caravan caravan = CaravanMaker.MakeCaravan(CaravanExitMapUtility.tmpPawns, faction, exitFromTile, false);
			Rot4 exitDir = (map == null) ? Rot4.Invalid : Find.WorldGrid.GetRotFromTo(exitFromTile, directionTile);
			for (int j = 0; j < CaravanExitMapUtility.tmpPawns.Count; j++)
			{
				CaravanExitMapUtility.tmpPawns[j].ExitMap(false, exitDir);
			}
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int k = 0; k < pawnsListForReading.Count; k++)
			{
				if (!pawnsListForReading[k].IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawnsListForReading[k], PawnDiscardDecideMode.Decide);
				}
			}
			if (map != null)
			{
				map.Parent.Notify_CaravanFormed(caravan);
				map.retainedCaravanData.Notify_CaravanFormed(caravan);
			}
			if (!caravan.pather.Moving && caravan.Tile != directionTile)
			{
				caravan.pather.StartPath(directionTile, null, true, true);
				caravan.pather.nextTileCostLeft /= 2f;
				caravan.tweener.ResetTweenedPosToRoot();
			}
			if (destinationTile != -1)
			{
				List<FloatMenuOption> list = FloatMenuMakerWorld.ChoicesAtFor(destinationTile, caravan);
				if (list.Any((FloatMenuOption x) => !x.Disabled))
				{
					FloatMenuOption floatMenuOption = list.First((FloatMenuOption x) => !x.Disabled);
					floatMenuOption.action();
				}
				else
				{
					caravan.pather.StartPath(destinationTile, null, true, true);
				}
			}
			if (sendMessage)
			{
				string text = "MessageFormedCaravan".Translate(caravan.Name).CapitalizeFirst();
				if (caravan.pather.Moving && caravan.pather.ArrivalAction != null)
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						" ",
						"MessageFormedCaravan_Orders".Translate(),
						": ",
						caravan.pather.ArrivalAction.Label,
						"."
					});
				}
				Messages.Message(text, caravan, MessageTypeDefOf.TaskCompletion, true);
			}
			return caravan;
		}

		public static void ExitMapAndJoinOrCreateCaravan(Pawn pawn, Rot4 exitDir)
		{
			Caravan caravan = CaravanExitMapUtility.FindCaravanToJoinFor(pawn);
			if (caravan != null)
			{
				CaravanExitMapUtility.AddCaravanExitTaleIfShould(pawn);
				caravan.AddPawn(pawn, true);
				pawn.ExitMap(false, exitDir);
			}
			else if (pawn.IsColonist)
			{
				Map map = pawn.Map;
				int directionTile = CaravanExitMapUtility.FindRandomStartingTileBasedOnExitDir(map.Tile, exitDir);
				Caravan caravan2 = CaravanExitMapUtility.ExitMapAndCreateCaravan(Gen.YieldSingle<Pawn>(pawn), pawn.Faction, map.Tile, directionTile, -1, false);
				caravan2.autoJoinable = true;
				bool flag = false;
				List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (CaravanExitMapUtility.FindCaravanToJoinFor(allPawnsSpawned[i]) != null && !allPawnsSpawned[i].Downed && !allPawnsSpawned[i].Drafted)
					{
						if (allPawnsSpawned[i].RaceProps.Animal)
						{
							flag = true;
						}
						RestUtility.WakeUp(allPawnsSpawned[i]);
						allPawnsSpawned[i].jobs.CheckForJobOverride();
					}
				}
				string text = "MessagePawnLeftMapAndCreatedCaravan".Translate(pawn.LabelShort, pawn).CapitalizeFirst();
				if (flag)
				{
					text = text + " " + "MessagePawnLeftMapAndCreatedCaravan_AnimalsWantToJoin".Translate();
				}
				Messages.Message(text, caravan2, MessageTypeDefOf.TaskCompletion, true);
			}
			else
			{
				Log.Error("Pawn " + pawn + " didn't find any caravan to join, and he can't create one.", false);
			}
		}

		public static bool CanExitMapAndJoinOrCreateCaravanNow(Pawn pawn)
		{
			return pawn.Spawned && pawn.Map.exitMapGrid.MapUsesExitGrid && (pawn.IsColonist || CaravanExitMapUtility.FindCaravanToJoinFor(pawn) != null);
		}

		public static List<int> AvailableExitTilesAt(Map map)
		{
			CaravanExitMapUtility.retTiles.Clear();
			int currentTileID = map.Tile;
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(currentTileID, CaravanExitMapUtility.tmpNeighbors);
			for (int i = 0; i < CaravanExitMapUtility.tmpNeighbors.Count; i++)
			{
				int num = CaravanExitMapUtility.tmpNeighbors[i];
				if (CaravanExitMapUtility.IsGoodCaravanStartingTile(num))
				{
					Rot4 rotFromTo = grid.GetRotFromTo(currentTileID, num);
					IntVec3 intVec;
					if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Walkable(map) && !x.Fogged(map), map, rotFromTo, CellFinder.EdgeRoadChance_Ignore, out intVec))
					{
						CaravanExitMapUtility.retTiles.Add(num);
					}
				}
			}
			CaravanExitMapUtility.retTiles.SortBy((int x) => grid.GetHeadingFromTo(currentTileID, x));
			return CaravanExitMapUtility.retTiles;
		}

		public static int RandomBestExitTileFrom(Map map)
		{
			Tile tileInfo = map.TileInfo;
			List<int> options = CaravanExitMapUtility.AvailableExitTilesAt(map);
			if (!options.Any<int>())
			{
				return -1;
			}
			List<Tile.RoadLink> roads = tileInfo.Roads;
			if (roads == null)
			{
				return options.RandomElement<int>();
			}
			int bestRoadIndex = -1;
			for (int i = 0; i < roads.Count; i++)
			{
				if (options.Contains(roads[i].neighbor))
				{
					if (bestRoadIndex == -1 || roads[i].road.priority > roads[bestRoadIndex].road.priority)
					{
						bestRoadIndex = i;
					}
				}
			}
			if (bestRoadIndex == -1)
			{
				return options.RandomElement<int>();
			}
			return (from rl in roads
			where options.Contains(rl.neighbor) && rl.road == roads[bestRoadIndex].road
			select rl).RandomElement<Tile.RoadLink>().neighbor;
		}

		public static int BestExitTileToGoTo(int destinationTile, Map from)
		{
			int num = -1;
			using (WorldPath worldPath = Find.WorldPathFinder.FindPath(from.Tile, destinationTile, null, null))
			{
				if (worldPath.Found && worldPath.NodesLeftCount >= 2)
				{
					num = worldPath.NodesReversed[worldPath.NodesReversed.Count - 2];
				}
			}
			if (num == -1)
			{
				return CaravanExitMapUtility.RandomBestExitTileFrom(from);
			}
			float num2 = 0f;
			int num3 = -1;
			List<int> list = CaravanExitMapUtility.AvailableExitTilesAt(from);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == num)
				{
					return list[i];
				}
				float num4 = (Find.WorldGrid.GetTileCenter(list[i]) - Find.WorldGrid.GetTileCenter(num)).MagnitudeHorizontalSquared();
				if (num3 == -1 || num4 < num2)
				{
					num3 = list[i];
					num2 = num4;
				}
			}
			return num3;
		}

		private static int FindRandomStartingTileBasedOnExitDir(int tileID, Rot4 exitDir)
		{
			CaravanExitMapUtility.tileCandidates.Clear();
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(tileID, CaravanExitMapUtility.tmpNeighbors);
			for (int i = 0; i < CaravanExitMapUtility.tmpNeighbors.Count; i++)
			{
				int num = CaravanExitMapUtility.tmpNeighbors[i];
				if (CaravanExitMapUtility.IsGoodCaravanStartingTile(num))
				{
					if (!exitDir.IsValid || !(grid.GetRotFromTo(tileID, num) != exitDir))
					{
						CaravanExitMapUtility.tileCandidates.Add(num);
					}
				}
			}
			int result;
			if (CaravanExitMapUtility.tileCandidates.TryRandomElement(out result))
			{
				return result;
			}
			if (CaravanExitMapUtility.tmpNeighbors.Where(delegate(int x)
			{
				if (!CaravanExitMapUtility.IsGoodCaravanStartingTile(x))
				{
					return false;
				}
				Rot4 rotFromTo = grid.GetRotFromTo(tileID, x);
				return ((exitDir == Rot4.North || exitDir == Rot4.South) && (rotFromTo == Rot4.East || rotFromTo == Rot4.West)) || ((exitDir == Rot4.East || exitDir == Rot4.West) && (rotFromTo == Rot4.North || rotFromTo == Rot4.South));
			}).TryRandomElement(out result))
			{
				return result;
			}
			if (CaravanExitMapUtility.tmpNeighbors.Where(new Func<int, bool>(CaravanExitMapUtility.IsGoodCaravanStartingTile)).TryRandomElement(out result))
			{
				return result;
			}
			return tileID;
		}

		private static int FindRandomStartingTileBasedOnExitDir(int tileID, Direction8Way exitDir)
		{
			CaravanExitMapUtility.tileCandidates.Clear();
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(tileID, CaravanExitMapUtility.tmpNeighbors);
			for (int i = 0; i < CaravanExitMapUtility.tmpNeighbors.Count; i++)
			{
				int num = CaravanExitMapUtility.tmpNeighbors[i];
				if (CaravanExitMapUtility.IsGoodCaravanStartingTile(num))
				{
					if (grid.GetDirection8WayFromTo(tileID, num) == exitDir)
					{
						CaravanExitMapUtility.tileCandidates.Add(num);
					}
				}
			}
			int result;
			if (CaravanExitMapUtility.tileCandidates.TryRandomElement(out result))
			{
				return result;
			}
			if (CaravanExitMapUtility.tmpNeighbors.Where(new Func<int, bool>(CaravanExitMapUtility.IsGoodCaravanStartingTile)).TryRandomElement(out result))
			{
				return result;
			}
			return tileID;
		}

		private static bool IsGoodCaravanStartingTile(int tile)
		{
			return !Find.World.Impassable(tile);
		}

		public static Caravan FindCaravanToJoinFor(Pawn pawn)
		{
			if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
			{
				return null;
			}
			if (!pawn.Spawned || !pawn.CanReachMapEdge())
			{
				return null;
			}
			int tile = pawn.Map.Tile;
			Find.WorldGrid.GetTileNeighbors(tile, CaravanExitMapUtility.tmpNeighbors);
			CaravanExitMapUtility.tmpNeighbors.Add(tile);
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (CaravanExitMapUtility.tmpNeighbors.Contains(caravan.Tile))
				{
					if (caravan.autoJoinable)
					{
						if (pawn.HostFaction == null)
						{
							if (caravan.Faction == pawn.Faction)
							{
								return caravan;
							}
						}
						else if (caravan.Faction == pawn.HostFaction)
						{
							return caravan;
						}
					}
				}
			}
			return null;
		}

		public static bool AnyoneTryingToJoinCaravan(Caravan c)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (!map.IsPlayerHome && Find.WorldGrid.IsNeighborOrSame(c.Tile, map.Tile))
				{
					List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
					for (int j = 0; j < allPawnsSpawned.Count; j++)
					{
						if (!allPawnsSpawned[j].IsColonistPlayerControlled && !allPawnsSpawned[j].Downed && CaravanExitMapUtility.FindCaravanToJoinFor(allPawnsSpawned[j]) == c)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static void OpenSomeoneTryingToJoinCaravanDialog(Caravan c, Action confirmAction)
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmMoveAutoJoinableCaravan".Translate(), confirmAction, false, null));
		}

		private static void AddCaravanExitTaleIfShould(Pawn pawn)
		{
			if (pawn.Spawned && pawn.IsFreeColonist)
			{
				if (pawn.Map.IsPlayerHome)
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanFormed, new object[]
					{
						pawn
					});
				}
				else if (GenHostility.AnyHostileActiveThreatToPlayer(pawn.Map))
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanFled, new object[]
					{
						pawn
					});
				}
			}
		}
	}
}
