using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_KeyBindings : Window
	{
		protected const float EntryHeight = 34f;

		protected const float CategoryHeadingHeight = 40f;

		protected Vector2 scrollPosition;

		protected float contentHeight;

		protected KeyPrefsData keyPrefsData;

		protected Vector2 WindowSize = new Vector2(900f, 760f);

		private static List<KeyBindingDef> keyBindingsWorkingList = new List<KeyBindingDef>();

		public override Vector2 InitialSize
		{
			get
			{
				return this.WindowSize;
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public Dialog_KeyBindings()
		{
			this.forcePause = true;
			this.onlyOneOfTypeAllowed = true;
			this.absorbInputAroundWindow = true;
			this.scrollPosition = new Vector2(0f, 0f);
			this.keyPrefsData = KeyPrefs.KeyPrefsData.Clone();
			this.contentHeight = 0f;
			KeyBindingCategoryDef keyBindingCategoryDef = null;
			foreach (KeyBindingDef current in DefDatabase<KeyBindingDef>.AllDefs)
			{
				if (!current.devModeOnly || Prefs.DevMode)
				{
					if (keyBindingCategoryDef != current.category)
					{
						keyBindingCategoryDef = current.category;
						this.contentHeight += 44f;
					}
					this.contentHeight += 34f;
				}
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Vector2 vector = new Vector2(120f, 40f);
			float y = vector.y;
			float num = 600f;
			float num2 = (inRect.width - num) / 2f;
			Rect position = new Rect(num2 + inRect.x, inRect.y, num, inRect.height - (y + 10f)).ContractedBy(10f);
			Rect position2 = new Rect(position.x, position.y + position.height + 10f, position.width, y);
			GUI.BeginGroup(position);
			Rect rect = new Rect(0f, 0f, position.width, 40f);
			Text.Font = GameFont.Medium;
			GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
			Widgets.Label(rect, "KeyboardConfig".Translate());
			GenUI.ResetLabelAlign();
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(0f, rect.height, position.width, position.height - rect.height);
			Rect rect2 = new Rect(0f, 0f, outRect.width - 16f, this.contentHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect2);
			float num3 = 0f;
			KeyBindingCategoryDef keyBindingCategoryDef = null;
			Dialog_KeyBindings.keyBindingsWorkingList.Clear();
			Dialog_KeyBindings.keyBindingsWorkingList.AddRange(DefDatabase<KeyBindingDef>.AllDefs);
			Dialog_KeyBindings.keyBindingsWorkingList.SortBy((KeyBindingDef x) => x.category.index, (KeyBindingDef x) => x.index);
			for (int i = 0; i < Dialog_KeyBindings.keyBindingsWorkingList.Count; i++)
			{
				KeyBindingDef keyBindingDef = Dialog_KeyBindings.keyBindingsWorkingList[i];
				if (!keyBindingDef.devModeOnly || Prefs.DevMode)
				{
					if (keyBindingCategoryDef != keyBindingDef.category)
					{
						bool skipDrawing = num3 - this.scrollPosition.y + 40f < 0f || num3 - this.scrollPosition.y > outRect.height;
						keyBindingCategoryDef = keyBindingDef.category;
						this.DrawCategoryEntry(keyBindingCategoryDef, rect2.width, ref num3, skipDrawing);
					}
					bool skipDrawing2 = num3 - this.scrollPosition.y + 34f < 0f || num3 - this.scrollPosition.y > outRect.height;
					this.DrawKeyEntry(keyBindingDef, rect2, ref num3, skipDrawing2);
				}
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.BeginGroup(position2);
			int num4 = 3;
			float num5 = vector.x * (float)num4 + 10f * (float)(num4 - 1);
			float num6 = (position2.width - num5) / 2f;
			float num7 = vector.x + 10f;
			Rect rect3 = new Rect(num6, 0f, vector.x, vector.y);
			Rect rect4 = new Rect(num6 + num7, 0f, vector.x, vector.y);
			Rect rect5 = new Rect(num6 + num7 * 2f, 0f, vector.x, vector.y);
			if (Widgets.ButtonText(rect3, "ResetButton".Translate(), true, false, true))
			{
				this.keyPrefsData.ResetToDefaults();
				this.keyPrefsData.ErrorCheck();
				SoundDefOf.TickLow.PlayOneShotOnCamera();
				Event.current.Use();
			}
			if (Widgets.ButtonText(rect4, "CancelButton".Translate(), true, false, true))
			{
				this.Close(true);
				Event.current.Use();
			}
			if (Widgets.ButtonText(rect5, "OK".Translate(), true, false, true))
			{
				KeyPrefs.KeyPrefsData = this.keyPrefsData;
				KeyPrefs.Save();
				this.Close(true);
				this.keyPrefsData.ErrorCheck();
				Event.current.Use();
			}
			GUI.EndGroup();
		}

		private void DrawCategoryEntry(KeyBindingCategoryDef category, float width, ref float curY, bool skipDrawing)
		{
			if (!skipDrawing)
			{
				Rect rect = new Rect(0f, curY, width, 40f).ContractedBy(4f);
				Text.Font = GameFont.Medium;
				Widgets.Label(rect, category.LabelCap);
				Text.Font = GameFont.Small;
				if (!category.description.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect, new TipSignal(category.description));
				}
			}
			curY += 40f;
			if (!skipDrawing)
			{
				Color color = GUI.color;
				GUI.color = new Color(0.3f, 0.3f, 0.3f);
				Widgets.DrawLineHorizontal(0f, curY, width);
				GUI.color = color;
			}
			curY += 4f;
		}

		private void DrawKeyEntry(KeyBindingDef keyDef, Rect parentRect, ref float curY, bool skipDrawing)
		{
			if (!skipDrawing)
			{
				Rect rect = new Rect(parentRect.x, parentRect.y + curY, parentRect.width, 34f).ContractedBy(3f);
				GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
				Widgets.Label(rect, keyDef.LabelCap);
				GenUI.ResetLabelAlign();
				float num = 4f;
				Vector2 vector = new Vector2(140f, 28f);
				Rect rect2 = new Rect(rect.x + rect.width - vector.x * 2f - num, rect.y, vector.x, vector.y);
				Rect rect3 = new Rect(rect.x + rect.width - vector.x, rect.y, vector.x, vector.y);
				TooltipHandler.TipRegion(rect2, new TipSignal("BindingButtonToolTip".Translate()));
				TooltipHandler.TipRegion(rect3, new TipSignal("BindingButtonToolTip".Translate()));
				if (Widgets.ButtonText(rect2, this.keyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.A).ToStringReadable(), true, false, true))
				{
					this.SettingButtonClicked(keyDef, KeyPrefs.BindingSlot.A);
				}
				if (Widgets.ButtonText(rect3, this.keyPrefsData.GetBoundKeyCode(keyDef, KeyPrefs.BindingSlot.B).ToStringReadable(), true, false, true))
				{
					this.SettingButtonClicked(keyDef, KeyPrefs.BindingSlot.B);
				}
			}
			curY += 34f;
		}

		private void SettingButtonClicked(KeyBindingDef keyDef, KeyPrefs.BindingSlot slot)
		{
			if (Event.current.button == 0)
			{
				Find.WindowStack.Add(new Dialog_DefineBinding(this.keyPrefsData, keyDef, slot));
				Event.current.Use();
			}
			else if (Event.current.button == 1)
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("ResetBinding".Translate(), delegate
				{
					KeyCode keyCode = (slot != KeyPrefs.BindingSlot.A) ? keyDef.defaultKeyCodeB : keyDef.defaultKeyCodeA;
					this.keyPrefsData.SetBinding(keyDef, slot, keyCode);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				list.Add(new FloatMenuOption("ClearBinding".Translate(), delegate
				{
					this.keyPrefsData.SetBinding(keyDef, slot, KeyCode.None);
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}
	}
}
