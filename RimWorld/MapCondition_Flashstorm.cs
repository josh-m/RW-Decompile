using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MapCondition_Flashstorm : MapCondition
	{
		private const int RainDisableTicksAfterConditionEnds = 30000;

		private static readonly IntRange AreaRadiusRange = new IntRange(45, 60);

		private static readonly IntRange TicksBetweenStrikes = new IntRange(320, 800);

		public IntVec2 centerLocation;

		private int areaRadius;

		private int nextLightningTicks;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<IntVec2>(ref this.centerLocation, "centerLocation", default(IntVec2), false);
			Scribe_Values.LookValue<int>(ref this.areaRadius, "areaRadius", 0, false);
			Scribe_Values.LookValue<int>(ref this.nextLightningTicks, "nextLightningTicks", 0, false);
		}

		public override void Init()
		{
			base.Init();
			this.areaRadius = MapCondition_Flashstorm.AreaRadiusRange.RandomInRange;
			this.FindGoodCenterLocation();
		}

		public override void MapConditionTick()
		{
			if (Find.TickManager.TicksGame > this.nextLightningTicks)
			{
				Vector2 a = new Vector2(Rand.Gaussian(0f, 1f), Rand.Gaussian(0f, 1f));
				a.Normalize();
				a *= Rand.Range(0f, (float)this.areaRadius);
				IntVec3 intVec = new IntVec3((int)Math.Round((double)a.x) + this.centerLocation.x, 0, (int)Math.Round((double)a.y) + this.centerLocation.z);
				if (this.IsGoodLocationForStrike(intVec))
				{
					base.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(base.Map, intVec));
					this.nextLightningTicks = Find.TickManager.TicksGame + MapCondition_Flashstorm.TicksBetweenStrikes.RandomInRange;
				}
			}
		}

		public override void End()
		{
			base.Map.weatherDecider.DisableRainFor(30000);
			base.End();
		}

		private void FindGoodCenterLocation()
		{
			if (base.Map.Size.x <= 16 || base.Map.Size.z <= 16)
			{
				throw new Exception("Map too small for flashstorm.");
			}
			for (int i = 0; i < 10; i++)
			{
				this.centerLocation = new IntVec2(Rand.Range(8, base.Map.Size.x - 8), Rand.Range(8, base.Map.Size.z - 8));
				if (this.IsGoodCenterLocation(this.centerLocation))
				{
					break;
				}
			}
		}

		private bool IsGoodLocationForStrike(IntVec3 loc)
		{
			return loc.InBounds(base.Map) && !loc.Roofed(base.Map) && loc.Standable(base.Map);
		}

		private bool IsGoodCenterLocation(IntVec2 loc)
		{
			int num = 0;
			int num2 = (int)(3.14159274f * (float)this.areaRadius * (float)this.areaRadius / 2f);
			foreach (IntVec3 current in this.GetPotentiallyAffectedCells(loc))
			{
				if (this.IsGoodLocationForStrike(current))
				{
					num++;
				}
				if (num >= num2)
				{
					break;
				}
			}
			return num >= num2;
		}

		[DebuggerHidden]
		private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
		{
			for (int x = center.x - this.areaRadius; x <= center.x + this.areaRadius; x++)
			{
				for (int z = center.z - this.areaRadius; z <= center.z + this.areaRadius; z++)
				{
					if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= this.areaRadius * this.areaRadius)
					{
						yield return new IntVec3(x, 0, z);
					}
				}
			}
		}
	}
}
