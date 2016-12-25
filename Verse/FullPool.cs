using System;
using System.Collections.Generic;

namespace Verse
{
	public static class FullPool<T> where T : IFullPoolable, new()
	{
		private static List<T> freeItems = new List<T>();

		public static T Get()
		{
			if (FullPool<T>.freeItems.Count == 0)
			{
				return (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
			}
			T result = FullPool<T>.freeItems[FullPool<T>.freeItems.Count - 1];
			FullPool<T>.freeItems.RemoveAt(FullPool<T>.freeItems.Count - 1);
			return result;
		}

		public static void Return(T item)
		{
			item.Reset();
			FullPool<T>.freeItems.Add(item);
		}
	}
}
