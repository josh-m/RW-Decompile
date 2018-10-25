using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompDeepDrill : ThingComp
	{
		private CompPowerTrader powerComp;

		private float portionProgress;

		private float portionYieldPct;

		private int lastUsedTick = -99999;

		private const float WorkPerPortionBase = 12000f;

		public static float WorkPerPortionCurrentDifficulty
		{
			get
			{
				return 12000f / Find.Storyteller.difficulty.mineYieldFactor;
			}
		}

		public float ProgressToNextPortionPercent
		{
			get
			{
				return this.portionProgress / CompDeepDrill.WorkPerPortionCurrentDifficulty;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look<float>(ref this.portionProgress, "portionProgress", 0f, false);
			Scribe_Values.Look<float>(ref this.portionYieldPct, "portionYieldPct", 0f, false);
			Scribe_Values.Look<int>(ref this.lastUsedTick, "lastUsedTick", 0, false);
		}

		public void DrillWorkDone(Pawn driller)
		{
			float statValue = driller.GetStatValue(StatDefOf.MiningSpeed, true);
			this.portionProgress += statValue;
			this.portionYieldPct += statValue * driller.GetStatValue(StatDefOf.MiningYield, true) / CompDeepDrill.WorkPerPortionCurrentDifficulty;
			this.lastUsedTick = Find.TickManager.TicksGame;
			if (this.portionProgress > CompDeepDrill.WorkPerPortionCurrentDifficulty)
			{
				this.TryProducePortion(this.portionYieldPct);
				this.portionProgress = 0f;
				this.portionYieldPct = 0f;
			}
		}

		public override void PostDeSpawn(Map map)
		{
			this.portionProgress = 0f;
			this.portionYieldPct = 0f;
			this.lastUsedTick = -99999;
		}

		private void TryProducePortion(float yieldPct)
		{
			ThingDef thingDef;
			int num;
			IntVec3 intVec;
			bool nextResource = this.GetNextResource(out thingDef, out num, out intVec);
			if (thingDef == null)
			{
				return;
			}
			int num2 = Mathf.Min(num, thingDef.deepCountPerPortion);
			if (nextResource)
			{
				this.parent.Map.deepResourceGrid.SetAt(intVec, thingDef, num - num2);
			}
			int stackCount = Mathf.Max(1, GenMath.RoundRandom((float)num2 * yieldPct));
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			thing.stackCount = stackCount;
			GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null);
			if (nextResource && !this.ValuableResourcesPresent())
			{
				if (DeepDrillUtility.GetBaseResource(this.parent.Map) == null)
				{
					Messages.Message("DeepDrillExhaustedNoFallback".Translate(), this.parent, MessageTypeDefOf.TaskCompletion, true);
				}
				else
				{
					Messages.Message("DeepDrillExhausted".Translate(Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(this.parent.Map).label, -1)), this.parent, MessageTypeDefOf.TaskCompletion, true);
					for (int i = 0; i < 9; i++)
					{
						IntVec3 c = intVec + GenRadial.RadialPattern[i];
						if (c.InBounds(this.parent.Map))
						{
							ThingWithComps firstThingWithComp = c.GetFirstThingWithComp(this.parent.Map);
							if (firstThingWithComp != null && !firstThingWithComp.GetComp<CompDeepDrill>().ValuableResourcesPresent())
							{
								firstThingWithComp.SetForbidden(true, true);
							}
						}
					}
				}
			}
		}

		private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			return DeepDrillUtility.GetNextResource(this.parent.Position, this.parent.Map, out resDef, out countPresent, out cell);
		}

		public bool CanDrillNow()
		{
			return (this.powerComp == null || this.powerComp.PowerOn) && (DeepDrillUtility.GetBaseResource(this.parent.Map) != null || this.ValuableResourcesPresent());
		}

		public bool ValuableResourcesPresent()
		{
			ThingDef thingDef;
			int num;
			IntVec3 intVec;
			return this.GetNextResource(out thingDef, out num, out intVec);
		}

		public bool UsedLastTick()
		{
			return this.lastUsedTick >= Find.TickManager.TicksGame - 1;
		}

		public override string CompInspectStringExtra()
		{
			if (!this.parent.Spawned)
			{
				return null;
			}
			ThingDef thingDef;
			int num;
			IntVec3 intVec;
			this.GetNextResource(out thingDef, out num, out intVec);
			if (thingDef == null)
			{
				return "DeepDrillNoResources".Translate();
			}
			return string.Concat(new string[]
			{
				"ResourceBelow".Translate(),
				": ",
				thingDef.LabelCap,
				"\n",
				"ProgressToNextPortion".Translate(),
				": ",
				this.ProgressToNextPortionPercent.ToStringPercent("F0")
			});
		}
	}
}
