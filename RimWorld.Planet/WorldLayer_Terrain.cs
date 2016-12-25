using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldLayer_Terrain : WorldLayer
	{
		private List<MeshCollider> meshCollidersInOrder = new List<MeshCollider>();

		private List<List<int>> triangleIndexToTileID = new List<List<int>>();

		private List<Vector2> elevationValues = new List<Vector2>();

		protected override void Regenerate()
		{
			base.Regenerate();
			World world = Find.World;
			WorldGrid grid = world.grid;
			int tilesCount = grid.TilesCount;
			List<Tile> tiles = grid.tiles;
			List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
			List<Vector3> verts = grid.verts;
			this.triangleIndexToTileID.Clear();
			this.CalculateInterpolatedVerticesParams();
			int num = 0;
			for (int i = 0; i < tilesCount; i++)
			{
				BiomeDef biome = tiles[i].biome;
				int j;
				LayerSubMesh subMesh = base.GetSubMesh(biome.DrawMaterial, out j);
				while (j >= this.triangleIndexToTileID.Count)
				{
					this.triangleIndexToTileID.Add(new List<int>());
				}
				int count = subMesh.verts.Count;
				int num2 = 0;
				int num3 = (i + 1 >= tileIDToVerts_offsets.Count) ? verts.Count : tileIDToVerts_offsets[i + 1];
				for (int k = tileIDToVerts_offsets[i]; k < num3; k++)
				{
					subMesh.verts.Add(verts[k]);
					subMesh.uvs.Add(this.elevationValues[num]);
					num++;
					if (k < num3 - 2)
					{
						subMesh.tris.Add(count + num2 + 2);
						subMesh.tris.Add(count + num2 + 1);
						subMesh.tris.Add(count);
						this.triangleIndexToTileID[j].Add(i);
					}
					num2++;
				}
			}
			base.FinalizeMesh(MeshParts.All, true);
			this.RegenerateMeshColliders();
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

		private void RegenerateMeshColliders()
		{
			this.meshCollidersInOrder.Clear();
			GameObject gameObject = WorldTerrainColliderManager.GameObject;
			MeshCollider[] components = gameObject.GetComponents<MeshCollider>();
			for (int i = 0; i < components.Length; i++)
			{
				MeshCollider obj = components[i];
				UnityEngine.Object.Destroy(obj);
			}
			for (int j = 0; j < this.subMeshes.Count; j++)
			{
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = this.subMeshes[j].mesh;
				this.meshCollidersInOrder.Add(meshCollider);
			}
		}

		private void CalculateInterpolatedVerticesParams()
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
				int num = (i + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[i + 1];
				int num2 = (i + 1 >= tilesCount) ? verts.Count : tileIDToVerts_offsets[i + 1];
				for (int j = tileIDToVerts_offsets[i]; j < num2; j++)
				{
					Vector2 item = default(Vector2);
					item.x = elevation;
					bool flag = false;
					for (int k = tileIDToNeighbors_offsets[i]; k < num; k++)
					{
						int num3 = (tileIDToNeighbors_values[k] + 1 >= tileIDToVerts_offsets.Count) ? verts.Count : tileIDToVerts_offsets[tileIDToNeighbors_values[k] + 1];
						for (int l = tileIDToVerts_offsets[tileIDToNeighbors_values[k]]; l < num3; l++)
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
			}
		}
	}
}
