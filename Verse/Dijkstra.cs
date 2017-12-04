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

		public static void Run(T startingNode, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, List<KeyValuePair<T, float>> outDistances, Dictionary<T, T> outParents = null)
		{
			Dijkstra<T>.singleNodeList.Clear();
			Dijkstra<T>.singleNodeList.Add(startingNode);
			Dijkstra<T>.Run(Dijkstra<T>.singleNodeList, neighborsGetter, distanceGetter, outDistances, outParents);
		}

		public static void Run(IEnumerable<T> startingNodes, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, List<KeyValuePair<T, float>> outDistances, Dictionary<T, T> outParents = null)
		{
			outDistances.Clear();
			Dijkstra<T>.distances.Clear();
			Dijkstra<T>.queue.Clear();
			if (outParents != null)
			{
				outParents.Clear();
			}
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
								Dijkstra<T>.HandleNeighbor(list2[j], num, node, distanceGetter, outParents);
							}
						}
						else
						{
							foreach (T current2 in enumerable)
							{
								Dijkstra<T>.HandleNeighbor(current2, num, node, distanceGetter, outParents);
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

		public static void Run(T startingNode, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, Dictionary<T, float> outDistances, Dictionary<T, T> outParents = null)
		{
			Dijkstra<T>.singleNodeList.Clear();
			Dijkstra<T>.singleNodeList.Add(startingNode);
			Dijkstra<T>.Run(Dijkstra<T>.singleNodeList, neighborsGetter, distanceGetter, outDistances, outParents);
		}

		public static void Run(IEnumerable<T> startingNodes, Func<T, IEnumerable<T>> neighborsGetter, Func<T, T, float> distanceGetter, Dictionary<T, float> outDistances, Dictionary<T, T> outParents = null)
		{
			Dijkstra<T>.Run(startingNodes, neighborsGetter, distanceGetter, Dijkstra<T>.tmpResult, outParents);
			outDistances.Clear();
			for (int i = 0; i < Dijkstra<T>.tmpResult.Count; i++)
			{
				outDistances.Add(Dijkstra<T>.tmpResult[i].Key, Dijkstra<T>.tmpResult[i].Value);
			}
			Dijkstra<T>.tmpResult.Clear();
		}

		private static void HandleNeighbor(T n, float nodeDist, KeyValuePair<T, float> node, Func<T, T, float> distanceGetter, Dictionary<T, T> outParents)
		{
			float num = nodeDist + Mathf.Max(distanceGetter(node.Key, n), 0f);
			bool flag = false;
			float num2;
			if (Dijkstra<T>.distances.TryGetValue(n, out num2))
			{
				if (num < num2)
				{
					Dijkstra<T>.distances[n] = num;
					flag = true;
				}
			}
			else
			{
				Dijkstra<T>.distances.Add(n, num);
				flag = true;
			}
			if (flag)
			{
				Dijkstra<T>.queue.Push(new KeyValuePair<T, float>(n, num));
				if (outParents != null)
				{
					if (outParents.ContainsKey(n))
					{
						outParents[n] = node.Key;
					}
					else
					{
						outParents.Add(n, node.Key);
					}
				}
			}
		}
	}
}
