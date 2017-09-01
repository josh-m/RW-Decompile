using System;
using System.Collections.Generic;

namespace RimWorld.Planet
{
	public class Tile
	{
		public struct RoadLink
		{
			public int neighbor;

			public RoadDef road;
		}

		public struct RiverLink
		{
			public int neighbor;

			public RiverDef river;
		}

		public const int Invalid = -1;

		public BiomeDef biome;

		public float elevation = 100f;

		public Hilliness hilliness;

		public float temperature = 20f;

		public float rainfall;

		public List<Tile.RoadLink> roads;

		public List<Tile.RiverLink> rivers;

		public bool WaterCovered
		{
			get
			{
				return this.elevation <= 0f;
			}
		}

		public List<Tile.RoadLink> VisibleRoads
		{
			get
			{
				return (!this.biome.allowRoads) ? null : this.roads;
			}
		}

		public List<Tile.RiverLink> VisibleRivers
		{
			get
			{
				return (!this.biome.allowRivers) ? null : this.rivers;
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
				"mm roads=",
				(this.roads != null) ? this.roads.Count : 0,
				" rivers=",
				(this.rivers != null) ? this.rivers.Count : 0,
				")"
			});
		}
	}
}
