using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class CaravanUIUtility
	{
		public const float ExtraSpaceForDaysWorthOfFoodReadout = 24f;

		public const float SpaceBetweenMassInfoAndDaysWorthOfFood = 22f;

		public static void DrawDaysWorthOfFoodInfo(Rect rect, float daysWorthOfFood, bool alignRight = false)
		{
			GUI.color = Color.gray;
			string text;
			if (daysWorthOfFood >= 1000f)
			{
				text = "InfiniteDaysWorthOfFoodInfo".Translate();
			}
			else
			{
				text = "DaysWorthOfFoodInfo".Translate(new object[]
				{
					daysWorthOfFood.ToString("0.#")
				});
			}
			Vector2 vector = Text.CalcSize(text);
			Rect rect2;
			if (alignRight)
			{
				rect2 = new Rect(rect.xMax - vector.x, rect.y, vector.x, vector.y);
			}
			else
			{
				rect2 = new Rect(rect.x, rect.y, vector.x, vector.y);
			}
			Widgets.Label(rect2, text);
			TooltipHandler.TipRegion(rect2, "DaysWorthOfFoodTooltip".Translate());
			GUI.color = Color.white;
		}

		public static void CreateCaravanTransferableWidgets(List<TransferableOneWay> transferables, out TransferableOneWayWidget pawnsTransfer, out TransferableOneWayWidget itemsTransfer, string sourceLabel, string destLabel, string thingCountTip, bool ignorePawnInventoryMass, Func<float> availableMassGetter, bool ignoreCorpsesGearAndInventoryMass)
		{
			pawnsTransfer = new TransferableOneWayWidget(null, sourceLabel, destLabel, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 24f, ignoreCorpsesGearAndInventoryMass, true);
			CaravanUIUtility.AddPawnsSections(pawnsTransfer, transferables);
			itemsTransfer = new TransferableOneWayWidget(from x in transferables
			where x.ThingDef.category != ThingCategory.Pawn
			select x, sourceLabel, destLabel, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 24f, ignoreCorpsesGearAndInventoryMass, true);
		}

		public static void AddPawnsSections(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
		{
			IEnumerable<TransferableOneWay> source = from x in transferables
			where x.ThingDef.category == ThingCategory.Pawn
			select x;
			widget.AddSection("ColonistsSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).IsColonist
			select x);
			widget.AddSection("PrisonersSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).IsPrisoner
			select x);
			widget.AddSection("AnimalsSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).RaceProps.Animal
			select x);
		}
	}
}
