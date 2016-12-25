using System;
using Verse;

namespace RimWorld
{
	public class CompMilkable : CompHasGatherableBodyResource
	{
		protected override int GatherResourcesIntervalDays
		{
			get
			{
				return this.Props.milkIntervalDays;
			}
		}

		protected override int ResourceAmount
		{
			get
			{
				return this.Props.milkAmount;
			}
		}

		protected override ThingDef ResourceDef
		{
			get
			{
				return this.Props.milkDef;
			}
		}

		protected override string SaveKey
		{
			get
			{
				return "milkFullness";
			}
		}

		public CompProperties_Milkable Props
		{
			get
			{
				return (CompProperties_Milkable)this.props;
			}
		}

		protected override bool Active
		{
			get
			{
				if (!base.Active)
				{
					return false;
				}
				Pawn pawn = this.parent as Pawn;
				return (!this.Props.milkFemaleOnly || pawn == null || pawn.gender == Gender.Female) && (pawn == null || pawn.ageTracker.CurLifeStage.milkable);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!this.Active)
			{
				return null;
			}
			return "MilkFullness".Translate() + ": " + base.Fullness.ToStringPercent();
		}
	}
}
