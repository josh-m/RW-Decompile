using System;
using System.Collections.Generic;

namespace Verse
{
	public static class SimplePool<T> where T : new()
	{
		private static List<T> freeItems = new List<T>();

		public static T Get()
		{
			if (SimplePool<T>.freeItems.Count == 0)
			{
				return (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
			}
			T result = SimplePool<T>.freeItems[SimplePool<T>.freeItems.Count - 1];
			SimplePool<T>.freeItems.RemoveAt(SimplePool<T>.freeItems.Count - 1);
			return result;
		}

		public static void Return(T item)
		{
			SimplePool<T>.freeItems.Add(item);
		}
	}
}
