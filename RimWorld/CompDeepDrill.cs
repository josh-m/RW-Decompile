using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompDeepDrill : ThingComp
	{
		private const float ResourceLumpWork = 14000f;

		private CompPowerTrader powerComp;

		private float progressOnLump;

		public float ProgressToNextLumpPercent
		{
			get
			{
				return this.progressOnLump / 14000f;
			}
		}

		public override void PostSpawnSetup()
		{
			this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
		}

		public override void PostExposeData()
		{
			Scribe_Values.LookValue<float>(ref this.progressOnLump, "progressOnLump", 0f, false);
		}

		public void DrillWorkDone(Pawn driller)
		{
			float statValue = driller.GetStatValue(StatDefOf.MiningSpeed, true);
			this.progressOnLump += statValue;
			if (this.progressOnLump > 14000f)
			{
				this.ProduceLump();
				this.progressOnLump = 0f;
			}
		}

		private void ProduceLump()
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
				Thing thing = ThingMaker.MakeThing(thingDef, null);
				thing.stackCount = num2;
				GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null);
				return;
			}
			Log.Error("Drill tried to ProduceLump but couldn't.");
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
					this.ProgressToNextLumpPercent.ToStringPercent()
				});
			}
			return "ResourceBelow".Translate() + ": " + "NothingLower".Translate();
		}
	}
}
