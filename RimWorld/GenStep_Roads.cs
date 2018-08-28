using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class GenStep_Roads : GenStep
	{
		private struct NeededRoad
		{
			public float angle;

			public RoadDef road;
		}

		private struct DrawCommand
		{
			public RoadDef roadDef;

			public Action action;
		}

		public struct DistanceElement
		{
			public float fromRoad;

			public float alongPath;

			public bool touched;

			public IntVec3 origin;
		}

		private const float CurveControlPointDistance = 4f;

		private const int CurveSampleMultiplier = 4;

		private readonly float[] endcapSamples = new float[]
		{
			0.75f,
			0.8f,
			0.85f,
			0.9f,
			0.95f
		};

		public override int SeedPart
		{
			get
			{
				return 1187464702;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			List<GenStep_Roads.NeededRoad> neededRoads = this.CalculateNeededRoads(map);
			if (neededRoads.Count == 0)
			{
				return;
			}
			List<GenStep_Roads.DrawCommand> list = new List<GenStep_Roads.DrawCommand>();
			DeepProfiler.Start("RebuildAllRegions");
			map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			DeepProfiler.End();
			TerrainDef rockDef = BaseGenUtility.RegionalRockTerrainDef(map.Tile, false);
			IntVec3 intVec = CellFinderLoose.TryFindCentralCell(map, 3, 10, null);
			RoadDef bestRoadType = (from rd in DefDatabase<RoadDef>.AllDefs
			where neededRoads.Count((GenStep_Roads.NeededRoad nr) => nr.road == rd) >= 2
			select rd).MaxByWithFallback((RoadDef rd) => rd.priority, null);
			if (bestRoadType != null)
			{
				GenStep_Roads.NeededRoad neededRoad = neededRoads[neededRoads.FindIndex((GenStep_Roads.NeededRoad nr) => nr.road == bestRoadType)];
				neededRoads.RemoveAt(neededRoads.FindIndex((GenStep_Roads.NeededRoad nr) => nr.road == bestRoadType));
				GenStep_Roads.NeededRoad neededRoad2 = neededRoads[neededRoads.FindIndex((GenStep_Roads.NeededRoad nr) => nr.road == bestRoadType)];
				neededRoads.RemoveAt(neededRoads.FindIndex((GenStep_Roads.NeededRoad nr) => nr.road == bestRoadType));
				RoadPathingDef pathingMode = neededRoad.road.pathingMode;
				IntVec3 intVec2 = this.FindRoadExitCell(map, neededRoad.angle, intVec, ref pathingMode);
				IntVec3 end = this.FindRoadExitCell(map, neededRoad2.angle, intVec2, ref pathingMode);
				Action action = this.PrepDrawRoad(map, rockDef, intVec2, end, neededRoad.road, pathingMode, out intVec);
				list.Add(new GenStep_Roads.DrawCommand
				{
					action = action,
					roadDef = bestRoadType
				});
			}
			foreach (GenStep_Roads.NeededRoad current in neededRoads)
			{
				RoadPathingDef pathingMode2 = current.road.pathingMode;
				IntVec3 intVec3 = this.FindRoadExitCell(map, current.angle, intVec, ref pathingMode2);
				if (!(intVec3 == IntVec3.Invalid))
				{
					list.Add(new GenStep_Roads.DrawCommand
					{
						action = this.PrepDrawRoad(map, rockDef, intVec, intVec3, current.road, pathingMode2),
						roadDef = current.road
					});
				}
			}
			foreach (GenStep_Roads.DrawCommand current2 in from dc in list
			orderby dc.roadDef.priority
			select dc)
			{
				if (current2.action != null)
				{
					current2.action();
				}
			}
		}

		private List<GenStep_Roads.NeededRoad> CalculateNeededRoads(Map map)
		{
			List<int> list = new List<int>();
			Find.WorldGrid.GetTileNeighbors(map.Tile, list);
			List<GenStep_Roads.NeededRoad> list2 = new List<GenStep_Roads.NeededRoad>();
			foreach (int current in list)
			{
				RoadDef roadDef = Find.WorldGrid.GetRoadDef(map.Tile, current, true);
				if (roadDef != null)
				{
					list2.Add(new GenStep_Roads.NeededRoad
					{
						angle = Find.WorldGrid.GetHeadingFromTo(map.Tile, current),
						road = roadDef
					});
				}
			}
			if (list2.Count > 1)
			{
				Vector3 vector = Vector3.zero;
				foreach (GenStep_Roads.NeededRoad current2 in list2)
				{
					vector += Vector3Utility.HorizontalVectorFromAngle(current2.angle);
				}
				vector /= (float)(-(float)list2.Count);
				vector += Rand.UnitVector3 * 1f / 6f;
				vector.y = 0f;
				for (int i = 0; i < list2.Count; i++)
				{
					list2[i] = new GenStep_Roads.NeededRoad
					{
						angle = (Vector3Utility.HorizontalVectorFromAngle(list2[i].angle) + vector).AngleFlat(),
						road = list2[i].road
					};
				}
			}
			return list2;
		}

		private IntVec3 FindRoadExitCell(Map map, float angle, IntVec3 crossroads, ref RoadPathingDef pathingDef)
		{
			Predicate<IntVec3> tileValidator = delegate(IntVec3 pos)
			{
				foreach (IntVec3 current in GenRadial.RadialCellsAround(pos, 8f, true))
				{
					if (current.InBounds(map) && current.GetTerrain(map).IsWater)
					{
						return false;
					}
				}
				return true;
			};
			float validAngleSpan;
			for (validAngleSpan = 10f; validAngleSpan < 90f; validAngleSpan += 10f)
			{
				Predicate<IntVec3> angleValidator = (IntVec3 pos) => GenGeo.AngleDifferenceBetween((pos - map.Center).AngleFlat, angle) < validAngleSpan;
				IntVec3 result;
				if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => angleValidator(x) && tileValidator(x) && map.reachability.CanReach(crossroads, x, PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false)), map, 0f, out result))
				{
					return result;
				}
			}
			if (pathingDef == RoadPathingDefOf.Avoid)
			{
				pathingDef = RoadPathingDefOf.Bulldoze;
			}
			float validAngleSpan2;
			for (validAngleSpan2 = 10f; validAngleSpan2 < 90f; validAngleSpan2 += 10f)
			{
				Predicate<IntVec3> angleValidator = (IntVec3 pos) => GenGeo.AngleDifferenceBetween((pos - map.Center).AngleFlat, angle) < validAngleSpan2;
				IntVec3 result;
				if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => angleValidator(x) && tileValidator(x) && map.reachability.CanReach(crossroads, x, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassAllDestroyableThings, Danger.Deadly, false)), map, 0f, out result))
				{
					return result;
				}
			}
			Log.Error(string.Format("Can't find exit from map from {0} to angle {1}", crossroads, angle), false);
			return IntVec3.Invalid;
		}

		private Action PrepDrawRoad(Map map, TerrainDef rockDef, IntVec3 start, IntVec3 end, RoadDef roadDef, RoadPathingDef pathingDef)
		{
			IntVec3 intVec;
			return this.PrepDrawRoad(map, rockDef, start, end, roadDef, pathingDef, out intVec);
		}

		private Action PrepDrawRoad(Map map, TerrainDef rockDef, IntVec3 start, IntVec3 end, RoadDef roadDef, RoadPathingDef pathingDef, out IntVec3 centerpoint)
		{
			centerpoint = IntVec3.Invalid;
			PawnPath pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater, Danger.Deadly, false), PathEndMode.OnCell);
			if (pawnPath == PawnPath.NotFound)
			{
				pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), PathEndMode.OnCell);
			}
			if (pawnPath == PawnPath.NotFound)
			{
				pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.PassAllDestroyableThingsNotWater, Danger.Deadly, false), PathEndMode.OnCell);
			}
			if (pawnPath == PawnPath.NotFound)
			{
				pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.PassAllDestroyableThings, Danger.Deadly, false), PathEndMode.OnCell);
			}
			if (pawnPath == PawnPath.NotFound)
			{
				return null;
			}
			List<IntVec3> list = this.RefinePath(pawnPath.NodesReversed, map);
			pawnPath.ReleaseToPool();
			GenStep_Roads.DistanceElement[,] distance = new GenStep_Roads.DistanceElement[map.Size.x, map.Size.z];
			for (int i = 0; i < distance.GetLength(0); i++)
			{
				for (int j = 0; j < distance.GetLength(1); j++)
				{
					distance[i, j].origin = IntVec3.Invalid;
				}
			}
			int count = list.Count;
			int centerpointIndex = Mathf.RoundToInt(Rand.Range(0.3f, 0.7f) * (float)count);
			int num = Mathf.Max(1, GenMath.RoundRandom((float)count / (float)roadDef.tilesPerSegment));
			for (int k = 0; k < num; k++)
			{
				int pathStartIndex = Mathf.RoundToInt((float)(count - 1) / (float)num * (float)k);
				int pathEndIndex = Mathf.RoundToInt((float)(count - 1) / (float)num * (float)(k + 1));
				this.DrawCurveSegment(distance, list, pathStartIndex, pathEndIndex, pathingDef, map, centerpointIndex, ref centerpoint);
			}
			return delegate
			{
				this.ApplyDistanceField(distance, map, rockDef, roadDef, pathingDef);
			};
		}

		private void DrawCurveSegment(GenStep_Roads.DistanceElement[,] distance, List<IntVec3> path, int pathStartIndex, int pathEndIndex, RoadPathingDef pathing, Map map, int centerpointIndex, ref IntVec3 centerpoint)
		{
			if (pathStartIndex == pathEndIndex)
			{
				Log.ErrorOnce("Zero-length segment drawn in road routine", 78187971, false);
				return;
			}
			GenMath.BezierCubicControls bcc = this.GenerateBezierControls(path, pathStartIndex, pathEndIndex);
			List<Vector3> list = new List<Vector3>();
			int num = (pathEndIndex - pathStartIndex) * 4;
			for (int i = 0; i <= num; i++)
			{
				list.Add(GenMath.BezierCubicEvaluate((float)i / (float)num, bcc));
			}
			int num2 = 0;
			for (int j = pathStartIndex; j <= pathEndIndex; j++)
			{
				if (j > 0 && j < path.Count && path[j].InBounds(map) && path[j].GetTerrain(map).IsWater)
				{
					num2++;
				}
			}
			if (pathStartIndex + 1 < pathEndIndex)
			{
				for (int k = 0; k < list.Count; k++)
				{
					IntVec3 intVec = list[k].ToIntVec3();
					bool flag = intVec.InBounds(map) && intVec.Impassable(map);
					int num3 = 0;
					int num4 = 0;
					while (num4 < GenAdj.CardinalDirections.Length && !flag)
					{
						IntVec3 c = intVec + GenAdj.CardinalDirections[num4];
						if (c.InBounds(map))
						{
							flag |= (pathing == RoadPathingDefOf.Avoid && c.Impassable(map));
							if (c.GetTerrain(map).IsWater)
							{
								num3++;
							}
							if (flag)
							{
								break;
							}
						}
						num4++;
					}
					if (flag || (float)num3 > (float)num2 * 1.5f + 2f)
					{
						this.DrawCurveSegment(distance, path, pathStartIndex, (pathStartIndex + pathEndIndex) / 2, pathing, map, centerpointIndex, ref centerpoint);
						this.DrawCurveSegment(distance, path, (pathStartIndex + pathEndIndex) / 2, pathEndIndex, pathing, map, centerpointIndex, ref centerpoint);
						return;
					}
				}
			}
			for (int l = 0; l < list.Count; l++)
			{
				this.FillDistanceField(distance, list[l].x, list[l].z, GenMath.LerpDouble(0f, (float)(list.Count - 1), (float)pathStartIndex, (float)pathEndIndex, (float)l), 10f, map);
			}
			if (centerpointIndex >= pathStartIndex && centerpointIndex < pathEndIndex)
			{
				centerpointIndex = Mathf.Clamp(Mathf.RoundToInt(GenMath.LerpDouble((float)pathStartIndex, (float)pathEndIndex, 0f, (float)list.Count, (float)centerpointIndex)), 0, list.Count - 1);
				centerpoint = list[centerpointIndex].ToIntVec3();
			}
		}

		private GenMath.BezierCubicControls GenerateBezierControls(List<IntVec3> path, int pathStartIndex, int pathEndIndex)
		{
			int index = Mathf.Max(0, pathStartIndex - (pathEndIndex - pathStartIndex));
			int index2 = Mathf.Min(path.Count - 1, pathEndIndex - (pathStartIndex - pathEndIndex));
			return new GenMath.BezierCubicControls
			{
				w0 = path[pathStartIndex].ToVector3Shifted(),
				w1 = path[pathStartIndex].ToVector3Shifted() + (path[pathEndIndex] - path[index]).ToVector3().normalized * 4f,
				w2 = path[pathEndIndex].ToVector3Shifted() + (path[pathStartIndex] - path[index2]).ToVector3().normalized * 4f,
				w3 = path[pathEndIndex].ToVector3Shifted()
			};
		}

		private void ApplyDistanceField(GenStep_Roads.DistanceElement[,] distance, Map map, TerrainDef rockDef, RoadDef roadDef, RoadPathingDef pathingDef)
		{
			for (int i = 0; i < map.Size.x; i++)
			{
				for (int j = 0; j < map.Size.z; j++)
				{
					GenStep_Roads.DistanceElement distanceElement = distance[i, j];
					if (distanceElement.touched)
					{
						float b = Mathf.Abs(distanceElement.fromRoad + Rand.Value - 0.5f);
						for (int k = 0; k < roadDef.roadGenSteps.Count; k++)
						{
							RoadDefGenStep roadDefGenStep = roadDef.roadGenSteps[k];
							float x = Mathf.LerpUnclamped(distanceElement.fromRoad, b, roadDefGenStep.antialiasingMultiplier);
							float num = roadDefGenStep.chancePerPositionCurve.Evaluate(x);
							if (num > 0f)
							{
								if (roadDefGenStep.periodicSpacing == 0 || distanceElement.alongPath / (float)roadDefGenStep.periodicSpacing % 1f * (float)roadDefGenStep.periodicSpacing < 1f)
								{
									IntVec3 position = new IntVec3(i, 0, j);
									if (Rand.Value < num)
									{
										roadDefGenStep.Place(map, position, rockDef, distanceElement.origin, distance);
									}
								}
							}
						}
					}
				}
			}
		}

		private void FillDistanceField(GenStep_Roads.DistanceElement[,] field, float cx, float cz, float alongPath, float radius, Map map)
		{
			int num = Mathf.Clamp(Mathf.FloorToInt(cx - radius), 0, field.GetLength(0) - 1);
			int num2 = Mathf.Clamp(Mathf.FloorToInt(cx + radius), 0, field.GetLength(0) - 1);
			int num3 = Mathf.Clamp(Mathf.FloorToInt(cz - radius), 0, field.GetLength(1) - 1);
			int num4 = Mathf.Clamp(Mathf.FloorToInt(cz + radius), 0, field.GetLength(1) - 1);
			IntVec3 origin = new Vector3(cx, 0f, cz).ToIntVec3().ClampInsideMap(map);
			for (int i = num; i <= num2; i++)
			{
				float num5 = ((float)i + 0.5f - cx) * ((float)i + 0.5f - cx);
				for (int j = num3; j <= num4; j++)
				{
					float num6 = ((float)j + 0.5f - cz) * ((float)j + 0.5f - cz);
					float num7 = Mathf.Sqrt(num5 + num6);
					float fromRoad = field[i, j].fromRoad;
					if (!field[i, j].touched || num7 < fromRoad)
					{
						field[i, j].fromRoad = num7;
						field[i, j].alongPath = alongPath;
						field[i, j].origin = origin;
					}
					field[i, j].touched = true;
				}
			}
		}

		private List<IntVec3> RefinePath(List<IntVec3> input, Map map)
		{
			List<IntVec3> list = this.RefineEndcap(input, map);
			list.Reverse();
			return this.RefineEndcap(list, map);
		}

		private List<IntVec3> RefineEndcap(List<IntVec3> input, Map map)
		{
			float[] array = new float[this.endcapSamples.Length];
			for (int i = 0; i < this.endcapSamples.Length; i++)
			{
				int index = Mathf.RoundToInt((float)input.Count * this.endcapSamples[i]);
				PawnPath pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater, Danger.Deadly, false), PathEndMode.OnCell);
				if (pawnPath == PawnPath.NotFound)
				{
					pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), PathEndMode.OnCell);
				}
				if (pawnPath == PawnPath.NotFound)
				{
					pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.PassAllDestroyableThingsNotWater, Danger.Deadly, false), PathEndMode.OnCell);
				}
				if (pawnPath == PawnPath.NotFound)
				{
					pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.PassAllDestroyableThings, Danger.Deadly, false), PathEndMode.OnCell);
				}
				if (pawnPath != null && pawnPath != PawnPath.NotFound)
				{
					array[i] = pawnPath.TotalCost;
				}
				pawnPath.ReleaseToPool();
			}
			float num = 0f;
			int num2 = 0;
			IntVec3 start = IntVec3.Invalid;
			for (int j = 0; j < 2; j++)
			{
				Rot4 rot = new Rot4(j);
				IntVec3 facingCell = rot.FacingCell;
				IntVec3 intVec = input[input.Count - 1];
				bool flag = true;
				if (Mathf.Abs(intVec.x * facingCell.x) > 5 && Mathf.Abs(intVec.x * facingCell.x - map.Size.x) > 5)
				{
					flag = false;
				}
				if (Mathf.Abs(intVec.z * facingCell.z) > 5 && Mathf.Abs(intVec.z * facingCell.z - map.Size.z) > 5)
				{
					flag = false;
				}
				if (flag)
				{
					for (int k = 0; k < this.endcapSamples.Length; k++)
					{
						if (array[k] != 0f)
						{
							int num3 = Mathf.RoundToInt((float)input.Count * this.endcapSamples[k]);
							IntVec3 intVec2 = input[num3];
							IntVec3 intVec3 = intVec2;
							if (facingCell.x != 0)
							{
								intVec3.x = intVec.x;
							}
							else if (facingCell.z != 0)
							{
								intVec3.z = intVec.z;
							}
							PawnPath pawnPath2 = map.pathFinder.FindPath(input[num3], input[input.Count - 1], TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), PathEndMode.OnCell);
							if (pawnPath2 == PawnPath.NotFound)
							{
								pawnPath2 = map.pathFinder.FindPath(input[num3], input[input.Count - 1], TraverseParms.For(TraverseMode.PassAllDestroyableThings, Danger.Deadly, false), PathEndMode.OnCell);
							}
							if (pawnPath2 != PawnPath.NotFound)
							{
								float num4 = array[k] / pawnPath2.TotalCost;
								if (num4 > num)
								{
									num = num4;
									num2 = num3;
									start = intVec3;
								}
								pawnPath2.ReleaseToPool();
							}
						}
					}
				}
			}
			input = new List<IntVec3>(input);
			if ((double)num > 1.75)
			{
				using (PawnPath pawnPath3 = map.pathFinder.FindPath(start, input[num2], TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), PathEndMode.OnCell))
				{
					input.RemoveRange(num2, input.Count - num2);
					input.AddRange(pawnPath3.NodesReversed);
				}
			}
			return input;
		}
	}
}
