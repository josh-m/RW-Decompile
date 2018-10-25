using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ManageFoodRestrictions : Window
	{
		private Vector2 scrollPosition;

		private FoodRestriction selFoodRestrictionInt;

		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		private static ThingFilter foodGlobalFilter;

		private FoodRestriction SelectedFoodRestriction
		{
			get
			{
				return this.selFoodRestrictionInt;
			}
			set
			{
				this.CheckSelectedFoodRestrictionHasName();
				this.selFoodRestrictionInt = value;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(700f, 700f);
			}
		}

		public Dialog_ManageFoodRestrictions(FoodRestriction selectedFoodRestriction)
		{
			this.forcePause = true;
			this.doCloseX = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
			if (Dialog_ManageFoodRestrictions.foodGlobalFilter == null)
			{
				Dialog_ManageFoodRestrictions.foodGlobalFilter = new ThingFilter();
				Dialog_ManageFoodRestrictions.foodGlobalFilter.SetAllow(ThingCategoryDefOf.Foods, true, null, null);
				Dialog_ManageFoodRestrictions.foodGlobalFilter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, true, null, null);
				Dialog_ManageFoodRestrictions.foodGlobalFilter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, true, null, null);
			}
			this.SelectedFoodRestriction = selectedFoodRestriction;
		}

		private void CheckSelectedFoodRestrictionHasName()
		{
			if (this.SelectedFoodRestriction != null && this.SelectedFoodRestriction.label.NullOrEmpty())
			{
				this.SelectedFoodRestriction.label = "Unnamed";
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect, "SelectFoodRestriction".Translate(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FoodRestriction current in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					FoodRestriction localRestriction = current;
					list.Add(new FloatMenuOption(localRestriction.label, delegate
					{
						this.SelectedFoodRestriction = localRestriction;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num += 10f;
			Rect rect2 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect2, "NewFoodRestriction".Translate(), true, false, true))
			{
				this.SelectedFoodRestriction = Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
			}
			num += 10f;
			Rect rect3 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect3, "DeleteFoodRestriction".Translate(), true, false, true))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (FoodRestriction current2 in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					FoodRestriction localRestriction = current2;
					list2.Add(new FloatMenuOption(localRestriction.label, delegate
					{
						AcceptanceReport acceptanceReport = Current.Game.foodRestrictionDatabase.TryDelete(localRestriction);
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
						}
						else if (localRestriction == this.SelectedFoodRestriction)
						{
							this.SelectedFoodRestriction = null;
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			Rect rect4 = new Rect(0f, 40f, inRect.width, inRect.height - 40f - this.CloseButSize.y).ContractedBy(10f);
			if (this.SelectedFoodRestriction == null)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, "NoFoodRestrictionSelected".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				return;
			}
			GUI.BeginGroup(rect4);
			Rect rect5 = new Rect(0f, 0f, 200f, 30f);
			Dialog_ManageFoodRestrictions.DoNameInputRect(rect5, ref this.SelectedFoodRestriction.label);
			Rect rect6 = new Rect(0f, 40f, 300f, rect4.height - 45f - 10f);
			Rect rect7 = rect6;
			ThingFilter filter = this.SelectedFoodRestriction.filter;
			ThingFilter parentFilter = Dialog_ManageFoodRestrictions.foodGlobalFilter;
			IEnumerable<SpecialThingFilterDef> forceHiddenFilters = this.HiddenSpecialThingFilters();
			ThingFilterUI.DoThingFilterConfigWindow(rect7, ref this.scrollPosition, filter, parentFilter, 1, null, forceHiddenFilters, true, null, null);
			GUI.EndGroup();
		}

		[DebuggerHidden]
		private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
		{
			yield return SpecialThingFilterDefOf.AllowFresh;
		}

		public override void PreClose()
		{
			base.PreClose();
			this.CheckSelectedFoodRestrictionHasName();
		}

		public static void DoNameInputRect(Rect rect, ref string name)
		{
			name = Widgets.TextField(rect, name, 30, Outfit.ValidNameRegex);
		}
	}
}
