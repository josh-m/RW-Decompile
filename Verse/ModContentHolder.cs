using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public class ModContentHolder<T> where T : class
	{
		private ModContentPack mod;

		public Dictionary<string, T> contentList = new Dictionary<string, T>();

		public ModContentHolder(ModContentPack mod)
		{
			this.mod = mod;
		}

		public void ClearDestroy()
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
			{
				foreach (T current in this.contentList.Values)
				{
					T localObj = current;
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						UnityEngine.Object.Destroy((UnityEngine.Object)((object)localObj));
					});
				}
			}
			this.contentList.Clear();
		}

		public void ReloadAll()
		{
			foreach (LoadedContentItem<T> current in ModContentLoader<T>.LoadAllForMod(this.mod))
			{
				if (this.contentList.ContainsKey(current.internalPath))
				{
					Log.Warning(string.Concat(new object[]
					{
						"Tried to load duplicate ",
						typeof(T),
						" with path: ",
						current.internalPath
					}));
				}
				else
				{
					this.contentList.Add(current.internalPath, current.contentItem);
				}
			}
		}

		public T Get(string path)
		{
			T result;
			if (this.contentList.TryGetValue(path, out result))
			{
				return result;
			}
			return (T)((object)null);
		}

		[DebuggerHidden]
		public IEnumerable<T> GetAllUnderPath(string pathRoot)
		{
			foreach (KeyValuePair<string, T> kvp in this.contentList)
			{
				if (kvp.Key.StartsWith(pathRoot))
				{
					yield return kvp.Value;
				}
			}
		}
	}
}
