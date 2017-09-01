using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class MapDeiniter
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public static void Deinit(Map map)
		{
			try
			{
				MapDeiniter.PassPawnsToWorld(map);
			}
			catch (Exception arg)
			{
				Log.Error("Error while deiniting map: could not pass pawns to world: " + arg);
			}
			try
			{
				MapDeiniter.NotifyFactions(map);
			}
			catch (Exception arg2)
			{
				Log.Error("Error while deiniting map: could not notify factions: " + arg2);
			}
			try
			{
				map.weatherManager.EndAllSustainers();
			}
			catch (Exception arg3)
			{
				Log.Error("Error while deiniting map: could not end all weather sustainers: " + arg3);
			}
			try
			{
				Find.SoundRoot.sustainerManager.RemoveAllFromMap(map);
			}
			catch (Exception arg4)
			{
				Log.Error("Error while deiniting map: could not end all effect sustainers: " + arg4);
			}
			try
			{
				Find.TickManager.RemoveAllFromMap(map);
			}
			catch (Exception arg5)
			{
				Log.Error("Error while deiniting map: could not remove things from the tick manager: " + arg5);
			}
			try
			{
				MapDeiniter.NotifyEverythingWhichUsesMapReference(map);
			}
			catch (Exception arg6)
			{
				Log.Error("Error while deiniting map: could not notify things/regions/rooms/etc: " + arg6);
			}
		}

		private static void PassPawnsToWorld(Map map)
		{
			bool flag = map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer);
			List<Pawn> list = map.mapPawns.AllPawns.ToList<Pawn>();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					Pawn pawn = list[i];
					if (pawn.Spawned)
					{
						pawn.DeSpawn();
					}
					if (pawn.IsColonist && flag)
					{
						map.ParentFaction.kidnapped.KidnapPawn(pawn, null);
					}
					else
					{
						MapDeiniter.CleanUpAndPassToWorld(pawn);
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not despawn and pass to world ",
						list[i],
						": ",
						ex
					}));
				}
			}
		}

		private static void CleanUpAndPassToWorld(Pawn p)
		{
			if (p.ownership != null)
			{
				p.ownership.UnclaimAll();
			}
			if (p.guest != null)
			{
				p.guest.SetGuestStatus(null, false);
			}
			p.inventory.UnloadEverything = false;
			Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.Decide);
		}

		private static void NotifyFactions(Map map)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				allFactionsListForReading[i].Notify_MapRemoved(map);
			}
		}

		private static void NotifyEverythingWhichUsesMapReference(Map map)
		{
			List<Map> maps = Find.Maps;
			int num = maps.IndexOf(map);
			ThingOwnerUtility.GetAllThingsRecursively(map, MapDeiniter.tmpThings, true);
			for (int i = 0; i < MapDeiniter.tmpThings.Count; i++)
			{
				MapDeiniter.tmpThings[i].Notify_MyMapRemoved();
			}
			MapDeiniter.tmpThings.Clear();
			for (int j = num; j < maps.Count; j++)
			{
				ThingOwner spawnedThings = maps[j].spawnedThings;
				for (int k = 0; k < spawnedThings.Count; k++)
				{
					if (j != num)
					{
						spawnedThings[k].DecrementMapIndex();
					}
				}
				List<Room> allRooms = maps[j].regionGrid.allRooms;
				for (int l = 0; l < allRooms.Count; l++)
				{
					if (j == num)
					{
						allRooms[l].Notify_MyMapRemoved();
					}
					else
					{
						allRooms[l].DecrementMapIndex();
					}
				}
				foreach (Region current in maps[j].regionGrid.AllRegions_NoRebuild_InvalidAllowed)
				{
					if (j == num)
					{
						current.Notify_MyMapRemoved();
					}
					else
					{
						current.DecrementMapIndex();
					}
				}
			}
		}
	}
}
