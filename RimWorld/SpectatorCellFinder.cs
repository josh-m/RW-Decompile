using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class SpectatorCellFinder
	{
		private const float MaxDistanceToSpectateRect = 14.5f;

		private static float[] scorePerSide = new float[4];

		private static List<IntVec3> usedCells = new List<IntVec3>();

		public static bool TryFindSpectatorCellFor(Pawn p, CellRect spectateRect, Map map, out IntVec3 cell, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1, List<IntVec3> extraDisallowedCells = null)
		{
			spectateRect.ClipInsideMap(map);
			if (spectateRect.Area == 0 || allowedSides == SpectateRectSide.None)
			{
				cell = IntVec3.Invalid;
				return false;
			}
			CellRect rectWithMargin = spectateRect.ExpandedBy(margin).ClipInsideMap(map);
			Predicate<IntVec3> predicate = delegate(IntVec3 x)
			{
				if (!x.InBounds(map))
				{
					return false;
				}
				if (!x.Standable(map))
				{
					return false;
				}
				if (x.Fogged(map))
				{
					return false;
				}
				if (rectWithMargin.Contains(x))
				{
					return false;
				}
				if ((x.z <= rectWithMargin.maxZ || (allowedSides & SpectateRectSide.Up) != SpectateRectSide.Up) && (x.x <= rectWithMargin.maxX || (allowedSides & SpectateRectSide.Right) != SpectateRectSide.Right) && (x.z >= rectWithMargin.minZ || (allowedSides & SpectateRectSide.Down) != SpectateRectSide.Down) && (x.x >= rectWithMargin.minX || (allowedSides & SpectateRectSide.Left) != SpectateRectSide.Left))
				{
					return false;
				}
				IntVec3 intVec3 = spectateRect.ClosestCellTo(x);
				if (intVec3.DistanceToSquared(x) > 210.25f)
				{
					return false;
				}
				if (!GenSight.LineOfSight(intVec3, x, map, true))
				{
					return false;
				}
				if (x.GetThingList(map).Find((Thing y) => y is Pawn && y != p) != null)
				{
					return false;
				}
				if (p != null)
				{
					if (!p.CanReserveAndReach(x, PathEndMode.OnCell, Danger.Some, 1))
					{
						return false;
					}
					Building edifice = x.GetEdifice(map);
					if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable && !p.CanReserve(edifice, 1))
					{
						return false;
					}
					if (x.IsForbidden(p))
					{
						return false;
					}
					if (x.GetDangerFor(p) != Danger.None)
					{
						return false;
					}
				}
				if (extraDisallowedCells != null && extraDisallowedCells.Contains(x))
				{
					return false;
				}
				if (!SpectatorCellFinder.CorrectlyRotatedChairAt(x, map, spectateRect))
				{
					int num = 0;
					for (int k = 0; k < GenAdj.AdjacentCells.Length; k++)
					{
						IntVec3 x2 = x + GenAdj.AdjacentCells[k];
						if (SpectatorCellFinder.CorrectlyRotatedChairAt(x2, map, spectateRect))
						{
							num++;
						}
					}
					if (num >= 3)
					{
						return false;
					}
					int num2 = SpectatorCellFinder.DistanceToClosestChair(x, new IntVec3(-1, 0, 0), map, 4, spectateRect);
					if (num2 >= 0)
					{
						int num3 = SpectatorCellFinder.DistanceToClosestChair(x, new IntVec3(1, 0, 0), map, 4, spectateRect);
						if (num3 >= 0 && Mathf.Abs(num2 - num3) <= 1)
						{
							return false;
						}
					}
					int num4 = SpectatorCellFinder.DistanceToClosestChair(x, new IntVec3(0, 0, 1), map, 4, spectateRect);
					if (num4 >= 0)
					{
						int num5 = SpectatorCellFinder.DistanceToClosestChair(x, new IntVec3(0, 0, -1), map, 4, spectateRect);
						if (num5 >= 0 && Mathf.Abs(num4 - num5) <= 1)
						{
							return false;
						}
					}
				}
				return true;
			};
			if (p != null && predicate(p.Position) && SpectatorCellFinder.CorrectlyRotatedChairAt(p.Position, map, spectateRect))
			{
				cell = p.Position;
				return true;
			}
			for (int i = 0; i < 1000; i++)
			{
				IntVec3 intVec = rectWithMargin.CenterCell + GenRadial.RadialPattern[i];
				if (predicate(intVec))
				{
					if (!SpectatorCellFinder.CorrectlyRotatedChairAt(intVec, map, spectateRect))
					{
						for (int j = 0; j < 90; j++)
						{
							IntVec3 intVec2 = intVec + GenRadial.RadialPattern[j];
							if (SpectatorCellFinder.CorrectlyRotatedChairAt(intVec2, map, spectateRect) && predicate(intVec2))
							{
								cell = intVec2;
								return true;
							}
						}
					}
					cell = intVec;
					return true;
				}
			}
			cell = IntVec3.Invalid;
			return false;
		}

		private static bool CorrectlyRotatedChairAt(IntVec3 x, Map map, CellRect spectateRect)
		{
			return SpectatorCellFinder.GetCorrectlyRotatedChairAt(x, map, spectateRect) != null;
		}

		private static Building GetCorrectlyRotatedChairAt(IntVec3 x, Map map, CellRect spectateRect)
		{
			if (!x.InBounds(map))
			{
				return null;
			}
			Building edifice = x.GetEdifice(map);
			if (edifice == null || edifice.def.category != ThingCategory.Building || !edifice.def.building.isSittable)
			{
				return null;
			}
			float num = GenGeo.AngleDifferenceBetween(edifice.Rotation.AsAngle, (spectateRect.ClosestCellTo(x) - edifice.Position).AngleFlat);
			if (num > 75f)
			{
				return null;
			}
			return edifice;
		}

		private static int DistanceToClosestChair(IntVec3 from, IntVec3 step, Map map, int maxDist, CellRect spectateRect)
		{
			int num = 0;
			IntVec3 intVec = from;
			while (true)
			{
				intVec += step;
				num++;
				if (!intVec.InBounds(map))
				{
					break;
				}
				if (SpectatorCellFinder.CorrectlyRotatedChairAt(intVec, map, spectateRect))
				{
					return num;
				}
				if (!intVec.Walkable(map))
				{
					return -1;
				}
				if (num >= maxDist)
				{
					return -1;
				}
			}
			return -1;
		}

		public static void DebugFlashPotentialSpectatorCells(CellRect spectateRect, Map map, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1)
		{
			List<IntVec3> list = new List<IntVec3>();
			int num = 50;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec;
				if (!SpectatorCellFinder.TryFindSpectatorCellFor(null, spectateRect, map, out intVec, allowedSides, margin, list))
				{
					break;
				}
				list.Add(intVec);
				float a = Mathf.Lerp(1f, 0.08f, (float)i / (float)num);
				Material mat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0.8f, 0f, a));
				map.debugDrawer.FlashCell(intVec, mat, (i + 1).ToString());
			}
			SpectateRectSide spectateRectSide = SpectatorCellFinder.FindSingleBestSide(spectateRect, map, allowedSides, margin);
			IntVec3 centerCell = spectateRect.CenterCell;
			switch (spectateRectSide)
			{
			case SpectateRectSide.Up:
				centerCell.z += spectateRect.Height / 2 + 10;
				break;
			case SpectateRectSide.Right:
				centerCell.x += spectateRect.Width / 2 + 10;
				break;
			case SpectateRectSide.Down:
				centerCell.z -= spectateRect.Height / 2 + 10;
				break;
			case SpectateRectSide.Left:
				centerCell.x -= spectateRect.Width / 2 + 10;
				break;
			}
			map.debugDrawer.FlashLine(spectateRect.CenterCell, centerCell);
		}

		public static SpectateRectSide FindSingleBestSide(CellRect spectateRect, Map map, SpectateRectSide allowedSides = SpectateRectSide.All, int margin = 1)
		{
			for (int i = 0; i < SpectatorCellFinder.scorePerSide.Length; i++)
			{
				SpectatorCellFinder.scorePerSide[i] = 0f;
			}
			SpectatorCellFinder.usedCells.Clear();
			int num = 30;
			CellRect cellRect = spectateRect.ExpandedBy(margin).ClipInsideMap(map);
			for (int j = 0; j < num; j++)
			{
				IntVec3 intVec;
				if (!SpectatorCellFinder.TryFindSpectatorCellFor(null, spectateRect, map, out intVec, allowedSides, margin, SpectatorCellFinder.usedCells))
				{
					break;
				}
				SpectatorCellFinder.usedCells.Add(intVec);
				float num2 = Mathf.Lerp(1f, 0.35f, (float)j / (float)num);
				float num3 = num2;
				Building correctlyRotatedChairAt = SpectatorCellFinder.GetCorrectlyRotatedChairAt(intVec, map, spectateRect);
				if (intVec.z > cellRect.maxZ && (allowedSides & SpectateRectSide.Up) == SpectateRectSide.Up)
				{
					SpectatorCellFinder.scorePerSide[0] += num3;
					if (correctlyRotatedChairAt != null && correctlyRotatedChairAt.Rotation == Rot4.South)
					{
						SpectatorCellFinder.scorePerSide[0] += 1.2f * num2;
					}
				}
				if (intVec.x > cellRect.maxX && (allowedSides & SpectateRectSide.Right) == SpectateRectSide.Right)
				{
					SpectatorCellFinder.scorePerSide[1] += num3;
					if (correctlyRotatedChairAt != null && correctlyRotatedChairAt.Rotation == Rot4.West)
					{
						SpectatorCellFinder.scorePerSide[1] += 1.2f * num2;
					}
				}
				if (intVec.z < cellRect.minZ && (allowedSides & SpectateRectSide.Down) == SpectateRectSide.Down)
				{
					SpectatorCellFinder.scorePerSide[2] += num3;
					if (correctlyRotatedChairAt != null && correctlyRotatedChairAt.Rotation == Rot4.North)
					{
						SpectatorCellFinder.scorePerSide[2] += 1.2f * num2;
					}
				}
				if (intVec.x < cellRect.minX && (allowedSides & SpectateRectSide.Left) == SpectateRectSide.Left)
				{
					SpectatorCellFinder.scorePerSide[3] += num3;
					if (correctlyRotatedChairAt != null && correctlyRotatedChairAt.Rotation == Rot4.East)
					{
						SpectatorCellFinder.scorePerSide[3] += 1.2f * num2;
					}
				}
			}
			float num4 = 0f;
			int num5 = -1;
			for (int k = 0; k < SpectatorCellFinder.scorePerSide.Length; k++)
			{
				if (SpectatorCellFinder.scorePerSide[k] != 0f)
				{
					if (num5 < 0 || SpectatorCellFinder.scorePerSide[k] > num4)
					{
						num5 = k;
						num4 = SpectatorCellFinder.scorePerSide[k];
					}
				}
			}
			SpectatorCellFinder.usedCells.Clear();
			switch (num5)
			{
			case 0:
				return SpectateRectSide.Up;
			case 1:
				return SpectateRectSide.Right;
			case 2:
				return SpectateRectSide.Down;
			case 3:
				return SpectateRectSide.Left;
			default:
				return SpectateRectSide.None;
			}
		}
	}
}
