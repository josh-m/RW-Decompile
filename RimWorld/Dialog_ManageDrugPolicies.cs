using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Dialog_ManageDrugPolicies : Window
	{
		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		private const float DrugEntryRowHeight = 35f;

		private const float BottomButtonsAreaHeight = 50f;

		private const float AddEntryButtonHeight = 35f;

		private const float AddEntryButtonWidth = 150f;

		private const float CellsPadding = 4f;

		private const float UsageSpacing = 12f;

		private Vector2 scrollPosition;

		private DrugPolicy selPolicy;

		private static readonly Texture2D IconForAddiction = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/ForAddiction", true);

		private static readonly Texture2D IconForJoy = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/ForJoy", true);

		private static readonly Texture2D IconScheduled = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/Scheduled", true);

		private static readonly Regex ValidNameRegex = Outfit.ValidNameRegex;

		private DrugPolicy SelectedPolicy
		{
			get
			{
				return this.selPolicy;
			}
			set
			{
				this.CheckSelectedPolicyHasName();
				this.selPolicy = value;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(900f, 700f);
			}
		}

		public Dialog_ManageDrugPolicies(DrugPolicy selectedAssignedDrugs)
		{
			this.forcePause = true;
			this.doCloseX = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
			this.SelectedPolicy = selectedAssignedDrugs;
		}

		private void CheckSelectedPolicyHasName()
		{
			if (this.SelectedPolicy != null && this.SelectedPolicy.label.NullOrEmpty())
			{
				this.SelectedPolicy.label = "Unnamed";
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect, "SelectDrugPolicy".Translate(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (DrugPolicy current in Current.Game.drugPolicyDatabase.AllPolicies)
				{
					DrugPolicy localAssignedDrugs = current;
					list.Add(new FloatMenuOption(localAssignedDrugs.label, delegate
					{
						this.SelectedPolicy = localAssignedDrugs;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num += 10f;
			Rect rect2 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect2, "NewDrugPolicy".Translate(), true, false, true))
			{
				this.SelectedPolicy = Current.Game.drugPolicyDatabase.MakeNewDrugPolicy();
			}
			num += 10f;
			Rect rect3 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect3, "DeleteDrugPolicy".Translate(), true, false, true))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (DrugPolicy current2 in Current.Game.drugPolicyDatabase.AllPolicies)
				{
					DrugPolicy localAssignedDrugs = current2;
					list2.Add(new FloatMenuOption(localAssignedDrugs.label, delegate
					{
						AcceptanceReport acceptanceReport = Current.Game.drugPolicyDatabase.TryDelete(localAssignedDrugs);
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
						}
						else if (localAssignedDrugs == this.SelectedPolicy)
						{
							this.SelectedPolicy = null;
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			Rect rect4 = new Rect(0f, 40f, inRect.width, inRect.height - 40f - this.CloseButSize.y).ContractedBy(10f);
			if (this.SelectedPolicy == null)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, "NoDrugPolicySelected".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				return;
			}
			GUI.BeginGroup(rect4);
			Rect rect5 = new Rect(0f, 0f, 200f, 30f);
			Dialog_ManageDrugPolicies.DoNameInputRect(rect5, ref this.SelectedPolicy.label);
			Rect rect6 = new Rect(0f, 40f, rect4.width, rect4.height - 45f - 10f);
			this.DoPolicyConfigArea(rect6);
			GUI.EndGroup();
		}

		public override void PreClose()
		{
			base.PreClose();
			this.CheckSelectedPolicyHasName();
		}

		public static void DoNameInputRect(Rect rect, ref string name)
		{
			name = Widgets.TextField(rect, name, 30, Dialog_ManageDrugPolicies.ValidNameRegex);
		}

		private void DoPolicyConfigArea(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height = 54f;
			Rect rect3 = rect;
			rect3.yMin = rect2.yMax;
			rect3.height -= 50f;
			Rect rect4 = rect;
			rect4.yMin = rect4.yMax - 50f;
			this.DoColumnLabels(rect2);
			Widgets.DrawMenuSection(rect3, true);
			if (this.SelectedPolicy.Count == 0)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, "NoDrugs".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			else
			{
				float height = (float)this.SelectedPolicy.Count * 35f;
				Rect viewRect = new Rect(0f, 0f, rect3.width - 16f, height);
				Widgets.BeginScrollView(rect3, ref this.scrollPosition, viewRect);
				DrugPolicy selectedPolicy = this.SelectedPolicy;
				for (int i = 0; i < selectedPolicy.Count; i++)
				{
					Rect rect5 = new Rect(0f, (float)i * 35f, viewRect.width, 35f);
					this.DoEntryRow(rect5, selectedPolicy[i]);
				}
				Widgets.EndScrollView();
			}
		}

		private void CalculateColumnsWidths(Rect rect, out float addictionWidth, out float allowJoyWidth, out float scheduledWidth, out float drugNameWidth, out float frequencyWidth, out float moodThresholdWidth, out float joyThresholdWidth, out float takeToInventoryWidth)
		{
			float num = rect.width - 108f;
			drugNameWidth = num * 0.2f;
			addictionWidth = 36f;
			allowJoyWidth = 36f;
			scheduledWidth = 36f;
			frequencyWidth = num * 0.35f;
			moodThresholdWidth = num * 0.15f;
			joyThresholdWidth = num * 0.15f;
			takeToInventoryWidth = num * 0.15f;
		}

		private void DoColumnLabels(Rect rect)
		{
			rect.width -= 16f;
			float num;
			float num2;
			float num3;
			float num4;
			float num5;
			float num6;
			float num7;
			float num8;
			this.CalculateColumnsWidths(rect, out num, out num2, out num3, out num4, out num5, out num6, out num7, out num8);
			float num9 = rect.x;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect2 = new Rect(num9 + 4f, rect.y, num4, rect.height);
			Widgets.Label(rect2, "DrugColumnLabel".Translate());
			TooltipHandler.TipRegion(rect2, "DrugNameColumnDesc".Translate());
			num9 += num4;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect3 = new Rect(num9, rect.y, num2 + num2, rect.height / 2f);
			Widgets.Label(rect3, "DrugUsageColumnLabel".Translate());
			TooltipHandler.TipRegion(rect3, "DrugUsageColumnDesc".Translate());
			Rect rect4 = new Rect(num9, rect.yMax - 24f, 24f, 24f);
			GUI.DrawTexture(rect4, Dialog_ManageDrugPolicies.IconForAddiction);
			TooltipHandler.TipRegion(rect4, "DrugUsageTipForAddiction".Translate());
			num9 += num;
			Rect rect5 = new Rect(num9, rect.yMax - 24f, 24f, 24f);
			GUI.DrawTexture(rect5, Dialog_ManageDrugPolicies.IconForJoy);
			TooltipHandler.TipRegion(rect5, "DrugUsageTipForJoy".Translate());
			num9 += num2;
			Rect rect6 = new Rect(num9, rect.yMax - 24f, 24f, 24f);
			GUI.DrawTexture(rect6, Dialog_ManageDrugPolicies.IconScheduled);
			TooltipHandler.TipRegion(rect6, "DrugUsageTipScheduled".Translate());
			num9 += num3;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect7 = new Rect(num9, rect.y, num5, rect.height);
			Widgets.Label(rect7, "FrequencyColumnLabel".Translate());
			TooltipHandler.TipRegion(rect7, "FrequencyColumnDesc".Translate());
			num9 += num5;
			Rect rect8 = new Rect(num9, rect.y, num6, rect.height);
			Widgets.Label(rect8, "MoodThresholdColumnLabel".Translate());
			TooltipHandler.TipRegion(rect8, "MoodThresholdColumnDesc".Translate());
			num9 += num6;
			Rect rect9 = new Rect(num9, rect.y, num7, rect.height);
			Widgets.Label(rect9, "JoyThresholdColumnLabel".Translate());
			TooltipHandler.TipRegion(rect9, "JoyThresholdColumnDesc".Translate());
			num9 += num7;
			Rect rect10 = new Rect(num9, rect.y, num8, rect.height);
			Widgets.Label(rect10, "TakeToInventoryColumnLabel".Translate());
			TooltipHandler.TipRegion(rect10, "TakeToInventoryColumnDesc".Translate());
			num9 += num8;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DoEntryRow(Rect rect, DrugPolicyEntry entry)
		{
			float num;
			float num2;
			float num3;
			float num4;
			float num5;
			float num6;
			float num7;
			float num8;
			this.CalculateColumnsWidths(rect, out num, out num2, out num3, out num4, out num5, out num6, out num7, out num8);
			Text.Anchor = TextAnchor.MiddleLeft;
			float num9 = rect.x;
			Widgets.Label(new Rect(num9, rect.y, num4, rect.height).ContractedBy(4f), entry.drug.LabelCap);
			Widgets.InfoCardButton(num9 + Text.CalcSize(entry.drug.LabelCap).x + 5f, rect.y + (rect.height - 24f) / 2f, entry.drug);
			num9 += num4;
			if (entry.drug.IsAddictiveDrug)
			{
				Widgets.Checkbox(num9, rect.y, ref entry.allowedForAddiction, 24f, false);
			}
			num9 += num;
			if (entry.drug.IsPleasureDrug)
			{
				Widgets.Checkbox(num9, rect.y, ref entry.allowedForJoy, 24f, false);
			}
			num9 += num2;
			Widgets.Checkbox(num9, rect.y, ref entry.allowScheduled, 24f, false);
			num9 += num3;
			if (entry.allowScheduled)
			{
				entry.daysFrequency = Widgets.FrequencyHorizontalSlider(new Rect(num9, rect.y, num5, rect.height).ContractedBy(4f), entry.daysFrequency, 0.1f, 25f, true);
				num9 += num5;
				string label;
				if (entry.onlyIfMoodBelow < 1f)
				{
					label = entry.onlyIfMoodBelow.ToStringPercent();
				}
				else
				{
					label = "NoDrugUseRequirement".Translate();
				}
				entry.onlyIfMoodBelow = Widgets.HorizontalSlider(new Rect(num9, rect.y, num6, rect.height).ContractedBy(4f), entry.onlyIfMoodBelow, 0.01f, 1f, true, label, null, null, -1f);
				num9 += num6;
				string label2;
				if (entry.onlyIfJoyBelow < 1f)
				{
					label2 = entry.onlyIfJoyBelow.ToStringPercent();
				}
				else
				{
					label2 = "NoDrugUseRequirement".Translate();
				}
				entry.onlyIfJoyBelow = Widgets.HorizontalSlider(new Rect(num9, rect.y, num7, rect.height).ContractedBy(4f), entry.onlyIfJoyBelow, 0.01f, 1f, true, label2, null, null, -1f);
				num9 += num7;
				Widgets.TextFieldNumeric<int>(new Rect(num9, rect.y, num8, rect.height).ContractedBy(4f), ref entry.takeToInventory, ref entry.takeToInventoryTempBuffer, 0f, 15f);
				num9 += num8;
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
