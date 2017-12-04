using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldFeature : IExposable, ILoadReferenceable
	{
		public int uniqueID;

		public FeatureDef def;

		public string name;

		public Vector3 drawCenter;

		public float drawAngle;

		public Vector2 maxDrawSizeInTiles;

		public float alpha;

		public IEnumerable<int> Tiles
		{
			get
			{
				WorldGrid worldGrid = Find.WorldGrid;
				int tilesCount = worldGrid.TilesCount;
				for (int i = 0; i < tilesCount; i++)
				{
					Tile t = worldGrid[i];
					if (t.feature == this)
					{
						yield return i;
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", 0, false);
			Scribe_Defs.Look<FeatureDef>(ref this.def, "def");
			Scribe_Values.Look<string>(ref this.name, "name", null, false);
			Scribe_Values.Look<Vector3>(ref this.drawCenter, "drawCenter", default(Vector3), false);
			Scribe_Values.Look<float>(ref this.drawAngle, "drawAngle", 0f, false);
			Scribe_Values.Look<Vector2>(ref this.maxDrawSizeInTiles, "maxDrawSizeInTiles", default(Vector2), false);
		}

		public string GetUniqueLoadID()
		{
			return "WorldFeature_" + this.uniqueID;
		}
	}
}
