using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class TerrainPatchMaker
	{
		private Map currentlyInitializedForMap;

		public List<TerrainThreshold> thresholds = new List<TerrainThreshold>();

		public float perlinFrequency = 0.01f;

		public float perlinLacunarity = 2f;

		public float perlinPersistence = 0.5f;

		public int perlinOctaves = 6;

		[Unsaved]
		private ModuleBase noise;

		private void Init(Map map)
		{
			this.noise = new Perlin((double)this.perlinFrequency, (double)this.perlinLacunarity, (double)this.perlinPersistence, this.perlinOctaves, Rand.Range(0, 2147483647), QualityMode.Medium);
			NoiseDebugUI.RenderSize = new IntVec2(map.Size.x, map.Size.z);
			NoiseDebugUI.StoreNoiseRender(this.noise, "TerrainPatchMaker " + this.thresholds[0].terrain.defName);
			this.currentlyInitializedForMap = map;
		}

		public void Cleanup()
		{
			this.noise = null;
			this.currentlyInitializedForMap = null;
		}

		public TerrainDef TerrainAt(IntVec3 c, Map map)
		{
			if (this.noise != null && map != this.currentlyInitializedForMap)
			{
				this.Cleanup();
			}
			if (this.noise == null)
			{
				this.Init(map);
			}
			return TerrainThreshold.TerrainAtValue(this.thresholds, this.noise.GetValue(c));
		}
	}
}
