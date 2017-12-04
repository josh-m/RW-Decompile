using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_AdvancedGameConfig : Window
	{
		private int selTile = -1;

		private const float ColumnWidth = 200f;

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
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = 200f;
			listing_Standard.Begin(inRect.AtZero());
			listing_Standard.Label("MapSize".Translate(), -1f);
			int[] mapSizes = Dialog_AdvancedGameConfig.MapSizes;
			for (int i = 0; i < mapSizes.Length; i++)
			{
				int num = mapSizes[i];
				if (num == 200)
				{
					listing_Standard.Label("MapSizeSmall".Translate(), -1f);
				}
				else if (num == 250)
				{
					listing_Standard.Label("MapSizeMedium".Translate(), -1f);
				}
				else if (num == 300)
				{
					listing_Standard.Label("MapSizeLarge".Translate(), -1f);
				}
				else if (num == 350)
				{
					listing_Standard.Label("MapSizeExtreme".Translate(), -1f);
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
			listing_Standard.Label("MapStartSeason".Translate(), -1f);
			string label2;
			if (Find.GameInitData.startingSeason == Season.Undefined)
			{
				label2 = "MapStartSeasonDefault".Translate();
			}
			else
			{
				label2 = Find.GameInitData.startingSeason.LabelCap();
			}
			Rect rect = listing_Standard.GetRect(32f);
			GridLayout gridLayout = new GridLayout(rect, 5, 1, 0f, 4f);
			if (Widgets.ButtonText(gridLayout.GetCellRectByIndex(0, 1, 1), "-", true, false, true))
			{
				Season season = Find.GameInitData.startingSeason;
				if (season == Season.Undefined)
				{
					season = Season.Winter;
				}
				else
				{
					season = (Season)(season - Season.Spring);
				}
				Find.GameInitData.startingSeason = season;
			}
			Widgets.Label(gridLayout.GetCellRectByIndex(1, 3, 1), label2);
			if (Widgets.ButtonText(gridLayout.GetCellRectByIndex(4, 1, 1), "+", true, false, true))
			{
				Season season2 = Find.GameInitData.startingSeason;
				if (season2 == Season.Winter)
				{
					season2 = Season.Undefined;
				}
				else
				{
					season2 += 1;
				}
				Find.GameInitData.startingSeason = season2;
			}
			GenUI.ResetLabelAlign();
			if (this.selTile >= 0 && Find.GameInitData.startingSeason != Season.Undefined)
			{
				float y = Find.WorldGrid.LongLatOf(this.selTile).y;
				if (GenTemperature.AverageTemperatureAtTileForTwelfth(this.selTile, Find.GameInitData.startingSeason.GetFirstTwelfth(y)) < 3f)
				{
					listing_Standard.Label("MapTemperatureDangerWarning".Translate(), -1f);
				}
			}
			if (Find.GameInitData.mapSize > 250)
			{
				listing_Standard.Label("MapSizePerformanceWarning".Translate(), -1f);
			}
			listing_Standard.End();
		}
	}
}
