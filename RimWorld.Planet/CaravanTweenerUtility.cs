using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanTweenerUtility
	{
		private const float BaseRadius = 0.15f;

		private const float BaseDistToCollide = 0.2f;

		public static Vector3 PatherTweenedPosRoot(Caravan caravan)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			if (!caravan.Spawned)
			{
				return worldGrid.GetTileCenter(caravan.Tile);
			}
			if (caravan.pather.Moving)
			{
				float num;
				if (!caravan.pather.IsNextTilePassable())
				{
					num = 0f;
				}
				else
				{
					num = 1f - caravan.pather.nextTileCostLeft / caravan.pather.nextTileCostTotal;
				}
				return worldGrid.GetTileCenter(caravan.pather.nextTile) * num + worldGrid.GetTileCenter(caravan.Tile) * (1f - num);
			}
			return worldGrid.GetTileCenter(caravan.Tile);
		}

		public static Vector3 CaravanCollisionPosOffsetFor(Caravan caravan)
		{
			if (!caravan.Spawned)
			{
				return Vector3.zero;
			}
			bool flag = caravan.Spawned && caravan.pather.Moving;
			float d = 0.15f * Find.WorldGrid.averageTileSize;
			if (!flag || caravan.pather.nextTile == caravan.pather.Destination)
			{
				int num;
				if (flag)
				{
					num = caravan.pather.nextTile;
				}
				else
				{
					num = caravan.Tile;
				}
				int num2 = 0;
				int vertexIndex = 0;
				CaravanTweenerUtility.GetCaravansStandingAtOrAboutToStandAt(num, out num2, out vertexIndex, caravan);
				if (num2 == 0)
				{
					return Vector3.zero;
				}
				return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(Find.WorldGrid.GetTileCenter(num), GenGeo.RegularPolygonVertexPosition(num2, vertexIndex) * d);
			}
			else
			{
				if (CaravanTweenerUtility.DrawPosCollides(caravan))
				{
					Rand.PushSeed();
					Rand.Seed = caravan.ID;
					float f = Rand.Range(0f, 360f);
					Rand.PopSeed();
					Vector2 point = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * d;
					return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(CaravanTweenerUtility.PatherTweenedPosRoot(caravan), point);
				}
				return Vector3.zero;
			}
		}

		private static void GetCaravansStandingAtOrAboutToStandAt(int tile, out int caravansCount, out int caravansWithLowerIdCount, Caravan forCaravan)
		{
			caravansCount = 0;
			caravansWithLowerIdCount = 0;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			int i = 0;
			while (i < caravans.Count)
			{
				Caravan caravan = caravans[i];
				if (caravan.Tile != tile)
				{
					if (caravan.pather.Moving && caravan.pather.nextTile == caravan.pather.Destination && caravan.pather.Destination == tile)
					{
						goto IL_87;
					}
				}
				else if (!caravan.pather.Moving)
				{
					goto IL_87;
				}
				IL_A4:
				i++;
				continue;
				IL_87:
				caravansCount++;
				if (caravan.ID < forCaravan.ID)
				{
					caravansWithLowerIdCount++;
					goto IL_A4;
				}
				goto IL_A4;
			}
		}

		private static bool DrawPosCollides(Caravan caravan)
		{
			Vector3 a = CaravanTweenerUtility.PatherTweenedPosRoot(caravan);
			float num = Find.WorldGrid.averageTileSize * 0.2f;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan2 = caravans[i];
				if (caravan2 != caravan)
				{
					if (Vector3.Distance(a, CaravanTweenerUtility.PatherTweenedPosRoot(caravan2)) < num)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
