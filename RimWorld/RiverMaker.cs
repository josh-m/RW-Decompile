using System;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	internal class RiverMaker
	{
		private ModuleBase generator;

		private ModuleBase shallowizer;

		private float surfaceLevel;

		public RiverMaker(Vector3 center, float angleA, float angleB, RiverDef riverDef)
		{
			this.surfaceLevel = riverDef.widthOnMap / 2f;
			this.generator = new DistFromAxis(1f);
			this.generator = new Bend((angleB - (angleA + 180f) + 360f) % 360f, 50f, this.generator);
			this.generator = new Rotate(0.0, (double)(-(double)angleA), 0.0, this.generator);
			this.generator = new Translate((double)(-(double)center.x), 0.0, (double)(-(double)center.z), this.generator);
			ModuleBase moduleBase = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, 2147483647), QualityMode.Medium);
			ModuleBase moduleBase2 = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, 2147483647), QualityMode.Medium);
			ModuleBase rhs = new Const(8.0);
			moduleBase = new Multiply(moduleBase, rhs);
			moduleBase2 = new Multiply(moduleBase2, rhs);
			this.generator = new Displace(this.generator, moduleBase, new Const(0.0), moduleBase2);
			this.shallowizer = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Range(0, 2147483647), QualityMode.Medium);
			this.shallowizer = new Abs(this.shallowizer);
		}

		public TerrainDef TerrainAt(IntVec3 loc)
		{
			float value = this.generator.GetValue(loc);
			if (value < this.surfaceLevel - 2f && this.shallowizer.GetValue(loc) > 0.2f)
			{
				return TerrainDefOf.WaterMovingDeep;
			}
			if (value < this.surfaceLevel)
			{
				return TerrainDefOf.WaterMovingShallow;
			}
			return null;
		}
	}
}
