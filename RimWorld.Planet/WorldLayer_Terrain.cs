using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Terrain : WorldLayer
	{
		private List<MeshCollider> meshCollidersInOrder = new List<MeshCollider>();

		private List<List<int>> triangleIndexToTileID = new List<List<int>>();

		private List<Vector3> elevationValues = new List<Vector3>();

		[DebuggerHidden]
		public override IEnumerable Regenerate()
		{
			foreach (object result in base.Regenerate())
			{
				yield return result;
			}
			World world = Find.World;
			WorldGrid grid = world.grid;
			int tilesCount = grid.TilesCount;
			List<Tile> tiles = grid.tiles;
			List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
			List<Vector3> verts = grid.verts;
			this.triangleIndexToTileID.Clear();
			foreach (object result2 in this.CalculateInterpolatedVerticesParams())
			{
				yield return result2;
			}
			int colorsAndUVsIndex = 0;
			for (int i = 0; i < tilesCount; i++)
			{
				Tile tile = tiles[i];
				BiomeDef biome = tile.biome;
				int j;
				LayerSubMesh subMesh = base.GetSubMesh(biome.DrawMaterial, out j);
				while (j >= this.triangleIndexToTileID.Count)
				{
					this.triangleIndexToTileID.Add(new List<int>());
				}
				int count = subMesh.verts.Count;
				int num = 0;
				int num2 = (i + 1 >= tileIDToVerts_offsets.Count) ? verts.Count : tileIDToVerts_offsets[i + 1];
				for (int k = tileIDToVerts_offsets[i]; k < num2; k++)
				{
					subMesh.verts.Add(verts[k]);
					subMesh.uvs.Add(this.elevationValues[colorsAndUVsIndex]);
					colorsAndUVsIndex++;
					if (k < num2 - 2)
					{
						subMesh.tris.Add(count + num + 2);
						subMesh.tris.Add(count + num + 1);
						subMesh.tris.Add(count);
						this.triangleIndexToTileID[j].Add(i);
					}
					num++;
				}
			}
			base.FinalizeMesh(MeshParts.All);
			foreach (object result3 in this.RegenerateMeshColliders())
			{
				yield return result3;
			}
			this.elevationValues.Clear();
			this.elevationValues.TrimExcess();
		}

		public int GetTileIDFromRayHit(RaycastHit hit)
		{
			int i = 0;
			int count = this.meshCollidersInOrder.Count;
			while (i < count)
			{
				if (this.meshCollidersInOrder[i] == hit.collider)
				{
					return this.triangleIndexToTileID[i][hit.triangleIndex];
				}
				i++;
			}
			return -1;
		}

		[DebuggerHidden]
		private IEnumerable RegenerateMeshColliders()
		{
			this.meshCollidersInOrder.Clear();
			GameObject gameObject = WorldTerrainColliderManager.GameObject;
			MeshCollider[] components = gameObject.GetComponents<MeshCollider>();
			for (int j = 0; j < components.Length; j++)
			{
				MeshCollider obj = components[j];
				UnityEngine.Object.Destroy(obj);
			}
			for (int i = 0; i < this.subMeshes.Count; i++)
			{
				MeshCollider comp = gameObject.AddComponent<MeshCollider>();
				comp.sharedMesh = this.subMeshes[i].mesh;
				this.meshCollidersInOrder.Add(comp);
				yield return null;
			}
		}

		[DebuggerHidden]
		private IEnumerable CalculateInterpolatedVerticesParams()
		{
			this.elevationValues.Clear();
			World world = Find.World;
			WorldGrid grid = world.grid;
			int tilesCount = grid.TilesCount;
			List<Vector3> verts = grid.verts;
			List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			List<Tile> tiles = grid.tiles;
			for (int i = 0; i < tilesCount; i++)
			{
				Tile tile = tiles[i];
				float elevation = tile.elevation;
				int oneAfterLastNeighbor = (i + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[i + 1];
				int oneAfterLastVert = (i + 1 >= tilesCount) ? verts.Count : tileIDToVerts_offsets[i + 1];
				for (int j = tileIDToVerts_offsets[i]; j < oneAfterLastVert; j++)
				{
					Vector3 item = default(Vector3);
					item.x = elevation;
					bool flag = false;
					for (int k = tileIDToNeighbors_offsets[i]; k < oneAfterLastNeighbor; k++)
					{
						int num = (tileIDToNeighbors_values[k] + 1 >= tileIDToVerts_offsets.Count) ? verts.Count : tileIDToVerts_offsets[tileIDToNeighbors_values[k] + 1];
						for (int l = tileIDToVerts_offsets[tileIDToNeighbors_values[k]]; l < num; l++)
						{
							if (verts[l] == verts[j])
							{
								Tile tile2 = tiles[tileIDToNeighbors_values[k]];
								if (!flag)
								{
									if ((tile2.elevation >= 0f && elevation <= 0f) || (tile2.elevation <= 0f && elevation >= 0f))
									{
										flag = true;
									}
									else if (tile2.elevation > item.x)
									{
										item.x = tile2.elevation;
									}
								}
								break;
							}
						}
					}
					if (flag)
					{
						item.x = 0f;
					}
					if (tile.biome.DrawMaterial.shader != ShaderDatabase.WorldOcean && item.x < 0f)
					{
						item.x = 0f;
					}
					this.elevationValues.Add(item);
				}
				if (i % 1000 == 0)
				{
					yield return null;
				}
			}
		}
	}
}
