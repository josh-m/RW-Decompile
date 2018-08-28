using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public static class GraphicDatabaseUtility
	{
		[DebuggerHidden]
		public static IEnumerable<string> GraphicNamesInFolder(string folderPath)
		{
			HashSet<string> loadedAssetNames = new HashSet<string>();
			Texture2D[] array = Resources.LoadAll<Texture2D>("Textures/" + folderPath);
			for (int i = 0; i < array.Length; i++)
			{
				Texture2D tex = array[i];
				string origAssetName = tex.name;
				string[] pieces = origAssetName.Split(new char[]
				{
					'_'
				});
				string assetName = string.Empty;
				if (pieces.Length <= 2)
				{
					assetName = pieces[0];
				}
				else if (pieces.Length == 3)
				{
					assetName = pieces[0] + "_" + pieces[1];
				}
				else if (pieces.Length == 4)
				{
					assetName = string.Concat(new string[]
					{
						pieces[0],
						"_",
						pieces[1],
						"_",
						pieces[2]
					});
				}
				else
				{
					Log.Error("Cannot load assets with >3 pieces.", false);
				}
				if (!loadedAssetNames.Contains(assetName))
				{
					loadedAssetNames.Add(assetName);
					yield return assetName;
				}
			}
		}
	}
}
