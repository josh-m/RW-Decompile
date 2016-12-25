using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class PlanetShapeGenerator
	{
		private const int MaxTileVertices = 6;

		private static int subdivisionsCount;

		private static float radius;

		private static Vector3 viewCenter;

		private static float viewAngle;

		private static List<TriangleIndices> tris = new List<TriangleIndices>();

		private static List<Vector3> verts = new List<Vector3>();

		private static List<Vector3> finalVerts;

		private static List<int> tileIDToFinalVerts_offsets;

		private static List<int> tileIDToNeighbors_offsets;

		private static List<int> tileIDToNeighbors_values;

		private static List<TriangleIndices> newTris = new List<TriangleIndices>();

		private static List<int> generatedTileVerts = new List<int>();

		private static List<int> adjacentTris = new List<int>();

		private static List<int> tmpTileIDs = new List<int>();

		private static List<int> tmpVerts = new List<int>();

		private static List<int> tmpNeighborsToAdd = new List<int>();

		private static List<int> vertToTris_offsets = new List<int>();

		private static List<int> vertToTris_values = new List<int>();

		private static List<int> vertToTileIDs_offsets = new List<int>();

		private static List<int> vertToTileIDs_values = new List<int>();

		private static List<int> tileIDToVerts_offsets = new List<int>();

		private static List<int> tileIDToVerts_values = new List<int>();

		public static void Generate(int subdivisionsCount, out List<Vector3> outVerts, out List<int> outTileIDToVerts_offsets, out List<int> outTileIDToNeighbors_offsets, out List<int> outTileIDToNeighbors_values, float radius, Vector3 viewCenter, float viewAngle)
		{
			PlanetShapeGenerator.subdivisionsCount = subdivisionsCount;
			PlanetShapeGenerator.radius = radius;
			PlanetShapeGenerator.viewCenter = viewCenter;
			PlanetShapeGenerator.viewAngle = viewAngle;
			PlanetShapeGenerator.DoGenerate();
			outVerts = PlanetShapeGenerator.finalVerts;
			outTileIDToVerts_offsets = PlanetShapeGenerator.tileIDToFinalVerts_offsets;
			outTileIDToNeighbors_offsets = PlanetShapeGenerator.tileIDToNeighbors_offsets;
			outTileIDToNeighbors_values = PlanetShapeGenerator.tileIDToNeighbors_values;
		}

		private static void DoGenerate()
		{
			PlanetShapeGenerator.ClearOrCreateMeshStaticData();
			PlanetShapeGenerator.CreateTileInfoStaticData();
			IcosahedronGenerator.GenerateIcosahedron(PlanetShapeGenerator.verts, PlanetShapeGenerator.tris, PlanetShapeGenerator.radius, PlanetShapeGenerator.viewCenter, PlanetShapeGenerator.viewAngle);
			for (int i = 0; i < PlanetShapeGenerator.subdivisionsCount + 1; i++)
			{
				bool lastPass = i == PlanetShapeGenerator.subdivisionsCount;
				PlanetShapeGenerator.Subdivide(lastPass);
			}
			PlanetShapeGenerator.CalculateTileNeighbors();
			PlanetShapeGenerator.ClearAndDeallocateWorkingLists();
		}

		private static void ClearOrCreateMeshStaticData()
		{
			PlanetShapeGenerator.tris.Clear();
			PlanetShapeGenerator.verts.Clear();
			PlanetShapeGenerator.finalVerts = new List<Vector3>();
		}

		private static void CreateTileInfoStaticData()
		{
			PlanetShapeGenerator.tileIDToFinalVerts_offsets = new List<int>();
			PlanetShapeGenerator.tileIDToNeighbors_offsets = new List<int>();
			PlanetShapeGenerator.tileIDToNeighbors_values = new List<int>();
		}

		private static void ClearAndDeallocateWorkingLists()
		{
			PlanetShapeGenerator.ClearAndDeallocate<TriangleIndices>(ref PlanetShapeGenerator.tris);
			PlanetShapeGenerator.ClearAndDeallocate<Vector3>(ref PlanetShapeGenerator.verts);
			PlanetShapeGenerator.ClearAndDeallocate<TriangleIndices>(ref PlanetShapeGenerator.newTris);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.generatedTileVerts);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.adjacentTris);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.tmpTileIDs);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.tmpVerts);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.tmpNeighborsToAdd);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.vertToTris_offsets);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.vertToTris_values);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.vertToTileIDs_offsets);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.vertToTileIDs_values);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.tileIDToVerts_offsets);
			PlanetShapeGenerator.ClearAndDeallocate<int>(ref PlanetShapeGenerator.tileIDToVerts_values);
		}

		private static void ClearAndDeallocate<T>(ref List<T> list)
		{
			list.Clear();
			list.TrimExcess();
			list = new List<T>();
		}

		private static void Subdivide(bool lastPass)
		{
			PackedListOfLists.GenerateVertToTrisPackedList(PlanetShapeGenerator.verts, PlanetShapeGenerator.tris, PlanetShapeGenerator.vertToTris_offsets, PlanetShapeGenerator.vertToTris_values);
			int count = PlanetShapeGenerator.verts.Count;
			int i = 0;
			int count2 = PlanetShapeGenerator.tris.Count;
			while (i < count2)
			{
				TriangleIndices triangleIndices = PlanetShapeGenerator.tris[i];
				Vector3 vector = (PlanetShapeGenerator.verts[triangleIndices.v1] + PlanetShapeGenerator.verts[triangleIndices.v2] + PlanetShapeGenerator.verts[triangleIndices.v3]) / 3f;
				PlanetShapeGenerator.verts.Add(vector.normalized * PlanetShapeGenerator.radius);
				i++;
			}
			PlanetShapeGenerator.newTris.Clear();
			if (lastPass)
			{
				PlanetShapeGenerator.vertToTileIDs_offsets.Clear();
				PlanetShapeGenerator.vertToTileIDs_values.Clear();
				PlanetShapeGenerator.tileIDToVerts_offsets.Clear();
				PlanetShapeGenerator.tileIDToVerts_values.Clear();
				int j = 0;
				int count3 = PlanetShapeGenerator.verts.Count;
				while (j < count3)
				{
					PlanetShapeGenerator.vertToTileIDs_offsets.Add(PlanetShapeGenerator.vertToTileIDs_values.Count);
					if (j >= count)
					{
						for (int k = 0; k < 6; k++)
						{
							PlanetShapeGenerator.vertToTileIDs_values.Add(-1);
						}
					}
					j++;
				}
			}
			for (int l = 0; l < count; l++)
			{
				PackedListOfLists.GetList<int>(PlanetShapeGenerator.vertToTris_offsets, PlanetShapeGenerator.vertToTris_values, l, PlanetShapeGenerator.adjacentTris);
				int count4 = PlanetShapeGenerator.adjacentTris.Count;
				if (!lastPass)
				{
					for (int m = 0; m < count4; m++)
					{
						int num = PlanetShapeGenerator.adjacentTris[m];
						int v = count + num;
						int nextOrderedVertex = PlanetShapeGenerator.tris[num].GetNextOrderedVertex(l);
						int num2 = -1;
						for (int n = 0; n < count4; n++)
						{
							if (m != n)
							{
								TriangleIndices triangleIndices2 = PlanetShapeGenerator.tris[PlanetShapeGenerator.adjacentTris[n]];
								if (triangleIndices2.v1 == nextOrderedVertex || triangleIndices2.v2 == nextOrderedVertex || triangleIndices2.v3 == nextOrderedVertex)
								{
									num2 = PlanetShapeGenerator.adjacentTris[n];
									break;
								}
							}
						}
						if (num2 >= 0)
						{
							int v2 = count + num2;
							PlanetShapeGenerator.newTris.Add(new TriangleIndices(l, v2, v));
						}
					}
				}
				else if (count4 == 5 || count4 == 6)
				{
					int num3 = 0;
					int nextOrderedVertex2 = PlanetShapeGenerator.tris[PlanetShapeGenerator.adjacentTris[num3]].GetNextOrderedVertex(l);
					int num4 = num3;
					int currentTriangleVertex = nextOrderedVertex2;
					PlanetShapeGenerator.generatedTileVerts.Clear();
					for (int num5 = 0; num5 < count4; num5++)
					{
						int item = count + PlanetShapeGenerator.adjacentTris[num4];
						PlanetShapeGenerator.generatedTileVerts.Add(item);
						int nextAdjancetTriangle = PlanetShapeGenerator.GetNextAdjancetTriangle(num4, currentTriangleVertex, PlanetShapeGenerator.adjacentTris);
						int nextOrderedVertex3 = PlanetShapeGenerator.tris[PlanetShapeGenerator.adjacentTris[nextAdjancetTriangle]].GetNextOrderedVertex(l);
						num4 = nextAdjancetTriangle;
						currentTriangleVertex = nextOrderedVertex3;
					}
					PlanetShapeGenerator.FinalizeGeneratedTile(PlanetShapeGenerator.generatedTileVerts);
				}
			}
			PlanetShapeGenerator.tris.Clear();
			PlanetShapeGenerator.tris.AddRange(PlanetShapeGenerator.newTris);
		}

		private static void FinalizeGeneratedTile(List<int> generatedTileVerts)
		{
			if ((generatedTileVerts.Count != 5 && generatedTileVerts.Count != 6) || generatedTileVerts.Count > 6)
			{
				Log.Error("Planet shape generation internal error: generated a tile with " + generatedTileVerts.Count + " vertices. Only 5 and 6 are allowed.");
				return;
			}
			if (PlanetShapeGenerator.ShouldDiscardGeneratedTile(generatedTileVerts))
			{
				return;
			}
			int count = PlanetShapeGenerator.tileIDToFinalVerts_offsets.Count;
			PlanetShapeGenerator.tileIDToFinalVerts_offsets.Add(PlanetShapeGenerator.finalVerts.Count);
			int i = 0;
			int count2 = generatedTileVerts.Count;
			while (i < count2)
			{
				int index = generatedTileVerts[i];
				PlanetShapeGenerator.finalVerts.Add(PlanetShapeGenerator.verts[index]);
				PlanetShapeGenerator.vertToTileIDs_values[PlanetShapeGenerator.vertToTileIDs_values.IndexOf(-1, PlanetShapeGenerator.vertToTileIDs_offsets[index])] = count;
				i++;
			}
			PackedListOfLists.AddList<int>(PlanetShapeGenerator.tileIDToVerts_offsets, PlanetShapeGenerator.tileIDToVerts_values, generatedTileVerts);
		}

		private static bool ShouldDiscardGeneratedTile(List<int> generatedTileVerts)
		{
			Vector3 a = Vector3.zero;
			int i = 0;
			int count = generatedTileVerts.Count;
			while (i < count)
			{
				a += PlanetShapeGenerator.verts[generatedTileVerts[i]];
				i++;
			}
			return !MeshUtility.Visible(a / (float)generatedTileVerts.Count, PlanetShapeGenerator.radius, PlanetShapeGenerator.viewCenter, PlanetShapeGenerator.viewAngle);
		}

		private static void CalculateTileNeighbors()
		{
			int i = 0;
			int count = PlanetShapeGenerator.tileIDToVerts_offsets.Count;
			while (i < count)
			{
				PlanetShapeGenerator.tmpNeighborsToAdd.Clear();
				PackedListOfLists.GetList<int>(PlanetShapeGenerator.tileIDToVerts_offsets, PlanetShapeGenerator.tileIDToVerts_values, i, PlanetShapeGenerator.tmpVerts);
				int j = 0;
				int count2 = PlanetShapeGenerator.tmpVerts.Count;
				while (j < count2)
				{
					PackedListOfLists.GetList<int>(PlanetShapeGenerator.vertToTileIDs_offsets, PlanetShapeGenerator.vertToTileIDs_values, PlanetShapeGenerator.tmpVerts[j], PlanetShapeGenerator.tmpTileIDs);
					int k = 0;
					int count3 = PlanetShapeGenerator.tmpTileIDs.Count;
					while (k < count3)
					{
						int num = PlanetShapeGenerator.tmpTileIDs[k];
						if (num != i)
						{
							if (num != -1)
							{
								if (!PlanetShapeGenerator.tmpNeighborsToAdd.Contains(num))
								{
									PlanetShapeGenerator.tmpNeighborsToAdd.Add(num);
								}
							}
						}
						k++;
					}
					j++;
				}
				PackedListOfLists.AddList<int>(PlanetShapeGenerator.tileIDToNeighbors_offsets, PlanetShapeGenerator.tileIDToNeighbors_values, PlanetShapeGenerator.tmpNeighborsToAdd);
				i++;
			}
		}

		private static int GetNextAdjancetTriangle(int currentAdjTriangleIndex, int currentTriangleVertex, List<int> adjacentTris)
		{
			int i = 0;
			int count = adjacentTris.Count;
			while (i < count)
			{
				if (currentAdjTriangleIndex != i)
				{
					TriangleIndices triangleIndices = PlanetShapeGenerator.tris[adjacentTris[i]];
					if (triangleIndices.v1 == currentTriangleVertex || triangleIndices.v2 == currentTriangleVertex || triangleIndices.v3 == currentTriangleVertex)
					{
						return i;
					}
				}
				i++;
			}
			Log.Error("Planet shape generation internal error: could not find next adjacent triangle.");
			return -1;
		}
	}
}
