using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class LoadedModManager
	{
		private static List<ModContentPack> runningMods = new List<ModContentPack>();

		public static IEnumerable<ModContentPack> RunningMods
		{
			get
			{
				return LoadedModManager.runningMods;
			}
		}

		public static void LoadAllActiveMods()
		{
			XmlInheritance.Clear();
			int num = 0;
			foreach (ModMetaData current in ModsConfig.ActiveModsInLoadOrder.ToList<ModMetaData>())
			{
				DeepProfiler.Start("Loading " + current);
				if (!current.RootDir.Exists)
				{
					ModsConfig.SetActive(current.Identifier, false);
					Log.Warning(string.Concat(new object[]
					{
						"Failed to find active mod ",
						current.Name,
						"(",
						current.Identifier,
						") at ",
						current.RootDir
					}));
					DeepProfiler.End();
				}
				else
				{
					ModContentPack modContentPack = new ModContentPack(current.RootDir, num, current.Name);
					num++;
					LoadedModManager.runningMods.Add(modContentPack);
					modContentPack.ReloadAllContent();
					DeepProfiler.End();
				}
			}
			XmlInheritance.Clear();
		}

		public static void ClearDestroy()
		{
			foreach (ModContentPack current in LoadedModManager.runningMods)
			{
				current.ClearDestroy();
			}
			LoadedModManager.runningMods.Clear();
		}
	}
}
