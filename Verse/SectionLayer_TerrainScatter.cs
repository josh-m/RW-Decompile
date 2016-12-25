using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class SectionLayer_TerrainScatter : SectionLayer
	{
		private class Scatterable
		{
			public ScatterableDef def;

			public Vector3 loc;

			public float size;

			public float rotation;

			public bool IsOnValidTerrain
			{
				get
				{
					IntVec3 c = this.loc.ToIntVec3();
					TerrainDef terrainDef = Find.TerrainGrid.TerrainAt(c);
					return this.def.scatterType == terrainDef.scatterType && !c.Filled();
				}
			}

			public Scatterable(ScatterableDef def, Vector3 loc)
			{
				this.def = def;
				this.loc = loc;
				this.size = Rand.Range(def.minSize, def.maxSize);
				this.rotation = Rand.Range(0f, 360f);
			}

			public void PrintOnto(SectionLayer layer)
			{
				Printer_Plane.PrintPlane(layer, this.loc, Vector2.one * this.size, this.def.mat, this.rotation, false, null, null, 0.01f);
			}
		}

		private List<SectionLayer_TerrainScatter.Scatterable> scats = new List<SectionLayer_TerrainScatter.Scatterable>();

		public override bool Visible
		{
			get
			{
				return DebugViewSettings.drawTerrain;
			}
		}

		public SectionLayer_TerrainScatter(Section section) : base(section)
		{
			this.relevantChangeTypes = MapMeshFlag.Terrain;
		}

		public override void Regenerate()
		{
			base.ClearSubMeshes(MeshParts.All);
			this.scats.RemoveAll((SectionLayer_TerrainScatter.Scatterable scat) => !scat.IsOnValidTerrain);
			int num = 0;
			TerrainDef[] topGrid = Find.TerrainGrid.topGrid;
			CellRect cellRect = this.section.CellRect;
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					if (topGrid[CellIndices.CellToIndex(j, i)].scatterType != null)
					{
						num++;
					}
				}
			}
			num /= 40;
			int num2 = 0;
			while (this.scats.Count < num && num2 < 200)
			{
				num2++;
				IntVec3 randomCell = this.section.CellRect.RandomCell;
				string terrScatType = Find.TerrainGrid.TerrainAt(randomCell).scatterType;
				if (terrScatType != null && !randomCell.Filled())
				{
					ScatterableDef def2;
					if ((from def in DefDatabase<ScatterableDef>.AllDefs
					where def.scatterType == terrScatType
					select def).TryRandomElement(out def2))
					{
						Vector3 loc = new Vector3((float)randomCell.x + Rand.Value, (float)randomCell.y, (float)randomCell.z + Rand.Value);
						SectionLayer_TerrainScatter.Scatterable scatterable = new SectionLayer_TerrainScatter.Scatterable(def2, loc);
						this.scats.Add(scatterable);
						scatterable.PrintOnto(this);
					}
				}
			}
			for (int k = 0; k < this.scats.Count; k++)
			{
				this.scats[k].PrintOnto(this);
			}
			base.FinalizeMesh(MeshParts.All);
		}
	}
}
