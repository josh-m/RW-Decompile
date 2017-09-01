using System;
using Verse;

namespace RimWorld
{
	public class CompFoodPoisoningChance : ThingComp
	{
		private CompProperties_FoodPoisoningChance Props
		{
			get
			{
				return (CompProperties_FoodPoisoningChance)this.props;
			}
		}

		public override void PostIngested(Pawn ingester)
		{
			if (this.Props.humanlikeOnly && !ingester.RaceProps.Humanlike)
			{
				return;
			}
			if (Rand.Value < this.Props.chance)
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, this.parent);
			}
		}
	}
}
