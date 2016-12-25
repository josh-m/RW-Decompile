using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MapCondition_ToxicFallout : MapCondition
	{
		private const int LerpTicks = 5000;

		private const float MaxSkyLerpFactor = 0.5f;

		private const float SkyGlow = 0.85f;

		private const int CheckInterval = 3451;

		private const float ToxicPerDay = 0.5f;

		private const float PlantKillChance = 0.0065f;

		private const float CorpseRotProgressAdd = 3000f;

		private SkyColorSet ToxicFalloutColors;

		private List<SkyOverlay> overlays;

		public MapCondition_ToxicFallout()
		{
			ColorInt colorInt = new ColorInt(216, 255, 0);
			Color arg_50_0 = colorInt.ToColor;
			ColorInt colorInt2 = new ColorInt(234, 200, 255);
			this.ToxicFalloutColors = new SkyColorSet(arg_50_0, colorInt2.ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);
			this.overlays = new List<SkyOverlay>
			{
				new WeatherOverlay_Fallout()
			};
			base..ctor();
		}

		public override void Init()
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
		}

		public override void MapConditionTick()
		{
			if (Find.TickManager.TicksGame % 3451 == 0)
			{
				List<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					Pawn pawn = allPawnsSpawned[i];
					if (!pawn.Position.Roofed(base.Map) && pawn.def.race.IsFlesh)
					{
						float num = 0.028758334f;
						num *= pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
						if (num != 0f)
						{
							Rand.PushSeed();
							Rand.Seed = pawn.thingIDNumber * 74374237;
							float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.Value);
							Rand.PopSeed();
							num *= num2;
							HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
						}
					}
				}
			}
			for (int j = 0; j < this.overlays.Count; j++)
			{
				this.overlays[j].TickOverlay(base.Map);
			}
		}

		public override void DoCellSteadyEffects(IntVec3 c)
		{
			if (!c.Roofed(base.Map))
			{
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is Plant)
					{
						if (Rand.Value < 0.0065f)
						{
							thing.Destroy(DestroyMode.Kill);
						}
					}
					else if (thing.def.category == ThingCategory.Item)
					{
						CompRottable compRottable = thing.TryGetComp<CompRottable>();
						if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
						{
							compRottable.RotProgress += 3000f;
						}
					}
				}
			}
		}

		public override void MapConditionDraw()
		{
			Map map = base.Map;
			for (int i = 0; i < this.overlays.Count; i++)
			{
				this.overlays[i].DrawOverlay(map);
			}
		}

		public override float SkyTargetLerpFactor()
		{
			return MapConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, 5000f, 0.5f);
		}

		public override SkyTarget? SkyTarget()
		{
			return new SkyTarget?(new SkyTarget(this.ToxicFalloutColors)
			{
				glow = 0.85f
			});
		}

		public override float AnimalDensityFactor()
		{
			return 0f;
		}

		public override float PlantDensityFactor()
		{
			return 0f;
		}

		public override bool AllowEnjoyableOutsideNow()
		{
			return false;
		}

		public override List<SkyOverlay> SkyOverlays()
		{
			return this.overlays;
		}
	}
}
