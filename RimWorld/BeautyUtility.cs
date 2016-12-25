using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class BeautyUtility
	{
		public static List<IntVec3> beautyRelevantCells = new List<IntVec3>();

		private static List<Room> visibleRooms = new List<Room>();

		public static readonly int SampleNumCells_Beauty = GenRadial.NumCellsInRadius(8.9f);

		private static List<Thing> tempCountedThings = new List<Thing>();

		public static float AverageBeautyPerceptible(IntVec3 root, Map map)
		{
			BeautyUtility.tempCountedThings.Clear();
			float num = 0f;
			int num2 = 0;
			BeautyUtility.FillBeautyRelevantCells(root, map);
			for (int i = 0; i < BeautyUtility.beautyRelevantCells.Count; i++)
			{
				num += BeautyUtility.CellBeauty(BeautyUtility.beautyRelevantCells[i], map, BeautyUtility.tempCountedThings);
				num2++;
			}
			return num / (float)num2;
		}

		public static void FillBeautyRelevantCells(IntVec3 root, Map map)
		{
			BeautyUtility.beautyRelevantCells.Clear();
			Room room = RoomQuery.RoomAt(root, map);
			if (room == null)
			{
				return;
			}
			BeautyUtility.visibleRooms.Clear();
			BeautyUtility.visibleRooms.Add(room);
			if (room.Regions.Count == 1 && room.Regions[0].portal != null)
			{
				foreach (Region current in room.Regions[0].Neighbors)
				{
					if (!BeautyUtility.visibleRooms.Contains(current.Room))
					{
						BeautyUtility.visibleRooms.Add(current.Room);
					}
				}
			}
			for (int i = 0; i < BeautyUtility.SampleNumCells_Beauty; i++)
			{
				IntVec3 intVec = root + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && !intVec.Fogged(map))
				{
					Room item = RoomQuery.RoomAt(intVec, map);
					if (!BeautyUtility.visibleRooms.Contains(item))
					{
						bool flag = false;
						for (int j = 0; j < 8; j++)
						{
							IntVec3 loc = intVec + GenAdj.AdjacentCells[j];
							if (BeautyUtility.visibleRooms.Contains(loc.GetRoom(map)))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							goto IL_17B;
						}
					}
					BeautyUtility.beautyRelevantCells.Add(intVec);
				}
				IL_17B:;
			}
		}

		public static float CellBeauty(IntVec3 c, Map map, List<Thing> countedThings = null)
		{
			float num = 0f;
			float num2 = 0f;
			bool flag = false;
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			int i = 0;
			while (i < list.Count)
			{
				Thing thing = list[i];
				if (countedThings == null)
				{
					goto IL_7C;
				}
				bool flag2 = false;
				for (int j = 0; j < countedThings.Count; j++)
				{
					if (thing == countedThings[j])
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					countedThings.Add(thing);
					goto IL_7C;
				}
				IL_D5:
				i++;
				continue;
				IL_7C:
				float num3 = thing.GetStatValue(StatDefOf.Beauty, true);
				if (thing is Filth && !map.roofGrid.Roofed(c))
				{
					num3 *= 0.3f;
				}
				if (thing.def.Fillage == FillCategory.Full)
				{
					flag = true;
					num2 += num3;
					goto IL_D5;
				}
				num += num3;
				goto IL_D5;
			}
			if (flag)
			{
				return num2;
			}
			return num + map.terrainGrid.TerrainAt(c).GetStatValueAbstract(StatDefOf.Beauty, null);
		}
	}
}
