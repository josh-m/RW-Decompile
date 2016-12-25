using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_AdvancedGameConfig : Window
	{
		private const float ColumnWidth = 200f;

		private int selTile = -1;

		private static readonly int[] MapSizes = new int[]
		{
			200,
			225,
			250,
			275,
			300,
			325,
			350,
			400
		};

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(700f, 500f);
			}
		}

		public Dialog_AdvancedGameConfig(int selTile)
		{
			this.doCloseButton = true;
			this.closeOnEscapeKey = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.selTile = selTile;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard(inRect.AtZero());
			listing_Standard.ColumnWidth = 200f;
			listing_Standard.Label("MapSize".Translate());
			int[] mapSizes = Dialog_AdvancedGameConfig.MapSizes;
			for (int i = 0; i < mapSizes.Length; i++)
			{
				int num = mapSizes[i];
				if (num == 200)
				{
					listing_Standard.Label("MapSizeSmall".Translate());
				}
				else if (num == 250)
				{
					listing_Standard.Label("MapSizeMedium".Translate());
				}
				else if (num == 300)
				{
					listing_Standard.Label("MapSizeLarge".Translate());
				}
				else if (num == 350)
				{
					listing_Standard.Label("MapSizeExtreme".Translate());
				}
				string label = "MapSizeDesc".Translate(new object[]
				{
					num,
					num * num
				});
				if (listing_Standard.RadioButton(label, Find.GameInitData.mapSize == num, 0f))
				{
					Find.GameInitData.mapSize = num;
				}
			}
			listing_Standard.NewColumn();
			GenUI.SetLabelAlign(TextAnchor.MiddleCenter);
			listing_Standard.Label("MapStartSeason".Translate());
			string label2;
			if (Find.GameInitData.startingMonth == Month.Undefined)
			{
				label2 = "MapStartSeasonDefault".Translate();
			}
			else
			{
				label2 = Find.GameInitData.startingMonth.GetSeason().LabelCap();
			}
			Rect rect = listing_Standard.GetRect(32f);
			GridLayout gridLayout = new GridLayout(rect, 5, 1, 0f, 4f);
			if (Widgets.ButtonText(gridLayout.GetCellRectByIndex(0, 1, 1), "-", true, false, true))
			{
				Season season = Find.GameInitData.startingMonth.GetSeason();
				if (season == Season.Undefined)
				{
					season = Season.Winter;
				}
				else
				{
					season -= 1;
				}
				Find.GameInitData.startingMonth = season.GetFirstMonth();
			}
			Widgets.Label(gridLayout.GetCellRectByIndex(1, 3, 1), label2);
			if (Widgets.ButtonText(gridLayout.GetCellRectByIndex(4, 1, 1), "+", true, false, true))
			{
				Season season2 = Find.GameInitData.startingMonth.GetSeason();
				if (season2 == Season.Winter)
				{
					season2 = Season.Undefined;
				}
				else
				{
					season2 += 1;
				}
				Find.GameInitData.startingMonth = season2.GetFirstMonth();
			}
			GenUI.ResetLabelAlign();
			if (this.selTile >= 0 && Find.GameInitData.startingMonth != Month.Undefined && GenTemperature.AverageTemperatureAtTileForMonth(this.selTile, Find.GameInitData.startingMonth) < 3f)
			{
				listing_Standard.Label("MapTemperatureDangerWarning".Translate());
			}
			if (Find.GameInitData.mapSize > 250)
			{
				listing_Standard.Label("MapSizePerformanceWarning".Translate());
			}
			listing_Standard.End();
		}
	}
}
