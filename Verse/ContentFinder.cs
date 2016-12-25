using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public static class ContentFinder<T> where T : class
	{
		public static T Get(string itemPath, bool reportFailure = true)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get a resource \"" + itemPath + "\" from a different thread. All resources must be loaded in the main thread.");
				return (T)((object)null);
			}
			T t = (T)((object)null);
			foreach (ModContentPack current in LoadedModManager.RunningMods)
			{
				t = current.GetContentHolder<T>().Get(itemPath);
				if (t != null)
				{
					T result = t;
					return result;
				}
			}
			if (typeof(T) == typeof(Texture2D))
			{
				t = (T)((object)Resources.Load<Texture2D>(GenFilePaths.ContentPath<Texture2D>() + itemPath));
			}
			if (typeof(T) == typeof(AudioClip))
			{
				t = (T)((object)Resources.Load<AudioClip>(GenFilePaths.ContentPath<AudioClip>() + itemPath));
			}
			if (t != null)
			{
				return t;
			}
			if (reportFailure)
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not load ",
					typeof(T),
					" at ",
					itemPath,
					" in any active mod or in base resources."
				}));
			}
			return (T)((object)null);
		}

		[DebuggerHidden]
		public static IEnumerable<T> GetAllInFolder(string folderPath)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get all resources in a folder \"" + folderPath + "\" from a different thread. All resources must be loaded in the main thread.");
			}
			else
			{
				foreach (ModContentPack mod in LoadedModManager.RunningMods)
				{
					foreach (T item in mod.GetContentHolder<T>().GetAllUnderPath(folderPath))
					{
						yield return item;
					}
				}
				T[] items = null;
				if (typeof(T) == typeof(Texture2D))
				{
					items = (T[])Resources.LoadAll<Texture2D>(GenFilePaths.ContentPath<Texture2D>() + folderPath);
				}
				if (typeof(T) == typeof(AudioClip))
				{
					items = (T[])Resources.LoadAll<AudioClip>(GenFilePaths.ContentPath<AudioClip>() + folderPath);
				}
				if (items != null)
				{
					T[] array = items;
					for (int i = 0; i < array.Length; i++)
					{
						T item2 = array[i];
						yield return item2;
					}
				}
			}
		}
	}
}
