using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGrid
	{
		private const int SubdivisionsCount = 10;

		public const float PlanetRadius = 100f;

		public List<Tile> tiles = new List<Tile>();

		public List<Vector3> verts;

		public List<int> tileIDToVerts_offsets;

		public List<int> tileIDToNeighbors_offsets;

		public List<int> tileIDToNeighbors_values;

		public float averageTileSize;

		public Vector3 viewCenter;

		public float viewAngle;

		private List<int> tileIndices;

		private static List<int> tmpNeighbors = new List<int>();

		private int cachedTraversalDistance = -1;

		private int cachedTraversalDistanceForStart = -1;

		private int cachedTraversalDistanceForEnd = -1;

		public int TilesCount
		{
			get
			{
				return this.tileIDToNeighbors_offsets.Count;
			}
		}

		public Vector3 NorthPolePos
		{
			get
			{
				return new Vector3(0f, 100f, 0f);
			}
		}

		public List<int> TileIndices
		{
			get
			{
				if (this.tileIndices == null)
				{
					int tilesCount = this.TilesCount;
					this.tileIndices = new List<int>();
					for (int i = 0; i < tilesCount; i++)
					{
						this.tileIndices.Add(i);
					}
				}
				return this.tileIndices;
			}
		}

		public Tile this[int tileID]
		{
			get
			{
				return this.tiles[tileID];
			}
		}

		public WorldGrid()
		{
			this.CalculateViewCenterAndAngle();
			PlanetShapeGenerator.Generate(10, out this.verts, out this.tileIDToVerts_offsets, out this.tileIDToNeighbors_offsets, out this.tileIDToNeighbors_values, 100f, this.viewCenter, this.viewAngle);
			this.CalculateAverageTileSize();
		}

		public bool InBounds(int tileID)
		{
			return tileID >= 0 && tileID < this.TilesCount;
		}

		public Vector2 LongLatOf(int tileID)
		{
			Vector3 tileCenter = this.GetTileCenter(tileID);
			float x = Mathf.Atan2(tileCenter.x, -tileCenter.z) * 57.29578f;
			float y = Mathf.Asin(tileCenter.y / 100f) * 57.29578f;
			return new Vector2(x, y);
		}

		public float GetHeadingFromTo(Vector3 from, Vector3 to)
		{
			if (from == to)
			{
				return 0f;
			}
			Vector3 northPolePos = this.NorthPolePos;
			Vector3 from2;
			Vector3 rhs;
			WorldRendererUtility.GetTangentialVectorFacing(from, northPolePos, out from2, out rhs);
			Vector3 vector;
			Vector3 vector2;
			WorldRendererUtility.GetTangentialVectorFacing(from, to, out vector, out vector2);
			float num = Vector3.Angle(from2, vector);
			float num2 = Vector3.Dot(vector, rhs);
			if (num2 < 0f)
			{
				num = 360f - num;
			}
			return num;
		}

		public float GetHeadingFromTo(int fromTileID, int toTileID)
		{
			if (fromTileID == toTileID)
			{
				return 0f;
			}
			Vector3 tileCenter = this.GetTileCenter(fromTileID);
			Vector3 tileCenter2 = this.GetTileCenter(toTileID);
			return this.GetHeadingFromTo(tileCenter, tileCenter2);
		}

		public Direction8Way GetDirection8WayFromTo(int fromTileID, int toTileID)
		{
			float headingFromTo = this.GetHeadingFromTo(fromTileID, toTileID);
			if (headingFromTo >= 337.5f || headingFromTo < 22.5f)
			{
				return Direction8Way.North;
			}
			if (headingFromTo < 67.5f)
			{
				return Direction8Way.NorthEast;
			}
			if (headingFromTo < 112.5f)
			{
				return Direction8Way.East;
			}
			if (headingFromTo < 157.5f)
			{
				return Direction8Way.SouthEast;
			}
			if (headingFromTo < 202.5f)
			{
				return Direction8Way.South;
			}
			if (headingFromTo < 247.5f)
			{
				return Direction8Way.SouthWest;
			}
			if (headingFromTo < 292.5f)
			{
				return Direction8Way.West;
			}
			return Direction8Way.NorthWest;
		}

		public Rot4 GetRotFromTo(int fromTileID, int toTileID)
		{
			float headingFromTo = this.GetHeadingFromTo(fromTileID, toTileID);
			if (headingFromTo >= 315f || headingFromTo < 45f)
			{
				return Rot4.North;
			}
			if (headingFromTo < 135f)
			{
				return Rot4.East;
			}
			if (headingFromTo < 225f)
			{
				return Rot4.South;
			}
			return Rot4.West;
		}

		public void GetTileVertices(int tileID, List<Vector3> outVerts)
		{
			PackedListOfLists.GetList<Vector3>(this.tileIDToVerts_offsets, this.verts, tileID, outVerts);
		}

		public void GetTileVerticesIndices(int tileID, List<int> outVertsIndices)
		{
			outVertsIndices.Clear();
			int num = this.tileIDToVerts_offsets[tileID];
			int num2 = this.verts.Count;
			if (tileID + 1 < this.tileIDToVerts_offsets.Count)
			{
				num2 = this.tileIDToVerts_offsets[tileID + 1];
			}
			for (int i = num; i < num2; i++)
			{
				outVertsIndices.Add(i);
			}
		}

		public void GetTileNeighbors(int tileID, List<int> outNeighbors)
		{
			PackedListOfLists.GetList<int>(this.tileIDToNeighbors_offsets, this.tileIDToNeighbors_values, tileID, outNeighbors);
		}

		public bool IsNeighbor(int tile1, int tile2)
		{
			this.GetTileNeighbors(tile1, WorldGrid.tmpNeighbors);
			return WorldGrid.tmpNeighbors.Contains(tile2);
		}

		public bool IsNeighborOrSame(int tile1, int tile2)
		{
			return tile1 == tile2 || this.IsNeighbor(tile1, tile2);
		}

		public Vector3 GetTileCenter(int tileID)
		{
			int num = (tileID + 1 >= this.tileIDToVerts_offsets.Count) ? this.verts.Count : this.tileIDToVerts_offsets[tileID + 1];
			Vector3 a = Vector3.zero;
			int num2 = 0;
			for (int i = this.tileIDToVerts_offsets[tileID]; i < num; i++)
			{
				a += this.verts[i];
				num2++;
			}
			return a / (float)num2;
		}

		public float DistanceFromEquatorNormalized(int tile)
		{
			return Mathf.Abs(Find.WorldGrid.GetTileCenter(tile).y / 100f);
		}

		public float ApproxDistanceInTiles(float sphericalDistance)
		{
			return sphericalDistance * 100f / this.averageTileSize;
		}

		public float ApproxDistanceInTiles(int firstTile, int secondTile)
		{
			Vector3 tileCenter = this.GetTileCenter(firstTile);
			Vector3 tileCenter2 = this.GetTileCenter(secondTile);
			return this.ApproxDistanceInTiles(GenMath.SphericalDistance(tileCenter.normalized, tileCenter2.normalized));
		}

		public int TraversalDistanceBetween(int start, int end)
		{
			if (start < 0 || end < 0)
			{
				return 0;
			}
			if (this.cachedTraversalDistanceForStart == start && this.cachedTraversalDistanceForEnd == end)
			{
				return this.cachedTraversalDistance;
			}
			int finalDist = -1;
			WorldFloodFiller.FloodFill(start, (int x) => true, delegate(int tile, int dist)
			{
				if (tile == end)
				{
					finalDist = dist;
					return true;
				}
				return false;
			}, 2147483647);
			if (finalDist < 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not reach tile ",
					end,
					" from ",
					start
				}));
				return 0;
			}
			this.cachedTraversalDistance = finalDist;
			this.cachedTraversalDistanceForStart = start;
			this.cachedTraversalDistanceForEnd = end;
			return finalDist;
		}

		private void CalculateAverageTileSize()
		{
			int tilesCount = this.TilesCount;
			double num = 0.0;
			int num2 = 0;
			for (int i = 0; i < tilesCount; i++)
			{
				Vector3 tileCenter = this.GetTileCenter(i);
				int num3 = (i + 1 >= this.tileIDToNeighbors_offsets.Count) ? this.tileIDToNeighbors_values.Count : this.tileIDToNeighbors_offsets[i + 1];
				for (int j = this.tileIDToNeighbors_offsets[i]; j < num3; j++)
				{
					int tileID = this.tileIDToNeighbors_values[j];
					Vector3 tileCenter2 = this.GetTileCenter(tileID);
					num += (double)Vector3.Distance(tileCenter, tileCenter2);
					num2++;
				}
			}
			this.averageTileSize = (float)(num / (double)num2);
		}

		private void CalculateViewCenterAndAngle()
		{
			this.viewAngle = Find.World.PlanetCoverage * 180f;
			this.viewCenter = Vector3.back;
			float angle = 45f;
			if (this.viewAngle > 45f)
			{
				angle = Mathf.Max(90f - this.viewAngle, 0f);
			}
			this.viewCenter = Quaternion.AngleAxis(angle, Vector3.right) * this.viewCenter;
		}
	}
}
