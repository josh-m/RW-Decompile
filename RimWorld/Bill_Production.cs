using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Bill_Production : Bill, IExposable
	{
		public BillRepeatMode repeatMode;

		public int repeatCount = 1;

		public int targetCount = 10;

		public BillStoreMode storeMode = BillStoreMode.BestStockpile;

		public string RepeatInfoText
		{
			get
			{
				if (this.repeatMode == BillRepeatMode.Forever)
				{
					return "Forever".Translate();
				}
				if (this.repeatMode == BillRepeatMode.RepeatCount)
				{
					return this.repeatCount.ToString() + "x";
				}
				if (this.repeatMode == BillRepeatMode.TargetCount)
				{
					return this.recipe.WorkerCounter.CountProducts(this).ToString() + "/" + this.targetCount.ToString();
				}
				throw new InvalidOperationException();
			}
		}

		public Bill_Production()
		{
		}

		public Bill_Production(RecipeDef recipe) : base(recipe)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.repeatCount, "repeatCount", 0, false);
			Scribe_Values.LookValue<int>(ref this.targetCount, "targetCount", 0, false);
			Scribe_Values.LookValue<BillRepeatMode>(ref this.repeatMode, "repeatMode", BillRepeatMode.RepeatCount, false);
			Scribe_Values.LookValue<BillStoreMode>(ref this.storeMode, "storeMode", BillStoreMode.DropOnFloor, false);
		}

		public override BillStoreMode GetStoreMode()
		{
			return this.storeMode;
		}

		public override bool ShouldDoNow()
		{
			if (this.suspended)
			{
				return false;
			}
			if (this.repeatMode == BillRepeatMode.Forever)
			{
				return true;
			}
			if (this.repeatMode == BillRepeatMode.RepeatCount)
			{
				return this.repeatCount > 0;
			}
			if (this.repeatMode == BillRepeatMode.TargetCount)
			{
				return this.recipe.WorkerCounter.CountProducts(this) < this.targetCount;
			}
			throw new InvalidOperationException();
		}

		public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
			if (this.repeatMode == BillRepeatMode.RepeatCount)
			{
				this.repeatCount--;
				if (this.repeatCount == 0)
				{
					Messages.Message("MessageBillComplete".Translate(new object[]
					{
						this.LabelCap
					}), (Thing)this.billStack.billGiver, MessageSound.Benefit);
				}
			}
		}

		protected override void DoConfigInterface(Rect baseRect, Color baseColor)
		{
			Rect rect = new Rect(28f, 32f, 100f, 30f);
			GUI.color = new Color(1f, 1f, 1f, 0.65f);
			Widgets.Label(rect, this.RepeatInfoText);
			GUI.color = baseColor;
			WidgetRow widgetRow = new WidgetRow(baseRect.xMax, baseRect.y + 29f, UIDirection.LeftThenUp, 99999f, 4f);
			if (widgetRow.ButtonText("Details".Translate() + "...", null, true, false))
			{
				Find.WindowStack.Add(new Dialog_BillConfig(this, ((Thing)this.billStack.billGiver).Position));
			}
			if (widgetRow.ButtonText(this.repeatMode.GetLabel().PadRight(20), null, true, false))
			{
				BillRepeatModeUtility.MakeConfigFloatMenu(this);
			}
			if (widgetRow.ButtonIcon(TexButton.Plus, null))
			{
				if (this.repeatMode == BillRepeatMode.Forever)
				{
					this.repeatMode = BillRepeatMode.RepeatCount;
					this.repeatCount = 1;
				}
				else if (this.repeatMode == BillRepeatMode.TargetCount)
				{
					this.targetCount += this.recipe.targetCountAdjustment;
				}
				else if (this.repeatMode == BillRepeatMode.RepeatCount)
				{
					this.repeatCount++;
				}
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
				if (TutorSystem.TutorialMode && this.repeatMode == BillRepeatMode.RepeatCount)
				{
					TutorSystem.Notify_Event(this.recipe.defName + "-RepeatCountSetTo-" + this.repeatCount);
				}
			}
			if (widgetRow.ButtonIcon(TexButton.Minus, null))
			{
				if (this.repeatMode == BillRepeatMode.Forever)
				{
					this.repeatMode = BillRepeatMode.RepeatCount;
					this.repeatCount = 1;
				}
				else if (this.repeatMode == BillRepeatMode.TargetCount)
				{
					this.targetCount = Mathf.Max(0, this.targetCount - this.recipe.targetCountAdjustment);
				}
				else if (this.repeatMode == BillRepeatMode.RepeatCount)
				{
					this.repeatCount = Mathf.Max(0, this.repeatCount - 1);
				}
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
				if (TutorSystem.TutorialMode && this.repeatMode == BillRepeatMode.RepeatCount)
				{
					TutorSystem.Notify_Event(this.recipe.defName + "-RepeatCountSetTo-" + this.repeatCount);
				}
			}
		}
	}
}
