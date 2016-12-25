using System;
using System.IO;
using System.Linq;

namespace Verse
{
	public static class SaveGameFilesUtility
	{
		public static bool IsAutoSave(string fileName)
		{
			return fileName.Length >= 8 && fileName.Substring(0, 8) == "Autosave";
		}

		public static bool SavedGameNamedExists(string fileName)
		{
			foreach (string current in from f in GenFilePaths.AllSavedGameFiles
			select Path.GetFileNameWithoutExtension(f.Name))
			{
				if (current == fileName)
				{
					return true;
				}
			}
			return false;
		}

		public static string UnusedDefaultFileName(string factionLabel)
		{
			string text = string.Empty;
			int num = 1;
			do
			{
				text = factionLabel + num.ToString();
				num++;
			}
			while (SaveGameFilesUtility.SavedGameNamedExists(text));
			return text;
		}
	}
}
