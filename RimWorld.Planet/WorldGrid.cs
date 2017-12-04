using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGrid : IExposable
	{
		public List<Tile> tiles = new List<Tile>();

		public List<Vector3> verts;

		public List<int> tileIDToVerts_offsets;

		public List<int> tileIDToNeighbors_offsets;

		public List<int> tileIDToNeighbors_values;

		public float averageTileSize;

		public Vector3 viewCenter;

		public float viewAngle;

		private byte[] tileBiome;

		private byte[] tileElevation;

		private byte[] tileHilliness;

		private byte[] tileTemperature;

		private byte[] tileRainfall;

		private byte[] tileSwampiness;

		public byte[] tileFeature;

		private byte[] tileRoadOrigins;

		private byte[] tileRoadAdjacency;

		private byte[] tileRoadDef;

		private byte[] tileRiverOrigins;

		private byte[] tileRiverAdjacency;

		private byte[] tileRiverDef;

		private static List<int> tmpNeighbors = new List<int>();

		private const int SubdivisionsCount = 10;

		public const float PlanetRadius = 100f;

		public const int ElevationOffset = 8192;

		public const int TemperatureOffset = 300;

		public const float TemperatureMultiplier = 10f;

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

		public Tile this[int tileID]
		{
			get
			{
				return ((ulong)tileID >= (ulong)((long)this.TilesCount)) ? null : this.tiles[tileID];
			}
		}

		public bool HasWorldData
		{
			get
			{
				return this.tileBiome != null;
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
			return (ulong)tileID < (ulong)((long)this.TilesCount);
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
			PackedListOfLists.GetListValuesIndices<Vector3>(this.tileIDToVerts_offsets, this.verts, tileID, outVertsIndices);
		}

		public void GetTileNeighbors(int tileID, List<int> outNeighbors)
		{
			PackedListOfLists.GetList<int>(this.tileIDToNeighbors_offsets, this.tileIDToNeighbors_values, tileID, outNeighbors);
		}

		public int GetTileNeighborCount(int tileID)
		{
			return PackedListOfLists.GetListCount<int>(this.tileIDToNeighbors_offsets, this.tileIDToNeighbors_values, tileID);
		}

		public int GetMaxTileNeighborCountEver(int tileID)
		{
			return PackedListOfLists.GetListCount<Vector3>(this.tileIDToVerts_offsets, this.verts, tileID);
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

		public int GetNeighborId(int tile1, int tile2)
		{
			this.GetTileNeighbors(tile1, WorldGrid.tmpNeighbors);
			return WorldGrid.tmpNeighbors.IndexOf(tile2);
		}

		public int GetTileNeighbor(int tileID, int adjacentId)
		{
			this.GetTileNeighbors(tileID, WorldGrid.tmpNeighbors);
			return WorldGrid.tmpNeighbors[adjacentId];
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

		public float TileRadiusToAngle(float radius)
		{
			return this.DistOnSurfaceToAngle(radius * this.averageTileSize);
		}

		public float DistOnSurfaceToAngle(float dist)
		{
			return dist / 628.318542f * 360f;
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

		public void OverlayRoad(int fromTile, int toTile, RoadDef roadDef)
		{
			if (roadDef == null)
			{
				Log.ErrorOnce("Attempted to remove road with overlayRoad; not supported", 90292249);
				return;
			}
			RoadDef roadDef2 = this.GetRoadDef(fromTile, toTile, false);
			if (roadDef2 == roadDef)
			{
				return;
			}
			Tile tile = this[fromTile];
			Tile tile2 = this[toTile];
			if (roadDef2 != null)
			{
				if (roadDef2.priority >= roadDef.priority)
				{
					return;
				}
				tile.roads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == toTile);
				tile2.roads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == fromTile);
			}
			if (tile.roads == null)
			{
				tile.roads = new List<Tile.RoadLink>();
			}
			if (tile2.roads == null)
			{
				tile2.roads = new List<Tile.RoadLink>();
			}
			tile.roads.Add(new Tile.RoadLink
			{
				neighbor = toTile,
				road = roadDef
			});
			tile2.roads.Add(new Tile.RoadLink
			{
				neighbor = fromTile,
				road = roadDef
			});
		}

		public RoadDef GetRoadDef(int fromTile, int toTile, bool visibleOnly = true)
		{
			if (!this.IsNeighbor(fromTile, toTile))
			{
				Log.ErrorOnce("Tried to find road information between non-neighboring tiles", 12390444);
				return null;
			}
			Tile tile = this.tiles[fromTile];
			List<Tile.RoadLink> list = (!visibleOnly) ? tile.roads : tile.VisibleRoads;
			if (list == null)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].neighbor == toTile)
				{
					return list[i].road;
				}
			}
			return null;
		}

		public void OverlayRiver(int fromTile, int toTile, RiverDef riverDef)
		{
			if (riverDef == null)
			{
				Log.ErrorOnce("Attempted to remove river with overlayRiver; not supported", 90292250);
				return;
			}
			RiverDef riverDef2 = this.GetRiverDef(fromTile, toTile, false);
			if (riverDef2 == riverDef)
			{
				return;
			}
			Tile tile = this[fromTile];
			Tile tile2 = this[toTile];
			if (riverDef2 != null)
			{
				if (riverDef2.degradeThreshold >= riverDef.degradeThreshold)
				{
					return;
				}
				tile.rivers.RemoveAll((Tile.RiverLink rl) => rl.neighbor == toTile);
				tile2.rivers.RemoveAll((Tile.RiverLink rl) => rl.neighbor == fromTile);
			}
			if (tile.rivers == null)
			{
				tile.rivers = new List<Tile.RiverLink>();
			}
			if (tile2.rivers == null)
			{
				tile2.rivers = new List<Tile.RiverLink>();
			}
			tile.rivers.Add(new Tile.RiverLink
			{
				neighbor = toTile,
				river = riverDef
			});
			tile2.rivers.Add(new Tile.RiverLink
			{
				neighbor = fromTile,
				river = riverDef
			});
		}

		public RiverDef GetRiverDef(int fromTile, int toTile, bool visibleOnly = true)
		{
			if (!this.IsNeighbor(fromTile, toTile))
			{
				Log.ErrorOnce("Tried to find river information between non-neighboring tiles", 12390444);
				return null;
			}
			Tile tile = this.tiles[fromTile];
			List<Tile.RiverLink> list = (!visibleOnly) ? tile.rivers : tile.VisibleRivers;
			if (list == null)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].neighbor == toTile)
				{
					return list[i].river;
				}
			}
			return null;
		}

		public float GetRoadMovementMultiplierFast(int fromTile, int toTile)
		{
			List<Tile.RoadLink> roads = this.tiles[fromTile].roads;
			if (roads == null)
			{
				return 1f;
			}
			for (int i = 0; i < roads.Count; i++)
			{
				if (roads[i].neighbor == toTile)
				{
					return roads[i].road.movementCostMultiplier;
				}
			}
			return 1f;
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
			Find.WorldFloodFiller.FloodFill(start, (int x) => true, delegate(int tile, int dist)
			{
				if (tile == end)
				{
					finalDist = dist;
					return true;
				}
				return false;
			}, 2147483647, null);
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

		public bool IsOnEdge(int tileID)
		{
			return this.InBounds(tileID) && this.GetTileNeighborCount(tileID) < this.GetMaxTileNeighborCountEver(tileID);
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

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.TilesToRawData();
			}
			DataExposeUtility.ByteArray(ref this.tileBiome, "tileBiome");
			DataExposeUtility.ByteArray(ref this.tileElevation, "tileElevation");
			DataExposeUtility.ByteArray(ref this.tileHilliness, "tileHilliness");
			DataExposeUtility.ByteArray(ref this.tileTemperature, "tileTemperature");
			DataExposeUtility.ByteArray(ref this.tileRainfall, "tileRainfall");
			DataExposeUtility.ByteArray(ref this.tileSwampiness, "tileSwampiness");
			DataExposeUtility.ByteArray(ref this.tileFeature, "tileFeature");
			DataExposeUtility.ByteArray(ref this.tileRoadOrigins, "tileRoadOrigins");
			DataExposeUtility.ByteArray(ref this.tileRoadAdjacency, "tileRoadAdjacency");
			DataExposeUtility.ByteArray(ref this.tileRoadDef, "tileRoadDef");
			DataExposeUtility.ByteArray(ref this.tileRiverOrigins, "tileRiverOrigins");
			DataExposeUtility.ByteArray(ref this.tileRiverAdjacency, "tileRiverAdjacency");
			DataExposeUtility.ByteArray(ref this.tileRiverDef, "tileRiverDef");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.RawDataToTiles();
			}
		}

		public void StandardizeTileData()
		{
			this.TilesToRawData();
			this.RawDataToTiles();
		}

		private void TilesToRawData()
		{
			this.tileBiome = DataSerializeUtility.SerializeUshort(this.TilesCount, (int i) => this.tiles[i].biome.shortHash);
			this.tileElevation = DataSerializeUtility.SerializeUshort(this.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt(((!this.tiles[i].WaterCovered) ? Mathf.Max(this.tiles[i].elevation, 1f) : this.tiles[i].elevation) + 8192f), 0, 65535));
			this.tileHilliness = DataSerializeUtility.SerializeByte(this.TilesCount, (int i) => (byte)this.tiles[i].hilliness);
			this.tileTemperature = DataSerializeUtility.SerializeUshort(this.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt((this.tiles[i].temperature + 300f) * 10f), 0, 65535));
			this.tileRainfall = DataSerializeUtility.SerializeUshort(this.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt(this.tiles[i].rainfall), 0, 65535));
			this.tileSwampiness = DataSerializeUtility.SerializeByte(this.TilesCount, (int i) => (byte)Mathf.Clamp(Mathf.RoundToInt(this.tiles[i].swampiness * 255f), 0, 255));
			this.tileFeature = DataSerializeUtility.SerializeUshort(this.TilesCount, (int i) => (this.tiles[i].feature != null) ? ((ushort)this.tiles[i].feature.uniqueID) : 65535);
			List<int> list = new List<int>();
			List<byte> list2 = new List<byte>();
			List<ushort> list3 = new List<ushort>();
			for (int m = 0; m < this.TilesCount; m++)
			{
				List<Tile.RoadLink> roads = this.tiles[m].roads;
				if (roads != null)
				{
					for (int j = 0; j < roads.Count; j++)
					{
						Tile.RoadLink roadLink = roads[j];
						if (roadLink.neighbor >= m)
						{
							byte b = (byte)this.GetNeighborId(m, roadLink.neighbor);
							if (b < 0)
							{
								Log.ErrorOnce("Couldn't find valid neighbor for road piece", 81637014);
							}
							else
							{
								list.Add(m);
								list2.Add(b);
								list3.Add(roadLink.road.shortHash);
							}
						}
					}
				}
			}
			this.tileRoadOrigins = DataSerializeUtility.SerializeInt(list.ToArray());
			this.tileRoadAdjacency = DataSerializeUtility.SerializeByte(list2.ToArray());
			this.tileRoadDef = DataSerializeUtility.SerializeUshort(list3.ToArray());
			List<int> list4 = new List<int>();
			List<byte> list5 = new List<byte>();
			List<ushort> list6 = new List<ushort>();
			for (int k = 0; k < this.TilesCount; k++)
			{
				List<Tile.RiverLink> rivers = this.tiles[k].rivers;
				if (rivers != null)
				{
					for (int l = 0; l < rivers.Count; l++)
					{
						Tile.RiverLink riverLink = rivers[l];
						if (riverLink.neighbor >= k)
						{
							byte b2 = (byte)this.GetNeighborId(k, riverLink.neighbor);
							if (b2 < 0)
							{
								Log.ErrorOnce("Couldn't find valid neighbor for river piece", 81637014);
							}
							else
							{
								list4.Add(k);
								list5.Add(b2);
								list6.Add(riverLink.river.shortHash);
							}
						}
					}
				}
			}
			this.tileRiverOrigins = DataSerializeUtility.SerializeInt(list4.ToArray());
			this.tileRiverAdjacency = DataSerializeUtility.SerializeByte(list5.ToArray());
			this.tileRiverDef = DataSerializeUtility.SerializeUshort(list6.ToArray());
		}

		private void RawDataToTiles()
		{
			if (this.tiles.Count != this.TilesCount)
			{
				this.tiles.Clear();
				for (int m = 0; m < this.TilesCount; m++)
				{
					this.tiles.Add(new Tile());
				}
			}
			else
			{
				for (int j = 0; j < this.TilesCount; j++)
				{
					this.tiles[j].roads = null;
					this.tiles[j].rivers = null;
				}
			}
			DataSerializeUtility.LoadUshort(this.tileBiome, this.TilesCount, delegate(int i, ushort data)
			{
				this.tiles[i].biome = DefDatabase<BiomeDef>.GetByShortHash(data);
			});
			DataSerializeUtility.LoadUshort(this.tileElevation, this.TilesCount, delegate(int i, ushort data)
			{
				this.tiles[i].elevation = (float)(data - 8192);
			});
			DataSerializeUtility.LoadByte(this.tileHilliness, this.TilesCount, delegate(int i, byte data)
			{
				this.tiles[i].hilliness = (Hilliness)data;
			});
			DataSerializeUtility.LoadUshort(this.tileTemperature, this.TilesCount, delegate(int i, ushort data)
			{
				this.tiles[i].temperature = (float)data / 10f - 300f;
			});
			DataSerializeUtility.LoadUshort(this.tileRainfall, this.TilesCount, delegate(int i, ushort data)
			{
				this.tiles[i].rainfall = (float)data;
			});
			DataSerializeUtility.LoadByte(this.tileSwampiness, this.TilesCount, delegate(int i, byte data)
			{
				this.tiles[i].swampiness = (float)data / 255f;
			});
			int[] array = DataSerializeUtility.DeserializeInt(this.tileRoadOrigins);
			byte[] array2 = DataSerializeUtility.DeserializeByte(this.tileRoadAdjacency);
			ushort[] array3 = DataSerializeUtility.DeserializeUshort(this.tileRoadDef);
			for (int k = 0; k < array.Length; k++)
			{
				int num = array[k];
				int tileNeighbor = this.GetTileNeighbor(num, (int)array2[k]);
				RoadDef byShortHash = DefDatabase<RoadDef>.GetByShortHash(array3[k]);
				if (byShortHash != null)
				{
					if (this.tiles[num].roads == null)
					{
						this.tiles[num].roads = new List<Tile.RoadLink>();
					}
					if (this.tiles[tileNeighbor].roads == null)
					{
						this.tiles[tileNeighbor].roads = new List<Tile.RoadLink>();
					}
					this.tiles[num].roads.Add(new Tile.RoadLink
					{
						neighbor = tileNeighbor,
						road = byShortHash
					});
					this.tiles[tileNeighbor].roads.Add(new Tile.RoadLink
					{
						neighbor = num,
						road = byShortHash
					});
				}
			}
			int[] array4 = DataSerializeUtility.DeserializeInt(this.tileRiverOrigins);
			byte[] array5 = DataSerializeUtility.DeserializeByte(this.tileRiverAdjacency);
			ushort[] array6 = DataSerializeUtility.DeserializeUshort(this.tileRiverDef);
			for (int l = 0; l < array4.Length; l++)
			{
				int num2 = array4[l];
				int tileNeighbor2 = this.GetTileNeighbor(num2, (int)array5[l]);
				RiverDef byShortHash2 = DefDatabase<RiverDef>.GetByShortHash(array6[l]);
				if (byShortHash2 != null)
				{
					if (this.tiles[num2].rivers == null)
					{
						this.tiles[num2].rivers = new List<Tile.RiverLink>();
					}
					if (this.tiles[tileNeighbor2].rivers == null)
					{
						this.tiles[tileNeighbor2].rivers = new List<Tile.RiverLink>();
					}
					this.tiles[num2].rivers.Add(new Tile.RiverLink
					{
						neighbor = tileNeighbor2,
						river = byShortHash2
					});
					this.tiles[tileNeighbor2].rivers.Add(new Tile.RiverLink
					{
						neighbor = num2,
						river = byShortHash2
					});
				}
			}
		}
	}
}
