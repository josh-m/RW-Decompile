using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GlowFlooder
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

		private Map map;

		private GlowFlooder.GlowFloodCell[] calcGrid;

		private FastPriorityQueue<int> openSet;

		private uint unseenVal;

		private uint openVal = 1u;

		private uint finalizedVal = 2u;

		private int mapSizePowTwo;

		private ushort gridSizeX;

		private ushort gridSizeY;

		private ushort gridSizeXMinus1;

		private ushort gridSizeZLog2;

		private CompGlower glower;

		private float attenLinearSlope;

		private int finalIdx;

		private Thing[] blockers = new Thing[8];

		private Color32[] glowGrid;

		private static readonly sbyte[,] Directions = new sbyte[,]
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

		public GlowFlooder(Map map)
		{
			this.map = map;
		}

		private void InitializeWorkingData()
		{
			this.mapSizePowTwo = this.map.info.PowerOfTwoOverMapSize;
			this.gridSizeX = (ushort)this.mapSizePowTwo;
			this.gridSizeY = (ushort)this.mapSizePowTwo;
			this.gridSizeXMinus1 = this.gridSizeX - 1;
			this.gridSizeZLog2 = (ushort)Math.Log((double)this.gridSizeY, 2.0);
			if (this.calcGrid == null || this.calcGrid.Length != (int)(this.gridSizeX * this.gridSizeY))
			{
				this.calcGrid = new GlowFlooder.GlowFloodCell[(int)(this.gridSizeX * this.gridSizeY)];
			}
			this.openSet = new FastPriorityQueue<int>(new GlowFlooder.CompareGlowFlooderLightSquares(this.calcGrid));
		}

		public void AddFloodGlowFor(CompGlower theGlower)
		{
			if (this.calcGrid == null)
			{
				this.InitializeWorkingData();
			}
			this.glowGrid = this.map.glowGrid.glowGrid;
			this.glower = theGlower;
			Thing[] innerArray = this.map.edificeGrid.InnerArray;
			this.unseenVal += 3u;
			this.openVal += 3u;
			this.finalizedVal += 3u;
			IntVec3 position = this.glower.parent.Position;
			this.attenLinearSlope = -1f / this.glower.Props.glowRadius;
			int num = Mathf.RoundToInt(this.glower.Props.glowRadius * 100f);
			IntVec3 intVec = default(IntVec3);
			IntVec3 c = default(IntVec3);
			int num2 = 0;
			CellIndices cellIndices = this.map.cellIndices;
			this.openSet.Clear();
			int num3 = (position.z << (int)this.gridSizeZLog2) + position.x;
			this.calcGrid[num3].intDist = 100;
			this.openSet.Push(num3);
			while (this.openSet.Count != 0)
			{
				int num4 = this.openSet.Pop();
				intVec.x = (int)((ushort)(num4 & (int)this.gridSizeXMinus1));
				intVec.z = (int)((ushort)(num4 >> (int)this.gridSizeZLog2));
				this.calcGrid[num4].status = this.finalizedVal;
				this.SetGlowGridFromDist(intVec);
				if (UnityData.isDebugBuild && DebugViewSettings.drawGlow)
				{
					this.map.debugDrawer.FlashCell(intVec, (float)this.calcGrid[num4].intDist / 10f, this.calcGrid[num4].intDist.ToString("F2"));
					num2++;
				}
				for (int i = 0; i < 8; i++)
				{
					c.x = (int)((ushort)(intVec.x + (int)GlowFlooder.Directions[i, 0]));
					c.z = (int)((ushort)(intVec.z + (int)GlowFlooder.Directions[i, 1]));
					int num5 = (c.z << (int)this.gridSizeZLog2) + c.x;
					if (c.InBounds(this.map))
					{
						if (this.calcGrid[num5].status != this.finalizedVal)
						{
							this.blockers[i] = innerArray[cellIndices.CellToIndex(c)];
							if (this.blockers[i] != null)
							{
								if (this.blockers[i].def.blockLight)
								{
									goto IL_47C;
								}
								this.blockers[i] = null;
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
							int num7 = this.calcGrid[num4].intDist + num6;
							if (num7 <= num)
							{
								if (this.calcGrid[num5].status != this.finalizedVal)
								{
									if (i >= 4)
									{
										bool flag = false;
										switch (i)
										{
										case 4:
											if (this.blockers[0] != null && this.blockers[1] != null)
											{
												flag = true;
											}
											break;
										case 5:
											if (this.blockers[1] != null && this.blockers[2] != null)
											{
												flag = true;
											}
											break;
										case 6:
											if (this.blockers[2] != null && this.blockers[3] != null)
											{
												flag = true;
											}
											break;
										case 7:
											if (this.blockers[0] != null && this.blockers[3] != null)
											{
												flag = true;
											}
											break;
										}
										if (flag)
										{
											goto IL_47C;
										}
									}
									if (this.calcGrid[num5].status <= this.unseenVal)
									{
										this.calcGrid[num5].intDist = 999999;
										this.calcGrid[num5].status = this.openVal;
									}
									if (num7 < this.calcGrid[num5].intDist)
									{
										this.calcGrid[num5].intDist = num7;
										this.calcGrid[num5].status = this.openVal;
										this.openSet.Push(num5);
									}
								}
							}
						}
					}
					IL_47C:;
				}
			}
		}

		private void SetGlowGridFromDist(IntVec3 c)
		{
			this.finalIdx = (c.z << (int)this.gridSizeZLog2) + c.x;
			float num = (float)this.calcGrid[this.finalIdx].intDist / 100f;
			ColorInt colB = default(ColorInt);
			if (num <= this.glower.Props.glowRadius)
			{
				float b = 1f / (num * num);
				float a = 1f + this.attenLinearSlope * num;
				float b2 = Mathf.Lerp(a, b, 0.4f);
				colB = this.glower.Props.glowColor * b2;
			}
			if (colB.r > 0 || colB.g > 0 || colB.b > 0)
			{
				colB.ClampToNonNegative();
				int num2 = this.map.cellIndices.CellToIndex(c);
				ColorInt colA = this.glowGrid[num2].AsColorInt();
				colA += colB;
				if (num < this.glower.Props.overlightRadius)
				{
					colA.a = 1;
				}
				Color32 toColor = colA.ToColor32;
				this.glowGrid[num2] = toColor;
			}
		}
	}
}
