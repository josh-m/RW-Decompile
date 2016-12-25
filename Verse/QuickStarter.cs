using System;
using UnityEngine.SceneManagement;

namespace Verse
{
	internal static class QuickStarter
	{
		private static bool quickStarted;

		public static bool CheckQuickStart()
		{
			if (GenCommandLine.CommandLineArgPassed("quicktest") && !QuickStarter.quickStarted && GenScene.InEntryScene)
			{
				QuickStarter.quickStarted = true;
				SceneManager.LoadScene("Play");
				return true;
			}
			return false;
		}
	}
}
