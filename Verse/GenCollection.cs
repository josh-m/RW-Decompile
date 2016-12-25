using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public static class GenCollection
	{
		public static bool SharesElementWith<T>(this IEnumerable<T> source, IEnumerable<T> other)
		{
			return source.Any((T item) => other.Contains(item));
		}

		[DebuggerHidden]
		public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> source, IList<T> workingList = null)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (workingList == null)
			{
				workingList = source.ToList<T>();
			}
			else
			{
				workingList.Clear();
				foreach (T t in source)
				{
					workingList.Add(t);
				}
			}
			int countUnChosen = workingList.Count;
			int rand = 0;
			while (countUnChosen > 0)
			{
				rand = Rand.Range(0, countUnChosen);
				yield return workingList[rand];
				T temp = workingList[rand];
				workingList[rand] = workingList[countUnChosen - 1];
				workingList[countUnChosen - 1] = temp;
				countUnChosen--;
			}
		}

		public static T RandomElement<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as IList<T>;
			if (list == null)
			{
				list = source.ToList<T>();
			}
			if (list.Count == 0)
			{
				Log.Warning("Getting random element from empty collection.");
				return default(T);
			}
			return list[Rand.Range(0, list.Count)];
		}

		public static bool TryRandomElement<T>(this IEnumerable<T> source, out T result)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				if (list.Count == 0)
				{
					result = default(T);
					return false;
				}
			}
			else if (!source.Any<T>())
			{
				result = default(T);
				return false;
			}
			result = source.RandomElement<T>();
			return true;
		}

		public static T RandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
		{
			float num = 0f;
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					float num2 = weightSelector(list[i]);
					if (num2 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num2,
							" from ",
							list[i]
						}));
						num2 = 0f;
					}
					num += num2;
				}
			}
			else
			{
				foreach (T current in source)
				{
					float num3 = weightSelector(current);
					if (num3 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num3,
							" from ",
							current
						}));
						num3 = 0f;
					}
					num += num3;
				}
			}
			if (num <= 0f)
			{
				Log.Error("RandomElementByWeight with totalWeight=" + num + " - use TryRandomElementByWeight.");
				return default(T);
			}
			float num4 = Rand.Value * num;
			float num5 = 0f;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					float num6 = weightSelector(list[j]);
					if (num6 > 0f)
					{
						num5 += num6;
						if (num5 >= num4)
						{
							return list[j];
						}
					}
				}
			}
			else
			{
				foreach (T current2 in source)
				{
					float num7 = weightSelector(current2);
					if (num7 > 0f)
					{
						num5 += num7;
						if (num5 >= num4)
						{
							return current2;
						}
					}
				}
			}
			return default(T);
		}

		public static bool TryRandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector, out T result)
		{
			float num = 0f;
			IList<T> list = source as IList<T>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					float num2 = weightSelector(list[i]);
					if (num2 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num2,
							" from ",
							list[i]
						}));
						num2 = 0f;
					}
					num += num2;
				}
			}
			else
			{
				foreach (T current in source)
				{
					float num3 = weightSelector(current);
					if (num3 < 0f)
					{
						Log.Error(string.Concat(new object[]
						{
							"Negative weight in selector: ",
							num3,
							" from ",
							current
						}));
						num3 = 0f;
					}
					num += num3;
				}
			}
			if (num <= 0f)
			{
				result = default(T);
				return false;
			}
			float num4 = Rand.Value * num;
			float num5 = 0f;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					float num6 = weightSelector(list[j]);
					if (num6 > 0f)
					{
						num5 += num6;
						if (num5 >= num4)
						{
							result = list[j];
							return true;
						}
					}
				}
			}
			else
			{
				foreach (T current2 in source)
				{
					float num7 = weightSelector(current2);
					if (num7 > 0f)
					{
						num5 += num7;
						if (num5 >= num4)
						{
							result = current2;
							return true;
						}
					}
				}
			}
			result = default(T);
			return false;
		}

		public static T RandomElementByWeightWithDefault<T>(this IEnumerable<T> source, Func<T, float> weightSelector, float defaultValueWeight)
		{
			if (defaultValueWeight < 0f)
			{
				Log.Error("Negative default value weight.");
				defaultValueWeight = 0f;
			}
			float num = 0f;
			foreach (T current in source)
			{
				float num2 = weightSelector(current);
				if (num2 < 0f)
				{
					Log.Error(string.Concat(new object[]
					{
						"Negative weight in selector: ",
						num2,
						" from ",
						current
					}));
					num2 = 0f;
				}
				num += num2;
			}
			float num3 = defaultValueWeight + num;
			if (num3 <= 0f)
			{
				Log.Error("RandomElementByWeightWithDefault with totalWeight=" + num3);
				return default(T);
			}
			if (Rand.Value < defaultValueWeight / num3 || num == 0f)
			{
				return default(T);
			}
			return source.RandomElementByWeight(weightSelector);
		}

		public static T RandomEnumValue<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>().RandomElement<T>();
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MaxBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			TSource result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				TSource tSource = enumerator.Current;
				TKey y = selector(tSource);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey tKey = selector(current);
					if (comparer.Compare(tKey, y) > 0)
					{
						tSource = current;
						y = tKey;
					}
				}
				result = tSource;
			}
			return result;
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
		{
			return source.MinBy(selector, Comparer<TKey>.Default);
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			TSource result;
			using (IEnumerator<TSource> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				TSource tSource = enumerator.Current;
				TKey y = selector(tSource);
				while (enumerator.MoveNext())
				{
					TSource current = enumerator.Current;
					TKey tKey = selector(current);
					if (comparer.Compare(tKey, y) < 0)
					{
						tSource = current;
						y = tKey;
					}
				}
				result = tSource;
			}
			return result;
		}

		public static void SortBy<T, TSortBy>(this List<T> list, Func<T, TSortBy> selector) where TSortBy : IComparable<TSortBy>
		{
			list.Sort(delegate(T a, T b)
			{
				TSortBy tSortBy = selector(a);
				return tSortBy.CompareTo(selector(b));
			});
		}

		public static void SortBy<T, TSortBy, TThenBy>(this List<T> list, Func<T, TSortBy> selector, Func<T, TThenBy> thenBySelector) where TSortBy : IComparable<TSortBy>, IEquatable<TSortBy> where TThenBy : IComparable<TThenBy>
		{
			list.Sort(delegate(T a, T b)
			{
				TSortBy tSortBy = selector(a);
				TSortBy other = selector(b);
				if (!tSortBy.Equals(other))
				{
					return tSortBy.CompareTo(other);
				}
				TThenBy tThenBy = thenBySelector(a);
				return tThenBy.CompareTo(thenBySelector(b));
			});
		}

		public static void SortByDescending<T, TSortByDescending>(this List<T> list, Func<T, TSortByDescending> selector) where TSortByDescending : IComparable<TSortByDescending>
		{
			list.Sort(delegate(T a, T b)
			{
				TSortByDescending tSortByDescending = selector(b);
				return tSortByDescending.CompareTo(selector(a));
			});
		}

		public static void SortByDescending<T, TSortByDescending, TThenByDescending>(this List<T> list, Func<T, TSortByDescending> selector, Func<T, TThenByDescending> thenByDescendingSelector) where TSortByDescending : IComparable<TSortByDescending>, IEquatable<TSortByDescending> where TThenByDescending : IComparable<TThenByDescending>
		{
			list.Sort(delegate(T a, T b)
			{
				TSortByDescending other = selector(a);
				TSortByDescending other2 = selector(b);
				if (!other.Equals(other2))
				{
					return other2.CompareTo(other);
				}
				TThenByDescending tThenByDescending = thenByDescendingSelector(b);
				return tThenByDescending.CompareTo(thenByDescendingSelector(a));
			});
		}

		public static int RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
		{
			List<TKey> list = null;
			int result;
			try
			{
				foreach (KeyValuePair<TKey, TValue> current in dictionary)
				{
					if (predicate(current))
					{
						if (list == null)
						{
							list = SimplePool<List<TKey>>.Get();
						}
						list.Add(current.Key);
					}
				}
				if (list != null)
				{
					int i = 0;
					int count = list.Count;
					while (i < count)
					{
						dictionary.Remove(list[i]);
						i++;
					}
					result = list.Count;
				}
				else
				{
					result = 0;
				}
			}
			finally
			{
				if (list != null)
				{
					list.Clear();
					SimplePool<List<TKey>>.Return(list);
				}
			}
			return result;
		}

		public static void RemoveLast<T>(this List<T> list)
		{
			list.RemoveAt(list.Count - 1);
		}

		public static bool Any<T>(this List<T> list, Predicate<T> predicate)
		{
			return list.FindIndex(predicate) != -1;
		}

		public static bool Any<T>(this List<T> list)
		{
			return list.Count != 0;
		}
	}
}
