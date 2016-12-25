using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WITab_Terrain : WITab
	{
		private static readonly Vector2 WinSize = new Vector2(440f, 540f);

		public override bool IsVisible
		{
			get
			{
				return base.SelTileID >= 0;
			}
		}

		public WITab_Terrain()
		{
			this.size = WITab_Terrain.WinSize;
			this.labelKey = "TabTerrain";
			this.tutorTag = "Terrain";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, WITab_Terrain.WinSize.x, WITab_Terrain.WinSize.y).ContractedBy(10f);
			Rect rect2 = rect;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect2, base.SelTile.biome.LabelCap);
			Rect rect3 = rect;
			rect3.yMin += 35f;
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard(rect3);
			listing_Standard.verticalSpacing = 0f;
			Tile selTile = base.SelTile;
			int selTileID = base.SelTileID;
			listing_Standard.Label(selTile.biome.description);
			listing_Standard.Gap(8f);
			listing_Standard.GapLine(12f);
			if (!selTile.biome.implemented)
			{
				listing_Standard.Label(selTile.biome.LabelCap + " " + "BiomeNotImplemented".Translate());
			}
			listing_Standard.LabelDouble("Terrain".Translate(), selTile.hilliness.GetLabelCap());
			if (!Find.World.Impassable(selTileID))
			{
				int num = (int)(1f / (ThingDefOf.Human.GetStatValueAbstract(StatDefOf.MoveSpeed, null) / 60f) * 190f);
				int numTicks = Mathf.Min(num + WorldPathGrid.CalculatedCostAt(selTileID, false, -1f), 120000);
				listing_Standard.LabelDouble("MovementTimeNow".Translate(), numTicks.ToStringTicksToPeriod(true));
				int numTicks2 = Mathf.Min(num + WorldPathGrid.CalculatedCostAt(selTileID, false, 0.5f), 120000);
				listing_Standard.LabelDouble("MovementTimeSummer".Translate(), numTicks2.ToStringTicksToPeriod(true));
				int numTicks3 = Mathf.Min(num + WorldPathGrid.CalculatedCostAt(selTileID, false, 0f), 120000);
				listing_Standard.LabelDouble("MovementTimeWinter".Translate(), numTicks3.ToStringTicksToPeriod(true));
			}
			if (selTile.biome.canBuildBase)
			{
				listing_Standard.LabelDouble("StoneTypesHere".Translate(), GenText.ToCommaList(from rt in Find.World.NaturalRockTypesIn(selTileID)
				select rt.label, true).CapitalizeFirst());
			}
			listing_Standard.LabelDouble("Elevation".Translate(), selTile.elevation.ToString("F0") + "m");
			listing_Standard.GapLine(12f);
			listing_Standard.LabelDouble("AvgTemp".Translate(), selTile.temperature.ToStringTemperature("F1"));
			float celsiusTemp = GenTemperature.AverageTemperatureAtTileForMonth(selTileID, Month.Jan);
			listing_Standard.LabelDouble("AvgWinterTemp".Translate(), celsiusTemp.ToStringTemperature("F1"));
			float celsiusTemp2 = GenTemperature.AverageTemperatureAtTileForMonth(selTileID, Month.Jul);
			listing_Standard.LabelDouble("AvgSummerTemp".Translate(), celsiusTemp2.ToStringTemperature("F1"));
			listing_Standard.LabelDouble("OutdoorGrowingPeriod".Translate(), Zone_Growing.GrowingMonthsDescription(selTileID));
			listing_Standard.LabelDouble("Rainfall".Translate(), selTile.rainfall.ToString("F0") + "mm");
			listing_Standard.GapLine(12f);
			listing_Standard.LabelDouble("TimeZone".Translate(), GenDate.TimeZoneAt(Find.WorldGrid.LongLatOf(selTileID).x).ToStringWithSign());
			Rot4 rot = Find.World.CoastDirectionAt(selTileID);
			if (rot.IsValid)
			{
				listing_Standard.LabelDouble(string.Empty, ("HasCoast" + rot.ToString()).Translate());
			}
			if (Prefs.DevMode)
			{
				listing_Standard.LabelDouble("Debug world tile ID", selTileID.ToString());
			}
			listing_Standard.End();
		}
	}
}
