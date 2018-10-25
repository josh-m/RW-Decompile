using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanUIUtility
	{
		public struct CaravanInfo
		{
			public float massUsage;

			public float massCapacity;

			public string massCapacityExplanation;

			public float tilesPerDay;

			public string tilesPerDayExplanation;

			public Pair<float, float> daysWorthOfFood;

			public Pair<ThingDef, float> foragedFoodPerDay;

			public string foragedFoodPerDayExplanation;

			public float visibility;

			public string visibilityExplanation;

			public float extraMassUsage;

			public float extraMassCapacity;

			public string extraMassCapacityExplanation;

			public CaravanInfo(float massUsage, float massCapacity, string massCapacityExplanation, float tilesPerDay, string tilesPerDayExplanation, Pair<float, float> daysWorthOfFood, Pair<ThingDef, float> foragedFoodPerDay, string foragedFoodPerDayExplanation, float visibility, string visibilityExplanation, float extraMassUsage = -1f, float extraMassCapacity = -1f, string extraMassCapacityExplanation = null)
			{
				this.massUsage = massUsage;
				this.massCapacity = massCapacity;
				this.massCapacityExplanation = massCapacityExplanation;
				this.tilesPerDay = tilesPerDay;
				this.tilesPerDayExplanation = tilesPerDayExplanation;
				this.daysWorthOfFood = daysWorthOfFood;
				this.foragedFoodPerDay = foragedFoodPerDay;
				this.foragedFoodPerDayExplanation = foragedFoodPerDayExplanation;
				this.visibility = visibility;
				this.visibilityExplanation = visibilityExplanation;
				this.extraMassUsage = extraMassUsage;
				this.extraMassCapacity = extraMassCapacity;
				this.extraMassCapacityExplanation = extraMassCapacityExplanation;
			}
		}

		private static readonly List<Pair<float, Color>> MassColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0.37f, Color.green),
			new Pair<float, Color>(0.82f, Color.yellow),
			new Pair<float, Color>(1f, new Color(1f, 0.6f, 0f))
		};

		private static readonly List<Pair<float, Color>> TilesPerDayColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0f, Color.white),
			new Pair<float, Color>(0.001f, Color.red),
			new Pair<float, Color>(1f, Color.yellow),
			new Pair<float, Color>(2f, Color.white)
		};

		private static readonly List<Pair<float, Color>> DaysWorthOfFoodColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(1f, Color.red),
			new Pair<float, Color>(2f, Color.white)
		};

		private static readonly List<Pair<float, Color>> DaysWorthOfFoodKnownRouteColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0.3f, Color.red),
			new Pair<float, Color>(0.9f, Color.yellow),
			new Pair<float, Color>(1.02f, Color.green)
		};

		private static readonly List<Pair<float, Color>> VisibilityColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0f, Color.white),
			new Pair<float, Color>(0.01f, Color.green),
			new Pair<float, Color>(0.2f, Color.green),
			new Pair<float, Color>(1f, Color.white),
			new Pair<float, Color>(1.2f, Color.red)
		};

		private static List<TransferableUIUtility.ExtraInfo> tmpInfo = new List<TransferableUIUtility.ExtraInfo>();

		public static void CreateCaravanTransferableWidgets(List<TransferableOneWay> transferables, out TransferableOneWayWidget pawnsTransfer, out TransferableOneWayWidget itemsTransfer, string thingCountTip, IgnorePawnsInventoryMode ignorePawnInventoryMass, Func<float> availableMassGetter, bool ignoreSpawnedCorpsesGearAndInventoryMass, int tile, bool playerPawnsReadOnly = false)
		{
			IEnumerable<TransferableOneWay> transferables2 = null;
			string sourceLabel = null;
			string destinationLabel = null;
			bool drawMass = true;
			bool includePawnsMassInMassUsage = false;
			pawnsTransfer = new TransferableOneWayWidget(transferables2, sourceLabel, destinationLabel, thingCountTip, drawMass, ignorePawnInventoryMass, includePawnsMassInMassUsage, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile, true, true, true, false, true, false, playerPawnsReadOnly);
			CaravanUIUtility.AddPawnsSections(pawnsTransfer, transferables);
			transferables2 = from x in transferables
			where x.ThingDef.category != ThingCategory.Pawn
			select x;
			string sourceLabel2 = null;
			destinationLabel = null;
			bool drawMass2 = true;
			bool includePawnsMassInMassUsage2 = false;
			itemsTransfer = new TransferableOneWayWidget(transferables2, sourceLabel2, destinationLabel, thingCountTip, drawMass2, ignorePawnInventoryMass, includePawnsMassInMassUsage2, availableMassGetter, 0f, ignoreSpawnedCorpsesGearAndInventoryMass, tile, true, false, false, true, false, true, false);
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
			widget.AddSection("CaptureSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).Downed && CaravanUtility.ShouldAutoCapture((Pawn)x.AnyThing, Faction.OfPlayer)
			select x);
			widget.AddSection("AnimalsSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).RaceProps.Animal
			select x);
		}

		private static string GetDaysWorthOfFoodLabel(Pair<float, float> daysWorthOfFood, bool multiline)
		{
			if (daysWorthOfFood.First >= 600f)
			{
				return "InfiniteDaysWorthOfFoodInfo".Translate();
			}
			string text = daysWorthOfFood.First.ToString("0.#");
			string str = (!multiline) ? " " : "\n";
			if (daysWorthOfFood.Second < 600f && daysWorthOfFood.Second < daysWorthOfFood.First)
			{
				text = text + str + "(" + "DaysWorthOfFoodInfoRot".Translate(daysWorthOfFood.Second.ToString("0.#") + ")");
			}
			return text;
		}

		private static Color GetDaysWorthOfFoodColor(Pair<float, float> daysWorthOfFood, int? ticksToArrive)
		{
			if (daysWorthOfFood.First >= 600f)
			{
				return Color.white;
			}
			float num = Mathf.Min(daysWorthOfFood.First, daysWorthOfFood.Second);
			if (ticksToArrive.HasValue)
			{
				return GenUI.LerpColor(CaravanUIUtility.DaysWorthOfFoodKnownRouteColor, num / ((float)ticksToArrive.Value / 60000f));
			}
			return GenUI.LerpColor(CaravanUIUtility.DaysWorthOfFoodColor, num);
		}

		public static void DrawCaravanInfo(CaravanUIUtility.CaravanInfo info, CaravanUIUtility.CaravanInfo? info2, int currentTile, int? ticksToArrive, float lastMassFlashTime, Rect rect, bool lerpMassColor = true, string extraDaysWorthOfFoodTipInfo = null, bool multiline = false)
		{
			CaravanUIUtility.tmpInfo.Clear();
			string value = string.Concat(new string[]
			{
				info.massUsage.ToStringEnsureThreshold(info.massCapacity, 0),
				" / ",
				info.massCapacity.ToString("F0"),
				" ",
				"kg".Translate()
			});
			string secondValue = (!info2.HasValue) ? null : string.Concat(new string[]
			{
				info2.Value.massUsage.ToStringEnsureThreshold(info2.Value.massCapacity, 0),
				" / ",
				info2.Value.massCapacity.ToString("F0"),
				" ",
				"kg".Translate()
			});
			CaravanUIUtility.tmpInfo.Add(new TransferableUIUtility.ExtraInfo("Mass".Translate(), value, CaravanUIUtility.GetMassColor(info.massUsage, info.massCapacity, lerpMassColor), CaravanUIUtility.GetMassTip(info.massUsage, info.massCapacity, info.massCapacityExplanation, (!info2.HasValue) ? null : new float?(info2.Value.massUsage), (!info2.HasValue) ? null : new float?(info2.Value.massCapacity), (!info2.HasValue) ? null : info2.Value.massCapacityExplanation), secondValue, (!info2.HasValue) ? Color.white : CaravanUIUtility.GetMassColor(info2.Value.massUsage, info2.Value.massCapacity, lerpMassColor), lastMassFlashTime));
			if (info.extraMassUsage != -1f)
			{
				string value2 = string.Concat(new string[]
				{
					info.extraMassUsage.ToStringEnsureThreshold(info.extraMassCapacity, 0),
					" / ",
					info.extraMassCapacity.ToString("F0"),
					" ",
					"kg".Translate()
				});
				string secondValue2 = (!info2.HasValue) ? null : string.Concat(new string[]
				{
					info2.Value.extraMassUsage.ToStringEnsureThreshold(info2.Value.extraMassCapacity, 0),
					" / ",
					info2.Value.extraMassCapacity.ToString("F0"),
					" ",
					"kg".Translate()
				});
				CaravanUIUtility.tmpInfo.Add(new TransferableUIUtility.ExtraInfo("CaravanMass".Translate(), value2, CaravanUIUtility.GetMassColor(info.extraMassUsage, info.extraMassCapacity, true), CaravanUIUtility.GetMassTip(info.extraMassUsage, info.extraMassCapacity, info.extraMassCapacityExplanation, (!info2.HasValue) ? null : new float?(info2.Value.extraMassUsage), (!info2.HasValue) ? null : new float?(info2.Value.extraMassCapacity), (!info2.HasValue) ? null : info2.Value.extraMassCapacityExplanation), secondValue2, (!info2.HasValue) ? Color.white : CaravanUIUtility.GetMassColor(info2.Value.extraMassUsage, info2.Value.extraMassCapacity, true), -9999f));
			}
			string text = "CaravanMovementSpeedTip".Translate();
			if (!info.tilesPerDayExplanation.NullOrEmpty())
			{
				text = text + "\n\n" + info.tilesPerDayExplanation;
			}
			if (info2.HasValue && !info2.Value.tilesPerDayExplanation.NullOrEmpty())
			{
				text = text + "\n\n-----\n\n" + info2.Value.tilesPerDayExplanation;
			}
			CaravanUIUtility.tmpInfo.Add(new TransferableUIUtility.ExtraInfo("CaravanMovementSpeed".Translate(), info.tilesPerDay.ToString("0.#") + " " + "TilesPerDay".Translate(), GenUI.LerpColor(CaravanUIUtility.TilesPerDayColor, info.tilesPerDay), text, (!info2.HasValue) ? null : (info2.Value.tilesPerDay.ToString("0.#") + " " + "TilesPerDay".Translate()), (!info2.HasValue) ? Color.white : GenUI.LerpColor(CaravanUIUtility.TilesPerDayColor, info2.Value.tilesPerDay), -9999f));
			CaravanUIUtility.tmpInfo.Add(new TransferableUIUtility.ExtraInfo("DaysWorthOfFood".Translate(), CaravanUIUtility.GetDaysWorthOfFoodLabel(info.daysWorthOfFood, multiline), CaravanUIUtility.GetDaysWorthOfFoodColor(info.daysWorthOfFood, ticksToArrive), "DaysWorthOfFoodTooltip".Translate() + extraDaysWorthOfFoodTipInfo + "\n\n" + VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(currentTile, Find.TickManager.TicksAbs), (!info2.HasValue) ? null : CaravanUIUtility.GetDaysWorthOfFoodLabel(info2.Value.daysWorthOfFood, multiline), (!info2.HasValue) ? Color.white : CaravanUIUtility.GetDaysWorthOfFoodColor(info2.Value.daysWorthOfFood, ticksToArrive), -9999f));
			string text2 = info.foragedFoodPerDay.Second.ToString("0.#");
			string text3 = (!info2.HasValue) ? null : info2.Value.foragedFoodPerDay.Second.ToString("0.#");
			string text4 = "ForagedFoodPerDayTip".Translate();
			text4 = text4 + "\n\n" + info.foragedFoodPerDayExplanation;
			if (info2.HasValue)
			{
				text4 = text4 + "\n\n-----\n\n" + info2.Value.foragedFoodPerDayExplanation;
			}
			if (info.foragedFoodPerDay.Second > 0f || (info2.HasValue && info2.Value.foragedFoodPerDay.Second > 0f))
			{
				string text5 = (!multiline) ? " " : "\n";
				if (!info2.HasValue)
				{
					string text6 = text2;
					text2 = string.Concat(new string[]
					{
						text6,
						text5,
						"(",
						info.foragedFoodPerDay.First.label,
						")"
					});
				}
				else
				{
					string text6 = text3;
					text3 = string.Concat(new string[]
					{
						text6,
						text5,
						"(",
						info2.Value.foragedFoodPerDay.First.label.Truncate(50f, null),
						")"
					});
				}
			}
			CaravanUIUtility.tmpInfo.Add(new TransferableUIUtility.ExtraInfo("ForagedFoodPerDay".Translate(), text2, Color.white, text4, text3, Color.white, -9999f));
			string text7 = "CaravanVisibilityTip".Translate();
			if (!info.visibilityExplanation.NullOrEmpty())
			{
				text7 = text7 + "\n\n" + info.visibilityExplanation;
			}
			if (info2.HasValue && !info2.Value.visibilityExplanation.NullOrEmpty())
			{
				text7 = text7 + "\n\n-----\n\n" + info2.Value.visibilityExplanation;
			}
			CaravanUIUtility.tmpInfo.Add(new TransferableUIUtility.ExtraInfo("Visibility".Translate(), info.visibility.ToStringPercent(), GenUI.LerpColor(CaravanUIUtility.VisibilityColor, info.visibility), text7, (!info2.HasValue) ? null : info2.Value.visibility.ToStringPercent(), (!info2.HasValue) ? Color.white : GenUI.LerpColor(CaravanUIUtility.VisibilityColor, info2.Value.visibility), -9999f));
			TransferableUIUtility.DrawExtraInfo(CaravanUIUtility.tmpInfo, rect);
		}

		private static Color GetMassColor(float massUsage, float massCapacity, bool lerpMassColor)
		{
			if (massCapacity == 0f)
			{
				return Color.white;
			}
			if (massUsage > massCapacity)
			{
				return Color.red;
			}
			if (lerpMassColor)
			{
				return GenUI.LerpColor(CaravanUIUtility.MassColor, massUsage / massCapacity);
			}
			return Color.white;
		}

		private static string GetMassTip(float massUsage, float massCapacity, string massCapacityExplanation, float? massUsage2, float? massCapacity2, string massCapacity2Explanation)
		{
			string text = string.Concat(new string[]
			{
				"MassCarriedSimple".Translate(),
				": ",
				massUsage.ToStringEnsureThreshold(massCapacity, 2),
				" ",
				"kg".Translate(),
				"\n",
				"MassCapacity".Translate(),
				": ",
				massCapacity.ToString("F2"),
				" ",
				"kg".Translate()
			});
			if (massUsage2.HasValue)
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n <-> \n",
					"MassCarriedSimple".Translate(),
					": ",
					massUsage2.Value.ToStringEnsureThreshold(massCapacity2.Value, 2),
					" ",
					"kg".Translate(),
					"\n",
					"MassCapacity".Translate(),
					": ",
					massCapacity2.Value.ToString("F2"),
					" ",
					"kg".Translate()
				});
			}
			text = text + "\n\n" + "CaravanMassUsageTooltip".Translate();
			if (!massCapacityExplanation.NullOrEmpty())
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n\n",
					"MassCapacity".Translate(),
					":\n",
					massCapacityExplanation
				});
			}
			if (!massCapacity2Explanation.NullOrEmpty())
			{
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					"\n\n-----\n\n",
					"MassCapacity".Translate(),
					":\n",
					massCapacity2Explanation
				});
			}
			return text;
		}
	}
}
