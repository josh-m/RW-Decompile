using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_PawnFilter_Age : ScenPart
	{
		public IntRange allowedAgeRange = new IntRange(0, 999999);

		private const int RangeMin = 15;

		private const int RangeMax = 120;

		private const int RangeMinMax = 19;

		private const int RangeMinWidth = 4;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, 31f);
			Widgets.IntRange(scenPartRect, (int)listing.CurHeight, ref this.allowedAgeRange, 15, 120, null, 4);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<IntRange>(ref this.allowedAgeRange, "allowedAgeRange", default(IntRange), false);
		}

		public override string Summary(Scenario scen)
		{
			if (this.allowedAgeRange.min > 15)
			{
				if (this.allowedAgeRange.max < 10000)
				{
					return "ScenPart_StartingPawnAgeRange".Translate(new object[]
					{
						this.allowedAgeRange.min,
						this.allowedAgeRange.max
					});
				}
				return "ScenPart_StartingPawnAgeMin".Translate(new object[]
				{
					this.allowedAgeRange.min
				});
			}
			else
			{
				if (this.allowedAgeRange.max < 10000)
				{
					return "ScenPart_StartingPawnAgeMax".Translate(new object[]
					{
						this.allowedAgeRange.max
					});
				}
				throw new Exception();
			}
		}

		public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
		{
			return this.allowedAgeRange.Includes(pawn.ageTracker.AgeBiologicalYears);
		}

		public override void Randomize()
		{
			this.allowedAgeRange = new IntRange(15, 120);
			int num = Rand.RangeInclusive(0, 2);
			if (num != 0)
			{
				if (num != 1)
				{
					if (num == 2)
					{
						this.allowedAgeRange.min = Rand.Range(20, 60);
						this.allowedAgeRange.max = Rand.Range(20, 60);
					}
				}
				else
				{
					this.allowedAgeRange.max = Rand.Range(20, 60);
				}
			}
			else
			{
				this.allowedAgeRange.min = Rand.Range(20, 60);
			}
			this.MakeAllowedAgeRangeValid();
		}

		private void MakeAllowedAgeRangeValid()
		{
			if (this.allowedAgeRange.max < 19)
			{
				this.allowedAgeRange.max = 19;
			}
			if (this.allowedAgeRange.max - this.allowedAgeRange.min < 4)
			{
				this.allowedAgeRange.min = this.allowedAgeRange.max - 4;
			}
		}
	}
}
