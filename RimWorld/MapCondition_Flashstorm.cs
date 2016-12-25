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

		public MapCondition_Flashstorm()
		{
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
					Find.WeatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(intVec));
					this.nextLightningTicks = Find.TickManager.TicksGame + MapCondition_Flashstorm.TicksBetweenStrikes.RandomInRange;
				}
			}
		}

		public override void End()
		{
			Find.Storyteller.weatherDecider.DisableRainFor(30000);
			base.End();
		}

		private void FindGoodCenterLocation()
		{
			if (Find.Map.Size.x <= 16 || Find.Map.Size.z <= 16)
			{
				throw new Exception("Map too small for flashstorm.");
			}
			for (int i = 0; i < 10; i++)
			{
				this.centerLocation = new IntVec2(Rand.Range(8, Find.Map.Size.x - 8), Rand.Range(8, Find.Map.Size.z - 8));
				if (this.IsGoodCenterLocation(this.centerLocation))
				{
					break;
				}
			}
		}

		private bool IsGoodLocationForStrike(IntVec3 loc)
		{
			return loc.InBounds() && !loc.Roofed() && loc.Standable();
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
