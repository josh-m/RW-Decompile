using System;
using Verse;

namespace RimWorld
{
	public sealed class PlaySettings : IExposable
	{
		public bool showLearningHelper = true;

		public bool showZones = true;

		public bool showBeauty;

		public bool showRoomStats;

		public bool showColonistBar = true;

		public bool showRoofOverlay;

		public bool autoHomeArea = true;

		public bool useWorkPriorities;

		public void ExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.showLearningHelper, "showLearningHelper", false, false);
			Scribe_Values.LookValue<bool>(ref this.showZones, "showZones", false, false);
			Scribe_Values.LookValue<bool>(ref this.showBeauty, "showBeauty", false, false);
			Scribe_Values.LookValue<bool>(ref this.showRoomStats, "showRoomStats", false, false);
			Scribe_Values.LookValue<bool>(ref this.showColonistBar, "showColonistBar", false, false);
			Scribe_Values.LookValue<bool>(ref this.showRoofOverlay, "showRoofOverlay", false, false);
			Scribe_Values.LookValue<bool>(ref this.autoHomeArea, "autoHomeArea", false, false);
			Scribe_Values.LookValue<bool>(ref this.useWorkPriorities, "useWorkPriorities", false, false);
		}

		public void DoPlaySettingsGlobalControls(WidgetRow row)
		{
			row.ToggleableIcon(ref this.showLearningHelper, TexButton.ShowLearningHelper, "ShowLearningHelperWhenEmptyToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			row.ToggleableIcon(ref this.showZones, TexButton.ShowZones, "ZoneVisibilityToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			row.ToggleableIcon(ref this.showBeauty, TexButton.ShowBeauty, "ShowBeautyToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			row.ToggleableIcon(ref this.showRoomStats, TexButton.ShowRoomStats, "ShowRoomStatsToggleButton".Translate(), SoundDefOf.MouseoverToggle, "InspectRoomStats");
			row.ToggleableIcon(ref this.showColonistBar, TexButton.ShowColonistBar, "ShowColonistBarToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			row.ToggleableIcon(ref this.showRoofOverlay, TexButton.ShowRoofOverlay, "ShowRoofOverlayToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			row.ToggleableIcon(ref this.autoHomeArea, TexButton.AutoHomeArea, "AutoHomeAreaToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			bool resourceReadoutCategorized = Prefs.ResourceReadoutCategorized;
			bool flag = resourceReadoutCategorized;
			row.ToggleableIcon(ref resourceReadoutCategorized, TexButton.CategorizedResourceReadout, "CategorizedResourceReadoutToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			if (resourceReadoutCategorized != flag)
			{
				Prefs.ResourceReadoutCategorized = resourceReadoutCategorized;
			}
		}
	}
}
