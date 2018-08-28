using System;
using Verse;

namespace RimWorld
{
	public class CompPlantHarmRadius : ThingComp
	{
		private int plantHarmAge;

		private int ticksToPlantHarm;

		private float LeaflessPlantKillChance = 0.09f;

		protected CompProperties_PlantHarmRadius PropsPlantHarmRadius
		{
			get
			{
				return (CompProperties_PlantHarmRadius)this.props;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look<int>(ref this.plantHarmAge, "plantHarmAge", 0, false);
			Scribe_Values.Look<int>(ref this.ticksToPlantHarm, "ticksToPlantHarm", 0, false);
		}

		public override void CompTick()
		{
			if (!this.parent.Spawned)
			{
				return;
			}
			this.plantHarmAge++;
			this.ticksToPlantHarm--;
			if (this.ticksToPlantHarm <= 0)
			{
				float x = (float)this.plantHarmAge / 60000f;
				float num = this.PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(x);
				float num2 = 3.14159274f * num * num;
				float num3 = num2 * this.PropsPlantHarmRadius.harmFrequencyPerArea;
				float num4 = 60f / num3;
				int num5;
				if (num4 >= 1f)
				{
					this.ticksToPlantHarm = GenMath.RoundRandom(num4);
					num5 = 1;
				}
				else
				{
					this.ticksToPlantHarm = 1;
					num5 = GenMath.RoundRandom(1f / num4);
				}
				for (int i = 0; i < num5; i++)
				{
					this.HarmRandomPlantInRadius(num);
				}
			}
		}

		private void HarmRandomPlantInRadius(float radius)
		{
			IntVec3 c = this.parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
			if (!c.InBounds(this.parent.Map))
			{
				return;
			}
			Plant plant = c.GetPlant(this.parent.Map);
			if (plant != null)
			{
				if (plant.LeaflessNow)
				{
					if (Rand.Value < this.LeaflessPlantKillChance)
					{
						plant.Kill(null, null);
					}
				}
				else
				{
					plant.MakeLeafless(Plant.LeaflessCause.Poison);
				}
			}
		}
	}
}
