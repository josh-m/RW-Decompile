using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public sealed class World : IExposable
	{
		public WorldInfo info = new WorldInfo();

		public WorldGrid grid;

		public WorldRenderer renderer = new WorldRenderer();

		public FactionManager factionManager = new FactionManager();

		public UniqueIDsManager uniqueIDsManager = new UniqueIDsManager();

		public WorldPawns worldPawns = new WorldPawns();

		public static readonly float MaxLatitude = 80f;

		public IntVec2 Size
		{
			get
			{
				return this.info.size;
			}
		}

		public float DegreesPerSquare
		{
			get
			{
				return World.MaxLatitude / (float)this.Size.z;
			}
		}

		public IEnumerable<IntVec2> AllSquares
		{
			get
			{
				for (int i = 0; i < this.Size.x; i++)
				{
					for (int j = 0; j < this.Size.z; j++)
					{
						yield return new IntVec2(i, j);
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<WorldInfo>(ref this.info, "info", new object[0]);
			Scribe_Deep.LookDeep<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				WorldGenerator_Grid.GenerateGridIntoWorld(this.info.seedString);
			}
			Scribe_Deep.LookDeep<FactionManager>(ref this.factionManager, "factionManager", new object[0]);
			Scribe_Deep.LookDeep<WorldPawns>(ref this.worldPawns, "worldPawns", new object[0]);
		}

		public void WorldTick()
		{
			this.worldPawns.WorldPawnsTick();
			this.factionManager.FactionManagerTick();
		}

		public bool InBounds(IntVec2 c)
		{
			return c.x >= 0 && c.z >= 0 && c.x < this.Size.x && c.z < this.Size.z;
		}

		public Vector2 LongLatOf(IntVec2 c)
		{
			float x = (float)c.x * this.DegreesPerSquare;
			float y = (float)c.z * this.DegreesPerSquare;
			return new Vector2(x, y);
		}

		public Rot4 CoastDirectionAt(IntVec2 c)
		{
			WorldSquare worldSquare = this.grid.Get(c);
			if (!worldSquare.biome.canBuildBase)
			{
				return Rot4.Invalid;
			}
			Rot4[] array = new Rot4[4];
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				IntVec2 toIntVec = GenAdj.CardinalDirections[i].ToIntVec2;
				IntVec2 intVec = c + toIntVec;
				if (this.InBounds(intVec))
				{
					WorldSquare worldSquare2 = this.grid.Get(intVec);
					if (worldSquare2.biome == BiomeDefOf.Ocean)
					{
						array[num] = Rot4.FromIntVec2(toIntVec);
						num++;
					}
				}
			}
			if (num == 0)
			{
				return Rot4.Invalid;
			}
			Rand.PushSeed();
			Rand.Seed = c.GetHashCode();
			int num2 = Rand.Range(0, num);
			Rand.PopSeed();
			return array[num2];
		}

		public IEnumerable<ThingDef> NaturalRockTypesIn(IntVec2 c)
		{
			Rand.PushSeed();
			Rand.Seed = c.GetHashCode();
			List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock
			select d).ToList<ThingDef>();
			int num = Rand.RangeInclusive(2, 3);
			if (num > list.Count)
			{
				num = list.Count;
			}
			List<ThingDef> list2 = new List<ThingDef>();
			for (int i = 0; i < num; i++)
			{
				ThingDef item = list.RandomElement<ThingDef>();
				list.Remove(item);
				list2.Add(item);
			}
			Rand.PopSeed();
			return list2;
		}

		public float DistanceFromEquatorNormalized(IntVec2 coords)
		{
			return (float)coords.z / (float)this.Size.z;
		}
	}
}
