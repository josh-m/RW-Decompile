using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompFoodPoisonable : ThingComp
	{
		private float poisonPct;

		public float PoisonPercent
		{
			get
			{
				return this.poisonPct;
			}
			set
			{
				this.poisonPct = Mathf.Clamp01(value);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.poisonPct, "poisonPct", 0f, false);
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			CompFoodPoisonable compFoodPoisonable = piece.TryGetComp<CompFoodPoisonable>();
			compFoodPoisonable.poisonPct = this.poisonPct;
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			base.PreAbsorbStack(otherStack, count);
			CompFoodPoisonable compFoodPoisonable = otherStack.TryGetComp<CompFoodPoisonable>();
			this.poisonPct = GenMath.WeightedAverage(this.poisonPct, (float)this.parent.stackCount, compFoodPoisonable.poisonPct, (float)count);
		}

		public override void PostIngested(Pawn ingester)
		{
			if (Rand.Value < this.poisonPct)
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, this.parent);
			}
		}
	}
}
