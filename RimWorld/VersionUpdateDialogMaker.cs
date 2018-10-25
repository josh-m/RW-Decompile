using System;
using Verse;

namespace RimWorld
{
	public static class VersionUpdateDialogMaker
	{
		private static bool dialogDone;

		public static void CreateVersionUpdateDialogIfNecessary()
		{
			if (!VersionUpdateDialogMaker.dialogDone && LastPlayedVersion.Version != null && (VersionControl.CurrentMajor != LastPlayedVersion.Version.Major || VersionControl.CurrentMinor != LastPlayedVersion.Version.Minor))
			{
				VersionUpdateDialogMaker.CreateNewVersionDialog();
			}
		}

		private static void CreateNewVersionDialog()
		{
			string value = LastPlayedVersion.Version.Major + "." + LastPlayedVersion.Version.Minor;
			string value2 = VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor;
			string text = "GameUpdatedToNewVersionInitial".Translate(value, value2);
			text += "\n\n";
			if (BackCompatibility.IsSaveCompatibleWith(LastPlayedVersion.Version.ToString()))
			{
				text += "GameUpdatedToNewVersionSavesCompatible".Translate();
			}
			else
			{
				text += "GameUpdatedToNewVersionSavesIncompatible".Translate();
			}
			text += "\n\n";
			text += "GameUpdatedToNewVersionSteam".Translate();
			Find.WindowStack.Add(new Dialog_MessageBox(text, null, null, null, null, null, false, null, null));
			VersionUpdateDialogMaker.dialogDone = true;
		}
	}
}
