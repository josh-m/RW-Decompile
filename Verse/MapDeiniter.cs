using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class MapDeiniter
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public static void Deinit(Map map)
		{
			try
			{
				MapDeiniter.DoQueuedPowerTasks(map);
			}
			catch (Exception arg)
			{
				Log.Error("Error while deiniting map: could not execute power related tasks: " + arg, false);
			}
			try
			{
				MapDeiniter.PassPawnsToWorld(map);
			}
			catch (Exception arg2)
			{
				Log.Error("Error while deiniting map: could not pass pawns to world: " + arg2, false);
			}
			try
			{
				map.weatherManager.EndAllSustainers();
			}
			catch (Exception arg3)
			{
				Log.Error("Error while deiniting map: could not end all weather sustainers: " + arg3, false);
			}
			try
			{
				Find.SoundRoot.sustainerManager.EndAllInMap(map);
			}
			catch (Exception arg4)
			{
				Log.Error("Error while deiniting map: could not end all effect sustainers: " + arg4, false);
			}
			try
			{
				map.areaManager.Notify_MapRemoved();
			}
			catch (Exception arg5)
			{
				Log.Error("Error while deiniting map: could not remove areas: " + arg5, false);
			}
			try
			{
				Find.TickManager.RemoveAllFromMap(map);
			}
			catch (Exception arg6)
			{
				Log.Error("Error while deiniting map: could not remove things from the tick manager: " + arg6, false);
			}
			try
			{
				MapDeiniter.NotifyEverythingWhichUsesMapReference(map);
			}
			catch (Exception arg7)
			{
				Log.Error("Error while deiniting map: could not notify things/regions/rooms/etc: " + arg7, false);
			}
			try
			{
				map.listerThings.Clear();
				map.spawnedThings.Clear();
			}
			catch (Exception arg8)
			{
				Log.Error("Error while deiniting map: could not remove things from thing listers: " + arg8, false);
			}
			try
			{
				Find.Archive.Notify_MapRemoved(map);
			}
			catch (Exception arg9)
			{
				Log.Error("Error while deiniting map: could not remove look targets: " + arg9, false);
			}
			try
			{
				Find.Storyteller.incidentQueue.Notify_MapRemoved(map);
			}
			catch (Exception arg10)
			{
				Log.Error("Error while deiniting map: could not remove queued incidents: " + arg10, false);
			}
		}

		private static void DoQueuedPowerTasks(Map map)
		{
			map.powerNetManager.UpdatePowerNetsAndConnections_First();
		}

		private static void PassPawnsToWorld(Map map)
		{
			List<Pawn> list = new List<Pawn>();
			List<Pawn> list2 = new List<Pawn>();
			bool flag = map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer);
			List<Pawn> list3 = map.mapPawns.AllPawns.ToList<Pawn>();
			for (int i = 0; i < list3.Count; i++)
			{
				Find.Storyteller.Notify_PawnEvent(list3[i], AdaptationEvent.LostBecauseMapClosed, null);
				try
				{
					Pawn pawn = list3[i];
					if (pawn.Spawned)
					{
						pawn.DeSpawn(DestroyMode.Vanish);
					}
					if (pawn.IsColonist && flag)
					{
						list.Add(pawn);
						map.ParentFaction.kidnapped.Kidnap(pawn, null);
					}
					else
					{
						if (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer)
						{
							list2.Add(pawn);
							PawnBanishUtility.Banish(pawn, map.Tile);
						}
						MapDeiniter.CleanUpAndPassToWorld(pawn);
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not despawn and pass to world ",
						list3[i],
						": ",
						ex
					}), false);
				}
			}
			if (list.Any<Pawn>() || list2.Any<Pawn>())
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (list.Any<Pawn>())
				{
					list.SortByDescending((Pawn x) => x.RaceProps.Humanlike);
					for (int j = 0; j < list.Count; j++)
					{
						stringBuilder.AppendLine(string.Concat(new string[]
						{
							"  - ",
							list[j].LabelCap,
							" (",
							"capturedBy".Translate(map.ParentFaction.Name),
							")"
						}));
					}
				}
				if (list2.Any<Pawn>())
				{
					list2.SortByDescending((Pawn x) => x.RaceProps.Humanlike);
					for (int k = 0; k < list2.Count; k++)
					{
						stringBuilder.AppendLine("  - " + list2[k].LabelCap);
					}
				}
				string label;
				string text;
				if (map.IsPlayerHome)
				{
					label = "LetterLabelPawnsLostBecauseMapClosed_Home".Translate();
					text = "LetterPawnsLostBecauseMapClosed_Home".Translate();
				}
				else
				{
					label = "LetterLabelPawnsLostBecauseMapClosed_Caravan".Translate();
					text = "LetterPawnsLostBecauseMapClosed_Caravan".Translate();
				}
				text = text + ":\n\n" + stringBuilder.ToString().TrimEndNewlines();
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, new GlobalTargetInfo(map.Tile), null, null);
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

		private static void NotifyEverythingWhichUsesMapReference(Map map)
		{
			List<Map> maps = Find.Maps;
			int num = maps.IndexOf(map);
			ThingOwnerUtility.GetAllThingsRecursively(map, MapDeiniter.tmpThings, true, null);
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
