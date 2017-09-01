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

		public const float SpaceBetweenMassInfoAndDaysWorthOfFood = 19f;

		public static void DrawDaysWorthOfFoodInfo(Rect rect, float daysWorthOfFood, float daysUntilRot, bool canEatLocalPlants, bool alignRight = false, float truncToWidth = 3.40282347E+38f)
		{
			GUI.color = Color.gray;
			string text;
			if (daysWorthOfFood >= 1000f)
			{
				text = "InfiniteDaysWorthOfFoodInfo".Translate();
			}
			else if (daysUntilRot < 1000f)
			{
				text = "DaysWorthOfFoodInfoRot".Translate(new object[]
				{
					daysWorthOfFood.ToString("0.#"),
					daysUntilRot.ToString("0.#")
				});
			}
			else
			{
				text = "DaysWorthOfFoodInfo".Translate(new object[]
				{
					daysWorthOfFood.ToString("0.#")
				});
			}
			string text2 = text;
			if (truncToWidth != 3.40282347E+38f)
			{
				text2 = text.Truncate(truncToWidth, null);
			}
			Vector2 vector = Text.CalcSize(text2);
			Rect rect2;
			if (alignRight)
			{
				rect2 = new Rect(rect.xMax - vector.x, rect.y, vector.x, vector.y);
			}
			else
			{
				rect2 = new Rect(rect.x, rect.y, vector.x, vector.y);
			}
			Widgets.Label(rect2, text2);
			string text3 = string.Empty;
			if (truncToWidth != 3.40282347E+38f && Text.CalcSize(text).x > truncToWidth)
			{
				text3 = text3 + text + "\n\n";
			}
			text3 = text3 + "DaysWorthOfFoodTooltip".Translate() + "\n\n";
			if (canEatLocalPlants)
			{
				text3 += "DaysWorthOfFoodTooltip_CanEatLocalPlants".Translate();
			}
			else
			{
				text3 += "DaysWorthOfFoodTooltip_CantEatLocalPlants".Translate();
			}
			TooltipHandler.TipRegion(rect2, text3);
			GUI.color = Color.white;
		}

		public static void CreateCaravanTransferableWidgets(List<TransferableOneWay> transferables, out TransferableOneWayWidget pawnsTransfer, out TransferableOneWayWidget itemsTransfer, string sourceLabel, string destLabel, string thingCountTip, IgnorePawnsInventoryMode ignorePawnInventoryMass, Func<float> availableMassGetter, bool ignoreCorpsesGearAndInventoryMass, int drawDaysUntilRotForTile)
		{
			pawnsTransfer = new TransferableOneWayWidget(null, sourceLabel, destLabel, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 24f, ignoreCorpsesGearAndInventoryMass, true, -1);
			CaravanUIUtility.AddPawnsSections(pawnsTransfer, transferables);
			itemsTransfer = new TransferableOneWayWidget(from x in transferables
			where x.ThingDef.category != ThingCategory.Pawn
			select x, sourceLabel, destLabel, thingCountTip, true, ignorePawnInventoryMass, false, availableMassGetter, 24f, ignoreCorpsesGearAndInventoryMass, true, drawDaysUntilRotForTile);
		}

		public static void AddPawnsSections(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
		{
			IEnumerable<TransferableOneWay> source = from x in transferables
			where x.ThingDef.category == ThingCategory.Pawn
			select x;
			widget.AddSection("ColonistsSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).IsFreeColonist
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
