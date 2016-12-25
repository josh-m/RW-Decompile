using System;
using UnityEngine.SceneManagement;

namespace Verse
{
	public static class GenScene
	{
		public const string EntrySceneName = "Entry";

		public const string MapSceneName = "Map";

		public static bool InEntryScene
		{
			get
			{
				return SceneManager.GetActiveScene().name == "Entry";
			}
		}

		public static bool InMapScene
		{
			get
			{
				return SceneManager.GetActiveScene().name == "Map";
			}
		}
	}
}
