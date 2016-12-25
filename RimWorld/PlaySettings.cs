using System;
using Verse;

namespace RimWorld
{
	public sealed class PlaySettings : IExposable
	{
		public bool showLearningHelper = true;

		public bool showZones = true;

		public bool showEnvironment;

		public bool showColonistBar = true;

		public bool showRoofOverlay;

		public bool autoHomeArea = true;

		public bool lockNorthUp = true;

		public bool usePlanetDayNightSystem = true;

		public bool expandingIcons = true;

		public bool useWorkPriorities;

		public void ExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.showLearningHelper, "showLearningHelper", false, false);
			Scribe_Values.LookValue<bool>(ref this.showZones, "showZones", false, false);
			Scribe_Values.LookValue<bool>(ref this.showEnvironment, "showEnvironment", false, false);
			Scribe_Values.LookValue<bool>(ref this.showColonistBar, "showColonistBar", false, false);
			Scribe_Values.LookValue<bool>(ref this.showRoofOverlay, "showRoofOverlay", false, false);
			Scribe_Values.LookValue<bool>(ref this.autoHomeArea, "autoHomeArea", false, false);
			Scribe_Values.LookValue<bool>(ref this.lockNorthUp, "lockNorthUp", false, false);
			Scribe_Values.LookValue<bool>(ref this.usePlanetDayNightSystem, "usePlanetDayNightSystem", false, false);
			Scribe_Values.LookValue<bool>(ref this.expandingIcons, "expandingIcons", false, false);
			Scribe_Values.LookValue<bool>(ref this.useWorkPriorities, "useWorkPriorities", false, false);
		}

		public void DoPlaySettingsGlobalControls(WidgetRow row, bool worldView)
		{
			bool flag = this.showColonistBar;
			if (worldView)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					row.ToggleableIcon(ref this.showColonistBar, TexButton.ShowColonistBar, "ShowColonistBarToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				}
				bool flag2 = this.lockNorthUp;
				row.ToggleableIcon(ref this.lockNorthUp, TexButton.LockNorthUp, "LockNorthUpToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				if (flag2 != this.lockNorthUp && this.lockNorthUp)
				{
					Find.WorldCameraDriver.RotateSoNorthIsUp(true);
				}
				if (Current.ProgramState == ProgramState.Playing)
				{
					row.ToggleableIcon(ref this.usePlanetDayNightSystem, TexButton.UsePlanetDayNightSystem, "UsePlanetDayNightSystemToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				}
				row.ToggleableIcon(ref this.expandingIcons, TexButton.ExpandingIcons, "ExpandingIconsToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
			}
			else
			{
				row.ToggleableIcon(ref this.showLearningHelper, TexButton.ShowLearningHelper, "ShowLearningHelperWhenEmptyToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				row.ToggleableIcon(ref this.showZones, TexButton.ShowZones, "ZoneVisibilityToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				row.ToggleableIcon(ref this.showEnvironment, TexButton.ShowEnvironment, "ShowEnvironmentToggleButton".Translate(), SoundDefOf.MouseoverToggle, "InspectRoomStats");
				row.ToggleableIcon(ref this.showColonistBar, TexButton.ShowColonistBar, "ShowColonistBarToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				row.ToggleableIcon(ref this.showRoofOverlay, TexButton.ShowRoofOverlay, "ShowRoofOverlayToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				row.ToggleableIcon(ref this.autoHomeArea, TexButton.AutoHomeArea, "AutoHomeAreaToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				bool resourceReadoutCategorized = Prefs.ResourceReadoutCategorized;
				bool flag3 = resourceReadoutCategorized;
				row.ToggleableIcon(ref resourceReadoutCategorized, TexButton.CategorizedResourceReadout, "CategorizedResourceReadoutToggleButton".Translate(), SoundDefOf.MouseoverToggle, null);
				if (resourceReadoutCategorized != flag3)
				{
					Prefs.ResourceReadoutCategorized = resourceReadoutCategorized;
				}
			}
			if (flag != this.showColonistBar)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}
	}
}
