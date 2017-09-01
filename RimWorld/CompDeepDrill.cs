using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompDeepDrill : ThingComp
	{
		private const float ResourceLumpWork = 14000f;

		private CompPowerTrader powerComp;

		private float lumpProgress;

		private float lumpYieldPct;

		public float ProgressToNextLumpPercent
		{
			get
			{
				return this.lumpProgress / 14000f;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look<float>(ref this.lumpProgress, "lumpProgress", 0f, false);
			Scribe_Values.Look<float>(ref this.lumpYieldPct, "lumpYieldPct", 0f, false);
		}

		public void DrillWorkDone(Pawn driller)
		{
			float statValue = driller.GetStatValue(StatDefOf.MiningSpeed, true);
			this.lumpProgress += statValue;
			this.lumpYieldPct += statValue * driller.GetStatValue(StatDefOf.MiningYield, true) / 14000f;
			if (this.lumpProgress > 14000f)
			{
				this.TryProduceLump(this.lumpYieldPct);
				this.lumpProgress = 0f;
				this.lumpYieldPct = 0f;
			}
		}

		private void TryProduceLump(float yieldPct)
		{
			ThingDef thingDef;
			int num;
			IntVec3 c;
			if (this.TryGetNextResource(out thingDef, out num, out c))
			{
				int num2 = Mathf.Min(new int[]
				{
					num,
					thingDef.deepCountPerCell / 2,
					thingDef.stackLimit
				});
				this.parent.Map.deepResourceGrid.SetAt(c, thingDef, num - num2);
				int stackCount = Mathf.Max(1, GenMath.RoundRandom((float)num2 * yieldPct));
				Thing thing = ThingMaker.MakeThing(thingDef, null);
				thing.stackCount = stackCount;
				GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null);
			}
			else
			{
				Log.Error("Drill tried to ProduceLump but couldn't.");
			}
		}

		public bool TryGetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			for (int i = 0; i < 9; i++)
			{
				IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(this.parent.Map))
				{
					ThingDef thingDef = this.parent.Map.deepResourceGrid.ThingDefAt(intVec);
					if (thingDef != null)
					{
						resDef = thingDef;
						countPresent = this.parent.Map.deepResourceGrid.CountAt(intVec);
						cell = intVec;
						return true;
					}
				}
			}
			resDef = null;
			countPresent = 0;
			cell = IntVec3.Invalid;
			return false;
		}

		public bool CanDrillNow()
		{
			return (this.powerComp == null || this.powerComp.PowerOn) && this.ResourcesPresent();
		}

		public bool ResourcesPresent()
		{
			ThingDef thingDef;
			int num;
			IntVec3 intVec;
			return this.TryGetNextResource(out thingDef, out num, out intVec);
		}

		public override string CompInspectStringExtra()
		{
			ThingDef thingDef;
			int num;
			IntVec3 intVec;
			if (this.TryGetNextResource(out thingDef, out num, out intVec))
			{
				return string.Concat(new string[]
				{
					"ResourceBelow".Translate(),
					": ",
					thingDef.label,
					"\n",
					"ProgressToNextLump".Translate(),
					": ",
					this.ProgressToNextLumpPercent.ToStringPercent("F0")
				});
			}
			return "ResourceBelow".Translate() + ": " + "NothingLower".Translate();
		}
	}
}
