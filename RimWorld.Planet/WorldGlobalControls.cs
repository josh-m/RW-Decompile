using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGlobalControls
	{
		public const float Width = 200f;

		private const int VisibilityControlsPerRow = 5;

		private WidgetRow rowVisibility = new WidgetRow();

		public void WorldGlobalControlsOnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			float num = (float)UI.screenWidth - 200f;
			float num2 = (float)UI.screenHeight - 4f;
			if (Current.ProgramState == ProgramState.Playing)
			{
				num2 -= 35f;
			}
			GlobalControlsUtility.DoPlaySettings(this.rowVisibility, true, ref num2);
			if (Current.ProgramState == ProgramState.Playing)
			{
				num2 -= 4f;
				GlobalControlsUtility.DoTimespeedControls(num, 200f, ref num2);
				if (Find.VisibleMap != null || Find.WorldSelector.AnyObjectOrTileSelected)
				{
					num2 -= 4f;
					GlobalControlsUtility.DoDate(num, 200f, ref num2);
				}
				float num3 = 230f;
				float num4 = Find.World.gameConditionManager.TotalHeightAt(num3 - 15f);
				Rect rect = new Rect(num - 30f, num2 - num4, num3, num4);
				Find.World.gameConditionManager.DoConditionsUI(rect);
				num2 -= rect.height;
			}
			if (Prefs.ShowRealtimeClock)
			{
				GlobalControlsUtility.DoRealtimeClock(num, 200f, ref num2);
			}
			Find.WorldRoutePlanner.DoRoutePlannerButton(ref num2);
			if (!Find.PlaySettings.lockNorthUp)
			{
				CompassWidget.CompassOnGUI(ref num2);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				num2 -= 10f;
				Find.LetterStack.LettersOnGUI(num2);
			}
		}
	}
}
