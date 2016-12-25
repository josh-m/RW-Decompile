using System;
using System.Collections;
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
				return new Vector2(700f, 600f);
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
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
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
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(0f, 50f, 180f, inRect.height - 50f);
			Listing_Standard listing_Standard = new Listing_Standard(rect2);
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
				using (IEnumerator enumerator = Enum.GetValues(typeof(BillStoreMode)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						BillStoreMode billStoreMode = (BillStoreMode)((byte)enumerator.Current);
						BillStoreMode smLocal = billStoreMode;
						list.Add(new FloatMenuOption(("BillStoreMode_" + billStoreMode).Translate(), delegate
						{
							this.bill.storeMode = smLocal;
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			listing_Standard.Gap(12f);
			if (this.bill.repeatMode == BillRepeatMode.RepeatCount)
			{
				listing_Standard.Label("RepeatCount".Translate(new object[]
				{
					this.bill.RepeatInfoText
				}));
				listing_Standard.IntSetter(ref this.bill.repeatCount, 1, "1", 42f);
				listing_Standard.IntAdjuster(ref this.bill.repeatCount, 1, 0);
				listing_Standard.IntAdjuster(ref this.bill.repeatCount, 25, 0);
			}
			else if (this.bill.repeatMode == BillRepeatMode.TargetCount)
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
				listing_Standard.Label(text);
				listing_Standard.IntSetter(ref this.bill.targetCount, 1, "1", 42f);
				listing_Standard.IntAdjuster(ref this.bill.targetCount, 1, 1);
				listing_Standard.IntAdjuster(ref this.bill.targetCount, 25, 1);
				listing_Standard.IntAdjuster(ref this.bill.targetCount, 250, 1);
			}
			listing_Standard.Gap(12f);
			listing_Standard.Label("IngredientSearchRadius".Translate() + ": " + this.bill.ingredientSearchRadius.ToString("F0"));
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
				}));
				listing_Standard.IntRange(ref this.bill.allowedSkillRange, 0, 20);
			}
			listing_Standard.End();
			List<ThingDef> list2 = new List<ThingDef>();
			for (int i = 0; i < this.bill.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = this.bill.recipe.ingredients[i];
				if (!ingredientCount.filter.AllowedThingDefs.Skip(1).Any<ThingDef>())
				{
					list2.Add(ingredientCount.filter.AllowedThingDefs.First<ThingDef>());
				}
			}
			Rect rect3 = new Rect(rect2.xMax + 6f, 50f, 280f, -1f);
			rect3.yMax = inRect.height - this.CloseButSize.y - 6f;
			ThingFilterUI.DoThingFilterConfigWindow(rect3, ref this.scrollPosition, this.bill.ingredientFilter, this.bill.recipe.fixedIngredientFilter, 4, list2, this.bill.recipe.forceHiddenSpecialFilters);
			Rect rect4 = new Rect(rect3.xMax + 6f, rect3.y + 30f, 0f, 0f);
			rect4.xMax = inRect.xMax;
			rect4.yMax = inRect.height - this.CloseButSize.y - 6f;
			StringBuilder stringBuilder = new StringBuilder();
			if (this.bill.recipe.description != null)
			{
				stringBuilder.AppendLine(this.bill.recipe.description);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("WorkAmount".Translate() + ": " + this.bill.recipe.WorkAmountTotal(null).ToStringWorkAmount());
			stringBuilder.AppendLine();
			for (int j = 0; j < this.bill.recipe.ingredients.Count; j++)
			{
				IngredientCount ingredientCount2 = this.bill.recipe.ingredients[j];
				if (!ingredientCount2.filter.Summary.NullOrEmpty())
				{
					stringBuilder.AppendLine(this.bill.recipe.IngredientValueGetter.BillRequirementsDescription(ingredientCount2));
				}
			}
			stringBuilder.AppendLine();
			string text4 = this.bill.recipe.IngredientValueGetter.ExtraDescriptionLine(this.bill.recipe);
			if (text4 != null)
			{
				stringBuilder.AppendLine(text4);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("MinimumSkills".Translate());
			stringBuilder.AppendLine(this.bill.recipe.MinSkillString);
			Text.Font = GameFont.Small;
			string text5 = stringBuilder.ToString();
			if (Text.CalcHeight(text5, rect4.width) > rect4.height)
			{
				Text.Font = GameFont.Tiny;
			}
			Widgets.Label(rect4, text5);
			Text.Font = GameFont.Small;
			if (this.bill.recipe.products.Count == 1)
			{
				Widgets.InfoCardButton(rect4.x, rect3.y, this.bill.recipe.products[0].thingDef);
			}
		}
	}
}
