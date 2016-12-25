using System;
using System.Collections.Generic;
using UnityEngine;

namespace RimWorld.Planet
{
	public static class PackedListOfLists
	{
		private static List<int> vertAdjacentTrisCount = new List<int>();

		public static void AddList<T>(List<int> offsets, List<T> values, List<T> listToAdd)
		{
			offsets.Add(values.Count);
			values.AddRange(listToAdd);
		}

		public static void GetList<T>(List<int> offsets, List<T> values, int listIndex, List<T> outList)
		{
			outList.Clear();
			int num = values.Count;
			if (listIndex + 1 < offsets.Count)
			{
				num = offsets[listIndex + 1];
			}
			for (int num2 = offsets[listIndex]; num2 != num; num2++)
			{
				outList.Add(values[num2]);
			}
		}

		public static void GenerateVertToTrisPackedList(List<Vector3> verts, List<TriangleIndices> tris, List<int> outOffsets, List<int> outValues)
		{
			outOffsets.Clear();
			outValues.Clear();
			PackedListOfLists.vertAdjacentTrisCount.Clear();
			int i = 0;
			int count = verts.Count;
			while (i < count)
			{
				PackedListOfLists.vertAdjacentTrisCount.Add(0);
				i++;
			}
			int j = 0;
			int count2 = tris.Count;
			while (j < count2)
			{
				TriangleIndices triangleIndices = tris[j];
				List<int> list;
				List<int> expr_56 = list = PackedListOfLists.vertAdjacentTrisCount;
				int num;
				int expr_60 = num = triangleIndices.v1;
				num = list[num];
				expr_56[expr_60] = num + 1;
				List<int> list2;
				List<int> expr_7C = list2 = PackedListOfLists.vertAdjacentTrisCount;
				int expr_86 = num = triangleIndices.v2;
				num = list2[num];
				expr_7C[expr_86] = num + 1;
				List<int> list3;
				List<int> expr_A2 = list3 = PackedListOfLists.vertAdjacentTrisCount;
				int expr_AC = num = triangleIndices.v3;
				num = list3[num];
				expr_A2[expr_AC] = num + 1;
				j++;
			}
			int num2 = 0;
			int k = 0;
			int count3 = verts.Count;
			while (k < count3)
			{
				outOffsets.Add(num2);
				int num3 = PackedListOfLists.vertAdjacentTrisCount[k];
				PackedListOfLists.vertAdjacentTrisCount[k] = 0;
				for (int l = 0; l < num3; l++)
				{
					outValues.Add(-1);
				}
				num2 += num3;
				k++;
			}
			int m = 0;
			int count4 = tris.Count;
			while (m < count4)
			{
				TriangleIndices triangleIndices2 = tris[m];
				outValues[outOffsets[triangleIndices2.v1] + PackedListOfLists.vertAdjacentTrisCount[triangleIndices2.v1]] = m;
				outValues[outOffsets[triangleIndices2.v2] + PackedListOfLists.vertAdjacentTrisCount[triangleIndices2.v2]] = m;
				outValues[outOffsets[triangleIndices2.v3] + PackedListOfLists.vertAdjacentTrisCount[triangleIndices2.v3]] = m;
				List<int> list4;
				List<int> expr_1CC = list4 = PackedListOfLists.vertAdjacentTrisCount;
				int num;
				int expr_1D6 = num = triangleIndices2.v1;
				num = list4[num];
				expr_1CC[expr_1D6] = num + 1;
				List<int> list5;
				List<int> expr_1F2 = list5 = PackedListOfLists.vertAdjacentTrisCount;
				int expr_1FC = num = triangleIndices2.v2;
				num = list5[num];
				expr_1F2[expr_1FC] = num + 1;
				List<int> list6;
				List<int> expr_218 = list6 = PackedListOfLists.vertAdjacentTrisCount;
				int expr_222 = num = triangleIndices2.v3;
				num = list6[num];
				expr_218[expr_222] = num + 1;
				m++;
			}
		}
	}
}
