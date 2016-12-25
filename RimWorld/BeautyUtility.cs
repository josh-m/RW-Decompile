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

		public static readonly int SampleNumCells_Space = GenRadial.NumCellsInRadius(4.9f);

		private static List<Thing> tempCountedThings = new List<Thing>();

		private static List<Room> scanRooms = new List<Room>();

		public static float AverageBeautyPerceptible(IntVec3 root)
		{
			BeautyUtility.tempCountedThings.Clear();
			float num = 0f;
			int num2 = 0;
			BeautyUtility.FillBeautyRelevantCells(root);
			for (int i = 0; i < BeautyUtility.beautyRelevantCells.Count; i++)
			{
				num += BeautyUtility.CellBeauty(BeautyUtility.beautyRelevantCells[i], BeautyUtility.tempCountedThings);
				num2++;
			}
			return num / (float)num2;
		}

		public static void FillBeautyRelevantCells(IntVec3 root)
		{
			BeautyUtility.beautyRelevantCells.Clear();
			Room room = RoomQuery.RoomAt(root);
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
				if (intVec.InBounds() && !intVec.Fogged())
				{
					Room item = RoomQuery.RoomAt(intVec);
					if (!BeautyUtility.visibleRooms.Contains(item))
					{
						bool flag = false;
						for (int j = 0; j < 8; j++)
						{
							IntVec3 loc = intVec + GenAdj.AdjacentCells[j];
							if (BeautyUtility.visibleRooms.Contains(loc.GetRoom()))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							goto IL_176;
						}
					}
					BeautyUtility.beautyRelevantCells.Add(intVec);
				}
				IL_176:;
			}
		}

		public static float CellBeauty(IntVec3 c, List<Thing> countedThings = null)
		{
			float num = 0f;
			float num2 = 0f;
			bool flag = false;
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
			int i = 0;
			while (i < list.Count)
			{
				Thing thing = list[i];
				if (countedThings == null)
				{
					goto IL_7B;
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
					goto IL_7B;
				}
				IL_D3:
				i++;
				continue;
				IL_7B:
				float num3 = thing.GetStatValue(StatDefOf.Beauty, true);
				if (thing is Filth && !Find.RoofGrid.Roofed(c))
				{
					num3 *= 0.3f;
				}
				if (thing.def.Fillage == FillCategory.Full)
				{
					flag = true;
					num2 += num3;
					goto IL_D3;
				}
				num += num3;
				goto IL_D3;
			}
			if (flag)
			{
				return num2;
			}
			return num + Find.TerrainGrid.TerrainAt(c).GetStatValueAbstract(StatDefOf.Beauty, null);
		}

		public static float SpacePerceptible(IntVec3 root)
		{
			BeautyUtility.scanRooms.Clear();
			for (int i = 0; i < 5; i++)
			{
				IntVec3 loc = root + GenRadial.RadialPattern[i];
				Room room = loc.GetRoom();
				if (room != null && !BeautyUtility.scanRooms.Contains(room))
				{
					BeautyUtility.scanRooms.Add(room);
				}
			}
			float num = (float)BeautyUtility.SampleNumCells_Space;
			for (int j = 0; j < BeautyUtility.SampleNumCells_Space; j++)
			{
				IntVec3 c = root + GenRadial.RadialPattern[j];
				if (!BeautyUtility.scanRooms.Contains(RoomQuery.RoomAt(c)))
				{
					num -= 1f;
				}
				else if (!c.Standable())
				{
					num -= 0.5f;
				}
				else if (!c.Walkable())
				{
					num -= 1f;
				}
			}
			return num / (float)BeautyUtility.SampleNumCells_Space;
		}
	}
}
