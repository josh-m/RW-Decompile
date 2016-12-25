using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGlobalControls
	{
		public const float Width = 150f;

		private const int VisibilityControlsPerRow = 5;

		private WidgetRow rowVisibility = new WidgetRow();

		public void WorldGlobalControlsOnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			float leftX = (float)UI.screenWidth - 150f;
			float num = (float)UI.screenHeight - 4f;
			if (Current.ProgramState == ProgramState.Playing)
			{
				num -= 35f;
			}
			GlobalControlsUtility.DoPlaySettings(this.rowVisibility, true, ref num);
			if (Current.ProgramState == ProgramState.Playing)
			{
				num -= 4f;
				GlobalControlsUtility.DoTimespeedControls(leftX, 150f, ref num);
				if (Find.VisibleMap != null || Find.WorldSelector.AnyObjectOrTileSelected)
				{
					num -= 4f;
					GlobalControlsUtility.DoDate(leftX, 150f, ref num);
				}
			}
			if (Prefs.ShowRealtimeClock)
			{
				GlobalControlsUtility.DoRealtimeClock(leftX, 150f, ref num);
			}
			if (!Find.PlaySettings.lockNorthUp)
			{
				CompassWidget.CompassOnGUI(ref num);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				num -= 10f;
				Find.LetterStack.LettersOnGUI(num);
			}
		}
	}
}
