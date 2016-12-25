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
			string text = LastPlayedVersion.Version.Major + "." + LastPlayedVersion.Version.Minor;
			string text2 = VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor;
			string text3 = "GameUpdatedToNewVersionInitial".Translate(new object[]
			{
				text,
				text2
			});
			text3 += "\n\n";
			text3 += "GameUpdatedToNewVersionSteam".Translate();
			Find.WindowStack.Add(new Dialog_MessageBox(text3, null, null, null, null, null, false));
			VersionUpdateDialogMaker.dialogDone = true;
		}
	}
}
