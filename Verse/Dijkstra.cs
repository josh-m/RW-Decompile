using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class Dijkstra<T>
	{
		private class DistanceComparer : IComparer<KeyValuePair<T, float>>
		{
			public int Compare(KeyValuePair<T, float> a, KeyValuePair<T, float> b)
			{
				return a.Value.CompareTo(b.Value);
			}
		}

		private static Dictionary<T, float> distances = new Dictionary<T, float>();

		private static FastPriorityQueue<KeyValuePair<T, float>> queue = new FastPriorityQueue<KeyValuePair<T, float>>(new Dijkstra<T>.DistanceComparer());

		private static List<T> singleNodeList = new List<T>();

		private static List<KeyValuePair<T, float>> tmpResult = new List<KeyValuePair<T, float>>();

		public static void Run(T startingNode, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, ref List<KeyValuePair<T, float>> outDistances)
		{
			Dijkstra<T>.singleNodeList.Clear();
			Dijkstra<T>.singleNodeList.Add(startingNode);
			Dijkstra<T>.Run(Dijkstra<T>.singleNodeList, neighborsGetter, distanceGetter, ref outDistances);
		}

		public static void Run(IEnumerable<T> startingNodes, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, ref List<KeyValuePair<T, float>> outDistances)
		{
			outDistances.Clear();
			Dijkstra<T>.distances.Clear();
			Dijkstra<T>.queue.Clear();
			IList<T> list = startingNodes as IList<T>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					T key = list[i];
					if (!Dijkstra<T>.distances.ContainsKey(key))
					{
						Dijkstra<T>.distances.Add(key, 0f);
						Dijkstra<T>.queue.Push(new KeyValuePair<T, float>(key, 0f));
					}
				}
			}
			else
			{
				foreach (T current in startingNodes)
				{
					if (!Dijkstra<T>.distances.ContainsKey(current))
					{
						Dijkstra<T>.distances.Add(current, 0f);
						Dijkstra<T>.queue.Push(new KeyValuePair<T, float>(current, 0f));
					}
				}
			}
			while (Dijkstra<T>.queue.Count != 0)
			{
				KeyValuePair<T, float> node = Dijkstra<T>.queue.Pop();
				float num = Dijkstra<T>.distances[node.Key];
				if (node.Value == num)
				{
					IEnumerable<T> enumerable = neighborsGetter(node.Key);
					if (enumerable != null)
					{
						IList<T> list2 = enumerable as IList<T>;
						if (list2 != null)
						{
							for (int j = 0; j < list2.Count; j++)
							{
								Dijkstra<T>.HandleNeighbor(list2[j], num, node, distanceGetter);
							}
						}
						else
						{
							foreach (T current2 in enumerable)
							{
								Dijkstra<T>.HandleNeighbor(current2, num, node, distanceGetter);
							}
						}
					}
				}
			}
			foreach (KeyValuePair<T, float> current3 in Dijkstra<T>.distances)
			{
				outDistances.Add(current3);
			}
			Dijkstra<T>.distances.Clear();
		}

		public static void Run(IEnumerable<T> startingNodes, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, ref Dictionary<T, float> outDistances)
		{
			Dijkstra<T>.Run(startingNodes, neighborsGetter, distanceGetter, ref Dijkstra<T>.tmpResult);
			outDistances.Clear();
			for (int i = 0; i < Dijkstra<T>.tmpResult.Count; i++)
			{
				outDistances.Add(Dijkstra<T>.tmpResult[i].Key, Dijkstra<T>.tmpResult[i].Value);
			}
			Dijkstra<T>.tmpResult.Clear();
		}

		private static void HandleNeighbor(T n, float nodeDist, KeyValuePair<T, float> node, Func<T, T, float> distanceGetter)
		{
			float num = nodeDist + Mathf.Max(distanceGetter(node.Key, n), 0f);
			float num2;
			if (Dijkstra<T>.distances.TryGetValue(n, out num2))
			{
				if (num < num2)
				{
					Dijkstra<T>.distances[n] = num;
					Dijkstra<T>.queue.Push(new KeyValuePair<T, float>(n, num));
				}
			}
			else
			{
				Dijkstra<T>.distances.Add(n, num);
				Dijkstra<T>.queue.Push(new KeyValuePair<T, float>(n, num));
			}
		}
	}
}
