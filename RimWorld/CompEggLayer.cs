using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompEggLayer : ThingComp
	{
		private float eggProgress;

		private int fertilizationCount;

		private Pawn fertilizedBy;

		private bool Active
		{
			get
			{
				Pawn pawn = this.parent as Pawn;
				return (!this.Props.eggLayFemaleOnly || pawn == null || pawn.gender == Gender.Female) && (pawn == null || pawn.ageTracker.CurLifeStage.milkable);
			}
		}

		public bool CanLayNow
		{
			get
			{
				return this.Active && this.eggProgress >= 1f;
			}
		}

		public bool FullyFertilized
		{
			get
			{
				return this.fertilizationCount >= this.Props.eggFertilizationCountMax;
			}
		}

		private bool ProgressStoppedBecauseUnfertilized
		{
			get
			{
				return this.fertilizationCount == 0 && this.eggProgress >= this.Props.eggProgressUnfertilizedMax;
			}
		}

		public CompProperties_EggLayer Props
		{
			get
			{
				return (CompProperties_EggLayer)this.props;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.eggProgress, "eggProgress", 0f, false);
			Scribe_Values.LookValue<int>(ref this.fertilizationCount, "fertilizationCount", 0, false);
			Scribe_References.LookReference<Pawn>(ref this.fertilizedBy, "fertilizedBy", false);
		}

		public override void CompTick()
		{
			if (this.Active)
			{
				float num = 1f / (this.Props.eggLayIntervalDays * 60000f);
				Pawn pawn = this.parent as Pawn;
				if (pawn != null)
				{
					num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
				}
				this.eggProgress += num;
				if (this.eggProgress > 1f)
				{
					this.eggProgress = 1f;
				}
				if (this.ProgressStoppedBecauseUnfertilized)
				{
					this.eggProgress = this.Props.eggProgressUnfertilizedMax;
				}
			}
		}

		public void Fertilize(Pawn male)
		{
			this.fertilizationCount = this.Props.eggFertilizationCountMax;
			this.fertilizedBy = male;
		}

		public virtual Thing ProduceEgg()
		{
			if (!this.Active)
			{
				Log.Error("LayEgg while not Active: " + this.parent);
			}
			this.eggProgress = 0f;
			int randomInRange = this.Props.eggCountRange.RandomInRange;
			if (randomInRange == 0)
			{
				return null;
			}
			Thing thing;
			if (this.fertilizationCount > 0)
			{
				thing = ThingMaker.MakeThing(this.Props.eggFertilizedDef, null);
				this.fertilizationCount = Mathf.Max(0, this.fertilizationCount - randomInRange);
			}
			else
			{
				thing = ThingMaker.MakeThing(this.Props.eggUnfertilizedDef, null);
			}
			thing.stackCount = randomInRange;
			CompHatcher compHatcher = thing.TryGetComp<CompHatcher>();
			if (compHatcher != null)
			{
				compHatcher.hatcheeFaction = this.parent.Faction;
				Pawn pawn = this.parent as Pawn;
				if (pawn != null)
				{
					compHatcher.hatcheeParent = pawn;
				}
				if (this.fertilizedBy != null)
				{
					compHatcher.otherParent = this.fertilizedBy;
				}
			}
			return thing;
		}

		public override string CompInspectStringExtra()
		{
			if (!this.Active)
			{
				return null;
			}
			string text = "EggProgress".Translate() + ": " + this.eggProgress.ToStringPercent();
			if (this.fertilizationCount > 0)
			{
				text = text + "\n" + "Fertilized".Translate();
			}
			else if (this.ProgressStoppedBecauseUnfertilized)
			{
				text = text + "\n" + "ProgressStoppedUntilFertilized".Translate();
			}
			return text;
		}
	}
}
