using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_ConfigPage_ConfigureStartingPawns : ScenPart_ConfigPage
	{
		private const int MaxPawnCount = 10;

		public int pawnCount = 3;

		private string pawnCountBuffer;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			base.DoEditInterface(listing);
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			Widgets.TextFieldNumeric<int>(scenPartRect, ref this.pawnCount, ref this.pawnCountBuffer, 1f, 10f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.pawnCount, "pawnCount", 0, false);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_StartWithPawns".Translate(new object[]
			{
				this.pawnCount
			});
		}

		public override void Randomize()
		{
			this.pawnCount = Rand.RangeInclusive(1, 6);
		}

		public override void PostWorldLoad()
		{
			int num = 0;
			while (true)
			{
				StartingPawnUtility.ClearAllStartingPawns();
				for (int i = 0; i < this.pawnCount; i++)
				{
					Find.GameInitData.startingPawns.Add(StartingPawnUtility.NewGeneratedStartingPawn());
				}
				num++;
				if (num > 20)
				{
					break;
				}
				if (StartingPawnUtility.WorkTypeRequirementsSatisfied())
				{
					return;
				}
			}
		}
	}
}
