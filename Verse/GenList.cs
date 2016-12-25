using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GenList
	{
		public static int CountAllowNull<T>(this IList<T> list)
		{
			return (list == null) ? 0 : list.Count;
		}

		public static bool NullOrEmpty<T>(this IList<T> list)
		{
			return list == null || list.Count == 0;
		}

		public static List<T> ListFullCopy<T>(this List<T> source)
		{
			List<T> list = new List<T>(source.Count);
			for (int i = 0; i < source.Count; i++)
			{
				list.Add(source[i]);
			}
			return list;
		}

		public static List<T> ListFullCopyOrNull<T>(this List<T> source)
		{
			if (source == null)
			{
				return null;
			}
			return source.ListFullCopy<T>();
		}

		public static void RemoveDuplicates<T>(this List<T> list) where T : class
		{
			if (list.Count <= 1)
			{
				return;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < i - 1; j++)
				{
					if (list[i] == list[j])
					{
						list.RemoveAt(i);
						i--;
						break;
					}
				}
			}
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int i = list.Count;
			while (i > 1)
			{
				i--;
				int index = Rand.RangeInclusive(0, i);
				T value = list[index];
				list[index] = list[i];
				list[i] = value;
			}
		}

		public static void InsertionSort<T>(this IList<T> list, Comparison<T> comparison)
		{
			int count = list.Count;
			for (int i = 1; i < count; i++)
			{
				T t = list[i];
				int num = i - 1;
				while (num >= 0 && comparison(list[num], t) > 0)
				{
					list[num + 1] = list[num];
					num--;
				}
				list[num + 1] = t;
			}
		}
	}
}
