using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class GenStep_RockChunks : GenStep
	{
		private const float ThreshLooseRock = 0.55f;

		private const float PlaceProbabilityPerCell = 0.006f;

		private const float RubbleProbability = 0.5f;

		private ModuleBase freqFactorNoise;

		public override void Generate()
		{
			this.freqFactorNoise = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
			this.freqFactorNoise = new ScaleBias(1.0, 1.0, this.freqFactorNoise);
			NoiseDebugUI.StoreNoiseRender(this.freqFactorNoise, "rock_chunks_freq_factor");
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				float num = 0.006f * this.freqFactorNoise.GetValue(current);
				if (MapGenerator.Elevation[current] < 0.55f && Rand.Value < num)
				{
					this.GrowLowRockFormationFrom(current);
				}
			}
		}

		private void GrowLowRockFormationFrom(IntVec3 root)
		{
			ThingDef rockRubble = ThingDefOf.RockRubble;
			ThingDef mineableThing = Find.World.NaturalRockTypesIn(Find.Map.WorldCoords).RandomElement<ThingDef>().building.mineableThing;
			Rot4 random = Rot4.Random;
			IntVec3 intVec = root;
			while (true)
			{
				Rot4 random2 = Rot4.Random;
				if (!(random2 == random))
				{
					intVec += random2.FacingCell;
					if (!intVec.InBounds() || intVec.GetEdifice() != null || intVec.GetFirstItem() != null)
					{
						break;
					}
					if (MapGenerator.Elevation[intVec] > 0.55f)
					{
						return;
					}
					if (!Find.TerrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordance.Heavy))
					{
						return;
					}
					GenSpawn.Spawn(mineableThing, intVec);
					IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
					for (int i = 0; i < adjacentCellsAndInside.Length; i++)
					{
						IntVec3 b = adjacentCellsAndInside[i];
						if (Rand.Value < 0.5f)
						{
							IntVec3 c = intVec + b;
							if (c.InBounds())
							{
								bool flag = false;
								List<Thing> thingList = c.GetThingList();
								for (int j = 0; j < thingList.Count; j++)
								{
									Thing thing = thingList[j];
									if (thing.def.category != ThingCategory.Plant && thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Pawn)
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									FilthMaker.MakeFilth(c, rockRubble, 1);
								}
							}
						}
					}
				}
			}
		}
	}
}
