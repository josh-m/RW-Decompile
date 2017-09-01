using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_BillConfig : Window
	{
		private IntVec3 billGiverPos;

		private Bill_Production bill;

		private Vector2 scrollPosition;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(760f, 600f);
			}
		}

		public Dialog_BillConfig(Bill_Production bill, IntVec3 billGiverPos)
		{
			this.billGiverPos = billGiverPos;
			this.bill = bill;
			this.forcePause = true;
			this.doCloseX = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.absorbInputAroundWindow = true;
			this.closeOnClickedOutside = true;
		}

		private void AdjustCount(int offset)
		{
			if (offset > 0)
			{
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera(null);
			}
			else
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera(null);
			}
			this.bill.repeatCount += offset;
			if (this.bill.repeatCount < 1)
			{
				this.bill.repeatCount = 1;
			}
		}

		public override void WindowUpdate()
		{
			this.bill.TryDrawIngredientSearchRadiusOnMap(this.billGiverPos);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(0f, 0f, 400f, 50f);
			Widgets.Label(rect, this.bill.LabelCap);
			Rect rect2 = new Rect(0f, 80f, 200f, inRect.height - 80f);
			Rect rect3 = new Rect(rect2.xMax + 17f, 50f, 180f, inRect.height - 50f - this.CloseButSize.y);
			Rect rect4 = new Rect(rect3.xMax + 17f, 50f, inRect.width - (rect3.xMax + 17f), inRect.height - 50f - this.CloseButSize.y);
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect3);
			if (this.bill.suspended)
			{
				if (listing_Standard.ButtonText("Suspended".Translate(), null))
				{
					this.bill.suspended = false;
				}
			}
			else if (listing_Standard.ButtonText("NotSuspended".Translate(), null))
			{
				this.bill.suspended = true;
			}
			if (listing_Standard.ButtonText(this.bill.repeatMode.GetLabel(), null))
			{
				BillRepeatModeUtility.MakeConfigFloatMenu(this.bill);
			}
			string label = ("BillStoreMode_" + this.bill.storeMode).Translate();
			if (listing_Standard.ButtonText(label, null))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (BillStoreModeDef current in from bsm in DefDatabase<BillStoreModeDef>.AllDefs
				orderby bsm.listOrder
				select bsm)
				{
					BillStoreModeDef smLocal = current;
					list.Add(new FloatMenuOption(("BillStoreMode_" + current).Translate(), delegate
					{
						this.bill.storeMode = smLocal;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			listing_Standard.Gap(12f);
			if (this.bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				listing_Standard.Label("RepeatCount".Translate(new object[]
				{
					this.bill.RepeatInfoText
				}), -1f);
				listing_Standard.IntSetter(ref this.bill.repeatCount, 1, "1", 42f);
				listing_Standard.IntAdjuster(ref this.bill.repeatCount, 1, 0);
				listing_Standard.IntAdjuster(ref this.bill.repeatCount, 25, 0);
			}
			else if (this.bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				string text = "CurrentlyHave".Translate() + ": ";
				text += this.bill.recipe.WorkerCounter.CountProducts(this.bill);
				text += " / ";
				text += ((this.bill.targetCount >= 999999) ? "Infinite".Translate().ToLower() : this.bill.targetCount.ToString());
				string text2 = this.bill.recipe.WorkerCounter.ProductsDescription(this.bill);
				if (!text2.NullOrEmpty())
				{
					string text3 = text;
					text = string.Concat(new string[]
					{
						text3,
						"\n",
						"CountingProducts".Translate(),
						": ",
						text2
					});
				}
				listing_Standard.Label(text, -1f);
				int targetCount = this.bill.targetCount;
				listing_Standard.IntSetter(ref this.bill.targetCount, 1, "1", 42f);
				listing_Standard.IntAdjuster(ref this.bill.targetCount, 1, 1);
				listing_Standard.IntAdjuster(ref this.bill.targetCount, 25, 1);
				listing_Standard.IntAdjuster(ref this.bill.targetCount, 250, 1);
				this.bill.unpauseWhenYouHave = Mathf.Max(0, this.bill.unpauseWhenYouHave + (this.bill.targetCount - targetCount));
			}
			listing_Standard.Gap(12f);
			listing_Standard.Label("IngredientSearchRadius".Translate() + ": " + this.bill.ingredientSearchRadius.ToString("F0"), -1f);
			this.bill.ingredientSearchRadius = listing_Standard.Slider(this.bill.ingredientSearchRadius, 3f, 100f);
			if (this.bill.ingredientSearchRadius >= 100f)
			{
				this.bill.ingredientSearchRadius = 999f;
			}
			if (this.bill.recipe.workSkill != null)
			{
				listing_Standard.Label("AllowedSkillRange".Translate(new object[]
				{
					this.bill.recipe.workSkill.label.ToLower()
				}), -1f);
				listing_Standard.IntRange(ref this.bill.allowedSkillRange, 0, 20);
			}
			if (this.bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				listing_Standard.Gap(12f);
				listing_Standard.CheckboxLabeled("PauseWhenSatisfied".Translate(), ref this.bill.pauseWhenSatisfied, null);
				if (this.bill.pauseWhenSatisfied)
				{
					listing_Standard.Label("UnpauseWhenYouHave".Translate() + ": " + this.bill.unpauseWhenYouHave.ToString("F0"), -1f);
					this.bill.unpauseWhenYouHave = Mathf.RoundToInt(listing_Standard.Slider((float)this.bill.unpauseWhenYouHave, 0f, (float)(this.bill.targetCount - 1)));
				}
			}
			listing_Standard.End();
			ThingFilterUI.DoThingFilterConfigWindow(rect4, ref this.scrollPosition, this.bill.ingredientFilter, this.bill.recipe.fixedIngredientFilter, 4, null, this.bill.recipe.forceHiddenSpecialFilters, this.bill.recipe.GetPremultipliedSmallIngredients());
			StringBuilder stringBuilder = new StringBuilder();
			if (this.bill.recipe.description != null)
			{
				stringBuilder.AppendLine(this.bill.recipe.description);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("WorkAmount".Translate() + ": " + this.bill.recipe.WorkAmountTotal(null).ToStringWorkAmount());
			stringBuilder.AppendLine();
			for (int i = 0; i < this.bill.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = this.bill.recipe.ingredients[i];
				if (!ingredientCount.filter.Summary.NullOrEmpty())
				{
					stringBuilder.AppendLine(this.bill.recipe.IngredientValueGetter.BillRequirementsDescription(this.bill.recipe, ingredientCount));
				}
			}
			stringBuilder.AppendLine();
			string text4 = this.bill.recipe.IngredientValueGetter.ExtraDescriptionLine(this.bill.recipe);
			if (text4 != null)
			{
				stringBuilder.AppendLine(text4);
				stringBuilder.AppendLine();
			}
			if (!this.bill.recipe.skillRequirements.NullOrEmpty<SkillRequirement>())
			{
				stringBuilder.AppendLine("MinimumSkills".Translate());
				stringBuilder.AppendLine(this.bill.recipe.MinSkillString);
			}
			Text.Font = GameFont.Small;
			string text5 = stringBuilder.ToString();
			if (Text.CalcHeight(text5, rect2.width) > rect2.height)
			{
				Text.Font = GameFont.Tiny;
			}
			Widgets.Label(rect2, text5);
			Text.Font = GameFont.Small;
			if (this.bill.recipe.products.Count == 1)
			{
				Widgets.InfoCardButton(rect2.x, rect4.y, this.bill.recipe.products[0].thingDef);
			}
		}
	}
}
