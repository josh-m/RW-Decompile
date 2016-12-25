using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ManageAreas : Window
	{
		private Map map;

		private static Regex validNameRegex = new Regex("^[a-zA-Z0-9 '\\-]*$");

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(700f, 700f);
			}
		}

		public Dialog_ManageAreas(Map map)
		{
			this.map = map;
			this.forcePause = true;
			this.doCloseX = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard(inRect);
			listing_Standard.ColumnWidth = inRect.width;
			List<Area> allAreas = this.map.areaManager.AllAreas;
			for (int i = 0; i < allAreas.Count; i++)
			{
				if (allAreas[i].Mutable)
				{
					Rect rect = listing_Standard.GetRect(24f);
					Dialog_ManageAreas.DoAreaRow(rect, allAreas[i]);
					listing_Standard.Gap(6f);
				}
			}
			listing_Standard.ColumnWidth = inRect.width / 2f;
			if (this.map.areaManager.CanMakeNewAllowed(AllowedAreaMode.Humanlike) && listing_Standard.ButtonText("NewArea".Translate(), null))
			{
				Area_Allowed area_Allowed;
				this.map.areaManager.TryMakeNewAllowed(AllowedAreaMode.Humanlike, out area_Allowed);
			}
			if (this.map.areaManager.CanMakeNewAllowed(AllowedAreaMode.Animal) && listing_Standard.ButtonText("NewAreaAnimal".Translate(), null))
			{
				Area_Allowed area_Allowed2;
				this.map.areaManager.TryMakeNewAllowed(AllowedAreaMode.Animal, out area_Allowed2);
			}
			listing_Standard.End();
		}

		private static void DoAreaRow(Rect rect, Area area)
		{
			if (Mouse.IsOver(rect))
			{
				area.MarkForDraw();
				GUI.color = area.Color;
				Widgets.DrawHighlight(rect);
				GUI.color = Color.white;
			}
			GUI.BeginGroup(rect);
			WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
			widgetRow.Icon(area.ColorTexture, null);
			widgetRow.Gap(4f);
			widgetRow.Label(area.Label, 220f);
			if (widgetRow.ButtonText("Rename".Translate(), null, true, false))
			{
				Find.WindowStack.Add(new Dialog_RenameArea(area));
			}
			if (widgetRow.ButtonText("InvertArea".Translate(), null, true, false))
			{
				area.Invert();
			}
			if (widgetRow.ButtonIcon(TexButton.DeleteX, null))
			{
				area.Delete();
			}
			GUI.EndGroup();
		}

		public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
		{
			string text = Widgets.TextField(rect, name);
			if (text.Length <= maxLength && Dialog_ManageAreas.validNameRegex.IsMatch(text))
			{
				name = text;
			}
		}
	}
}
