using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GlowFlooder
	{
		private struct GlowFloodCell
		{
			public int intDist;

			public uint status;
		}

		private class CompareGlowFlooderLightSquares : IComparer<int>
		{
			private GlowFlooder.GlowFloodCell[] grid;

			public CompareGlowFlooderLightSquares(GlowFlooder.GlowFloodCell[] grid)
			{
				this.grid = grid;
			}

			public int Compare(int a, int b)
			{
				if (this.grid[a].intDist > this.grid[b].intDist)
				{
					return 1;
				}
				if (this.grid[a].intDist < this.grid[b].intDist)
				{
					return -1;
				}
				return 0;
			}
		}

		private static GlowFlooder.GlowFloodCell[] calcGrid;

		private static FastPriorityQueue<int> openSet;

		private static uint unseenVal = 0u;

		private static uint openVal = 1u;

		private static uint finalizedVal = 2u;

		private static int mapSizePowTwo;

		private static ushort gridSizeX;

		private static ushort gridSizeY;

		private static ushort gridSizeXMinus1;

		private static ushort gridSizeZLog2;

		private static CompGlower glower;

		private static float attenLinearSlope;

		private static int finalIdx;

		private static Thing[] blockers = new Thing[8];

		private static Color32[] glowGrid;

		private static sbyte[,] Directions = new sbyte[,]
		{
			{
				0,
				-1
			},
			{
				1,
				0
			},
			{
				0,
				1
			},
			{
				-1,
				0
			},
			{
				1,
				-1
			},
			{
				1,
				1
			},
			{
				-1,
				1
			},
			{
				-1,
				-1
			}
		};

		public static void Reinit()
		{
			GlowFlooder.calcGrid = null;
		}

		private static void InitializeWorkingData()
		{
			GlowFlooder.mapSizePowTwo = Find.Map.info.PowerOfTwoOverMapSize;
			GlowFlooder.gridSizeX = (ushort)GlowFlooder.mapSizePowTwo;
			GlowFlooder.gridSizeY = (ushort)GlowFlooder.mapSizePowTwo;
			GlowFlooder.gridSizeXMinus1 = GlowFlooder.gridSizeX - 1;
			GlowFlooder.gridSizeZLog2 = (ushort)Math.Log((double)GlowFlooder.gridSizeY, 2.0);
			if (GlowFlooder.calcGrid == null || GlowFlooder.calcGrid.Length != (int)(GlowFlooder.gridSizeX * GlowFlooder.gridSizeY))
			{
				GlowFlooder.calcGrid = new GlowFlooder.GlowFloodCell[(int)(GlowFlooder.gridSizeX * GlowFlooder.gridSizeY)];
			}
			GlowFlooder.openSet = new FastPriorityQueue<int>(new GlowFlooder.CompareGlowFlooderLightSquares(GlowFlooder.calcGrid));
		}

		public static void AddFloodGlowFor(CompGlower theGlower)
		{
			if (GlowFlooder.calcGrid == null)
			{
				GlowFlooder.InitializeWorkingData();
			}
			GlowFlooder.glowGrid = Find.GlowGrid.glowGrid;
			GlowFlooder.glower = theGlower;
			Thing[] innerArray = Find.EdificeGrid.InnerArray;
			GlowFlooder.unseenVal += 3u;
			GlowFlooder.openVal += 3u;
			GlowFlooder.finalizedVal += 3u;
			IntVec3 position = GlowFlooder.glower.parent.Position;
			GlowFlooder.attenLinearSlope = -1f / GlowFlooder.glower.Props.glowRadius;
			int num = Mathf.RoundToInt(GlowFlooder.glower.Props.glowRadius * 100f);
			IntVec3 intVec = default(IntVec3);
			IntVec3 c = default(IntVec3);
			int num2 = 0;
			GlowFlooder.openSet.Clear();
			int num3 = (position.z << (int)GlowFlooder.gridSizeZLog2) + position.x;
			GlowFlooder.calcGrid[num3].intDist = 100;
			GlowFlooder.openSet.Push(num3);
			while (GlowFlooder.openSet.Count != 0)
			{
				int num4 = GlowFlooder.openSet.Pop();
				intVec.x = (int)((ushort)(num4 & (int)GlowFlooder.gridSizeXMinus1));
				intVec.z = (int)((ushort)(num4 >> (int)GlowFlooder.gridSizeZLog2));
				GlowFlooder.calcGrid[num4].status = GlowFlooder.finalizedVal;
				GlowFlooder.SetGlowGridFromDist(intVec);
				if (UnityData.isDebugBuild && DebugViewSettings.drawGlow)
				{
					Find.DebugDrawer.FlashCell(intVec, (float)GlowFlooder.calcGrid[num4].intDist / 10f, GlowFlooder.calcGrid[num4].intDist.ToString("F2"));
					num2++;
				}
				for (int i = 0; i < 8; i++)
				{
					c.x = (int)((ushort)(intVec.x + (int)GlowFlooder.Directions[i, 0]));
					c.z = (int)((ushort)(intVec.z + (int)GlowFlooder.Directions[i, 1]));
					int num5 = (c.z << (int)GlowFlooder.gridSizeZLog2) + c.x;
					if (c.InBounds())
					{
						if (GlowFlooder.calcGrid[num5].status != GlowFlooder.finalizedVal)
						{
							GlowFlooder.blockers[i] = innerArray[CellIndices.CellToIndex(c)];
							if (GlowFlooder.blockers[i] != null)
							{
								if (GlowFlooder.blockers[i].def.blockLight)
								{
									goto IL_41E;
								}
								GlowFlooder.blockers[i] = null;
							}
							int num6;
							if (i < 4)
							{
								num6 = 100;
							}
							else
							{
								num6 = 141;
							}
							int num7 = GlowFlooder.calcGrid[num4].intDist + num6;
							if (num7 <= num)
							{
								if (GlowFlooder.calcGrid[num5].status != GlowFlooder.finalizedVal)
								{
									if (i >= 4)
									{
										bool flag = false;
										switch (i)
										{
										case 4:
											if (GlowFlooder.blockers[0] != null && GlowFlooder.blockers[1] != null)
											{
												flag = true;
											}
											break;
										case 5:
											if (GlowFlooder.blockers[1] != null && GlowFlooder.blockers[2] != null)
											{
												flag = true;
											}
											break;
										case 6:
											if (GlowFlooder.blockers[2] != null && GlowFlooder.blockers[3] != null)
											{
												flag = true;
											}
											break;
										case 7:
											if (GlowFlooder.blockers[0] != null && GlowFlooder.blockers[3] != null)
											{
												flag = true;
											}
											break;
										}
										if (flag)
										{
											goto IL_41E;
										}
									}
									if (GlowFlooder.calcGrid[num5].status <= GlowFlooder.unseenVal)
									{
										GlowFlooder.calcGrid[num5].intDist = 999999;
										GlowFlooder.calcGrid[num5].status = GlowFlooder.openVal;
									}
									if (num7 < GlowFlooder.calcGrid[num5].intDist)
									{
										GlowFlooder.calcGrid[num5].intDist = num7;
										GlowFlooder.calcGrid[num5].status = GlowFlooder.openVal;
										GlowFlooder.openSet.Push(num5);
									}
								}
							}
						}
					}
					IL_41E:;
				}
			}
		}

		private static void SetGlowGridFromDist(IntVec3 c)
		{
			GlowFlooder.finalIdx = (c.z << (int)GlowFlooder.gridSizeZLog2) + c.x;
			float num = (float)GlowFlooder.calcGrid[GlowFlooder.finalIdx].intDist / 100f;
			ColorInt colB = default(ColorInt);
			if (num <= GlowFlooder.glower.Props.glowRadius)
			{
				float b = 1f / (num * num);
				float a = 1f + GlowFlooder.attenLinearSlope * num;
				float b2 = Mathf.Lerp(a, b, 0.4f);
				colB = GlowFlooder.glower.Props.glowColor * b2;
			}
			if (colB.r > 0 || colB.g > 0 || colB.b > 0)
			{
				colB.ClampToNonNegative();
				int num2 = CellIndices.CellToIndex(c);
				ColorInt colA = GlowFlooder.glowGrid[num2].AsColorInt();
				colA += colB;
				if (num < GlowFlooder.glower.Props.overlightRadius)
				{
					colA.a = 1;
				}
				Color32 toColor = colA.ToColor32;
				GlowFlooder.glowGrid[num2] = toColor;
			}
		}
	}
}
