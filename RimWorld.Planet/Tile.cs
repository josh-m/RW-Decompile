using System;
using System.Collections.Generic;
using Verse;

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

		public float swampiness;

		public WorldFeature feature;

		public List<Tile.RoadLink> potentialRoads;

		public List<Tile.RiverLink> potentialRivers;

		public bool WaterCovered
		{
			get
			{
				return this.elevation <= 0f;
			}
		}

		public List<Tile.RoadLink> Roads
		{
			get
			{
				return (!this.biome.allowRoads) ? null : this.potentialRoads;
			}
		}

		public List<Tile.RiverLink> Rivers
		{
			get
			{
				return (!this.biome.allowRivers) ? null : this.potentialRivers;
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
				"mm swampiness=",
				this.swampiness.ToStringPercent(),
				" potentialRoads=",
				(this.potentialRoads != null) ? this.potentialRoads.Count : 0,
				" (allowed=",
				this.biome.allowRoads,
				") potentialRivers=",
				(this.potentialRivers != null) ? this.potentialRivers.Count : 0,
				" (allowed=",
				this.biome.allowRivers,
				"))"
			});
		}
	}
}
