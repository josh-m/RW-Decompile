using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_ConfigPage_ConfigureStartingPawns : ScenPart_ConfigPage
	{
		public int pawnCount = 3;

		public int pawnChoiceCount = 10;

		private string pawnCountBuffer;

		private string pawnCountChoiceBuffer;

		private const int MaxPawnCount = 10;

		private const int MaxPawnChoiceCount = 10;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			base.DoEditInterface(listing);
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			scenPartRect.height = ScenPart.RowHeight;
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect = new Rect(scenPartRect.x - 200f, scenPartRect.y + ScenPart.RowHeight, 200f, ScenPart.RowHeight);
			rect.xMax -= 4f;
			Widgets.Label(rect, "ScenPart_StartWithPawns_OutOf".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldNumeric<int>(scenPartRect, ref this.pawnCount, ref this.pawnCountBuffer, 1f, 10f);
			scenPartRect.y += ScenPart.RowHeight;
			Widgets.TextFieldNumeric<int>(scenPartRect, ref this.pawnChoiceCount, ref this.pawnCountChoiceBuffer, (float)this.pawnCount, 10f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.pawnCount, "pawnCount", 0, false);
			Scribe_Values.Look<int>(ref this.pawnChoiceCount, "pawnChoiceCount", 0, false);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_StartWithPawns".Translate(new object[]
			{
				this.pawnCount,
				this.pawnChoiceCount
			});
		}

		public override void Randomize()
		{
			this.pawnCount = Rand.RangeInclusive(1, 6);
			this.pawnChoiceCount = 10;
		}

		public override void PostWorldGenerate()
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
					goto Block_3;
				}
			}
			return;
			Block_3:
			while (Find.GameInitData.startingPawns.Count < this.pawnChoiceCount)
			{
				Find.GameInitData.startingPawns.Add(StartingPawnUtility.NewGeneratedStartingPawn());
			}
			Find.GameInitData.startingPawnCount = this.pawnCount;
		}
	}
}
