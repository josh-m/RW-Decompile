using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Street : SymbolResolver
	{
		private static List<bool> street = new List<bool>();

		public override void Resolve(ResolveParams rp)
		{
			bool? streetHorizontal = rp.streetHorizontal;
			bool flag = (!streetHorizontal.HasValue) ? (rp.rect.Width >= rp.rect.Height) : streetHorizontal.Value;
			int width = (!flag) ? rp.rect.Width : rp.rect.Height;
			TerrainDef floorDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, false);
			this.CalculateStreet(rp.rect, flag, floorDef);
			this.FillStreetGaps(flag, width);
			this.RemoveShortStreetParts(flag, width);
			this.SpawnFloor(rp.rect, flag, floorDef);
		}

		private void CalculateStreet(CellRect rect, bool horizontal, TerrainDef floorDef)
		{
			SymbolResolver_Street.street.Clear();
			int num = (!horizontal) ? rect.Height : rect.Width;
			for (int i = 0; i < num; i++)
			{
				if (horizontal)
				{
					SymbolResolver_Street.street.Add(this.CausesStreet(new IntVec3(rect.minX + i, 0, rect.minZ - 1), floorDef) && this.CausesStreet(new IntVec3(rect.minX + i, 0, rect.maxZ + 1), floorDef));
				}
				else
				{
					SymbolResolver_Street.street.Add(this.CausesStreet(new IntVec3(rect.minX - 1, 0, rect.minZ + i), floorDef) && this.CausesStreet(new IntVec3(rect.maxX + 1, 0, rect.minZ + i), floorDef));
				}
			}
		}

		private void FillStreetGaps(bool horizontal, int width)
		{
			int num = -1;
			for (int i = 0; i < SymbolResolver_Street.street.Count; i++)
			{
				if (SymbolResolver_Street.street[i])
				{
					num = i;
				}
				else if (num != -1 && i - num <= width)
				{
					for (int j = i + 1; j < i + width + 1; j++)
					{
						if (j >= SymbolResolver_Street.street.Count)
						{
							break;
						}
						if (SymbolResolver_Street.street[j])
						{
							SymbolResolver_Street.street[i] = true;
							break;
						}
					}
				}
			}
		}

		private void RemoveShortStreetParts(bool horizontal, int width)
		{
			for (int i = 0; i < SymbolResolver_Street.street.Count; i++)
			{
				if (SymbolResolver_Street.street[i])
				{
					int num = 0;
					for (int j = i; j < SymbolResolver_Street.street.Count; j++)
					{
						if (!SymbolResolver_Street.street[j])
						{
							break;
						}
						num++;
					}
					int num2 = 0;
					for (int k = i; k >= 0; k--)
					{
						if (!SymbolResolver_Street.street[k])
						{
							break;
						}
						num2++;
					}
					int num3 = num2 + num - 1;
					if (num3 < width)
					{
						SymbolResolver_Street.street[i] = false;
					}
				}
			}
		}

		private void SpawnFloor(CellRect rect, bool horizontal, TerrainDef floorDef)
		{
			Map map = BaseGen.globalSettings.map;
			TerrainGrid terrainGrid = map.terrainGrid;
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if ((horizontal && SymbolResolver_Street.street[current.x - rect.minX]) || (!horizontal && SymbolResolver_Street.street[current.z - rect.minZ]))
				{
					terrainGrid.SetTerrain(current, floorDef);
				}
				iterator.MoveNext();
			}
		}

		private bool CausesStreet(IntVec3 c, TerrainDef floorDef)
		{
			Map map = BaseGen.globalSettings.map;
			if (!c.InBounds(map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			return (edifice != null && edifice.def == ThingDefOf.Wall) || c.GetDoor(map) != null || c.GetTerrain(map) == floorDef;
		}
	}
}
