using System;

namespace RimWorld.Planet
{
	public class Tile
	{
		public BiomeDef biome;

		public float elevation = 100f;

		public Hilliness hilliness;

		public float temperature = 20f;

		public float rainfall;

		public bool WaterCovered
		{
			get
			{
				return this.elevation <= 0f;
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.biome,
				" elev=",
				this.elevation,
				"m hill=",
				this.hilliness,
				" temp=",
				this.temperature,
				"°C rain=",
				this.rainfall,
				"mm)"
			});
		}
	}
}
