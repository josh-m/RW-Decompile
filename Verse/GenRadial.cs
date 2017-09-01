using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public static class GenRadial
	{
		private const int RadialPatternCount = 10000;

		public static IntVec3[] ManualRadialPattern;

		public static IntVec3[] RadialPattern;

		private static float[] RadialPatternRadii;

		public static float MaxRadialPatternRadius
		{
			get
			{
				return GenRadial.RadialPatternRadii[GenRadial.RadialPatternRadii.Length - 1];
			}
		}

		static GenRadial()
		{
			GenRadial.ManualRadialPattern = new IntVec3[49];
			GenRadial.RadialPattern = new IntVec3[10000];
			GenRadial.RadialPatternRadii = new float[10000];
			GenRadial.SetupManualRadialPattern();
			GenRadial.SetupRadialPattern();
		}

		private static void SetupManualRadialPattern()
		{
			GenRadial.ManualRadialPattern[0] = new IntVec3(0, 0, 0);
			GenRadial.ManualRadialPattern[1] = new IntVec3(0, 0, -1);
			GenRadial.ManualRadialPattern[2] = new IntVec3(1, 0, 0);
			GenRadial.ManualRadialPattern[3] = new IntVec3(0, 0, 1);
			GenRadial.ManualRadialPattern[4] = new IntVec3(-1, 0, 0);
			GenRadial.ManualRadialPattern[5] = new IntVec3(1, 0, -1);
			GenRadial.ManualRadialPattern[6] = new IntVec3(1, 0, 1);
			GenRadial.ManualRadialPattern[7] = new IntVec3(-1, 0, 1);
			GenRadial.ManualRadialPattern[8] = new IntVec3(-1, 0, -1);
			GenRadial.ManualRadialPattern[9] = new IntVec3(2, 0, 0);
			GenRadial.ManualRadialPattern[10] = new IntVec3(-2, 0, 0);
			GenRadial.ManualRadialPattern[11] = new IntVec3(0, 0, 2);
			GenRadial.ManualRadialPattern[12] = new IntVec3(0, 0, -2);
			GenRadial.ManualRadialPattern[13] = new IntVec3(2, 0, 1);
			GenRadial.ManualRadialPattern[14] = new IntVec3(2, 0, -1);
			GenRadial.ManualRadialPattern[15] = new IntVec3(-2, 0, 1);
			GenRadial.ManualRadialPattern[16] = new IntVec3(-2, 0, -1);
			GenRadial.ManualRadialPattern[17] = new IntVec3(-1, 0, 2);
			GenRadial.ManualRadialPattern[18] = new IntVec3(1, 0, 2);
			GenRadial.ManualRadialPattern[19] = new IntVec3(-1, 0, -2);
			GenRadial.ManualRadialPattern[20] = new IntVec3(1, 0, -2);
			GenRadial.ManualRadialPattern[21] = new IntVec3(2, 0, 2);
			GenRadial.ManualRadialPattern[22] = new IntVec3(-2, 0, -2);
			GenRadial.ManualRadialPattern[23] = new IntVec3(2, 0, -2);
			GenRadial.ManualRadialPattern[24] = new IntVec3(-2, 0, 2);
			GenRadial.ManualRadialPattern[25] = new IntVec3(3, 0, 0);
			GenRadial.ManualRadialPattern[26] = new IntVec3(0, 0, 3);
			GenRadial.ManualRadialPattern[27] = new IntVec3(-3, 0, 0);
			GenRadial.ManualRadialPattern[28] = new IntVec3(0, 0, -3);
			GenRadial.ManualRadialPattern[29] = new IntVec3(3, 0, 1);
			GenRadial.ManualRadialPattern[30] = new IntVec3(-3, 0, -1);
			GenRadial.ManualRadialPattern[31] = new IntVec3(1, 0, 3);
			GenRadial.ManualRadialPattern[32] = new IntVec3(-1, 0, -3);
			GenRadial.ManualRadialPattern[33] = new IntVec3(-3, 0, 1);
			GenRadial.ManualRadialPattern[34] = new IntVec3(3, 0, -1);
			GenRadial.ManualRadialPattern[35] = new IntVec3(-1, 0, 3);
			GenRadial.ManualRadialPattern[36] = new IntVec3(1, 0, -3);
			GenRadial.ManualRadialPattern[37] = new IntVec3(3, 0, 2);
			GenRadial.ManualRadialPattern[38] = new IntVec3(-3, 0, -2);
			GenRadial.ManualRadialPattern[39] = new IntVec3(2, 0, 3);
			GenRadial.ManualRadialPattern[40] = new IntVec3(-2, 0, -3);
			GenRadial.ManualRadialPattern[41] = new IntVec3(-3, 0, 2);
			GenRadial.ManualRadialPattern[42] = new IntVec3(3, 0, -2);
			GenRadial.ManualRadialPattern[43] = new IntVec3(-2, 0, 3);
			GenRadial.ManualRadialPattern[44] = new IntVec3(2, 0, -3);
			GenRadial.ManualRadialPattern[45] = new IntVec3(3, 0, 3);
			GenRadial.ManualRadialPattern[46] = new IntVec3(3, 0, -3);
			GenRadial.ManualRadialPattern[47] = new IntVec3(-3, 0, 3);
			GenRadial.ManualRadialPattern[48] = new IntVec3(-3, 0, -3);
		}

		private static void SetupRadialPattern()
		{
			List<IntVec3> list = new List<IntVec3>();
			for (int i = -60; i < 60; i++)
			{
				for (int j = -60; j < 60; j++)
				{
					list.Add(new IntVec3(i, 0, j));
				}
			}
			list.Sort(delegate(IntVec3 A, IntVec3 B)
			{
				float num = (float)A.LengthHorizontalSquared;
				float num2 = (float)B.LengthHorizontalSquared;
				if (num < num2)
				{
					return -1;
				}
				if (num == num2)
				{
					return 0;
				}
				return 1;
			});
			for (int k = 0; k < 10000; k++)
			{
				GenRadial.RadialPattern[k] = list[k];
				GenRadial.RadialPatternRadii[k] = list[k].LengthHorizontal;
			}
		}

		public static int NumCellsToFillForRadius_ManualRadialPattern(int radius)
		{
			if (radius == 0)
			{
				return 1;
			}
			if (radius == 1)
			{
				return 9;
			}
			if (radius == 2)
			{
				return 21;
			}
			if (radius == 3)
			{
				return 37;
			}
			Log.Error("NumSquares radius error");
			return 0;
		}

		public static int NumCellsInRadius(float radius)
		{
			if (radius >= GenRadial.MaxRadialPatternRadius)
			{
				Log.Error(string.Concat(new object[]
				{
					"Not enough squares to get to radius ",
					radius,
					". Max is ",
					GenRadial.MaxRadialPatternRadius
				}));
				return 10000;
			}
			float num = radius + 1.401298E-45f;
			for (int i = 0; i < 10000; i++)
			{
				if (GenRadial.RadialPatternRadii[i] > num)
				{
					return i;
				}
			}
			return 10000;
		}

		public static float RadiusOfNumCells(int numCells)
		{
			return GenRadial.RadialPatternRadii[numCells];
		}

		[DebuggerHidden]
		public static IEnumerable<IntVec3> RadialPatternInRadius(float radius)
		{
			int numSquares = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < numSquares; i++)
			{
				yield return GenRadial.RadialPattern[i];
			}
		}

		[DebuggerHidden]
		public static IEnumerable<IntVec3> RadialCellsAround(IntVec3 center, float radius, bool useCenter)
		{
			int numSquares = GenRadial.NumCellsInRadius(radius);
			for (int i = (!useCenter) ? 1 : 0; i < numSquares; i++)
			{
				yield return GenRadial.RadialPattern[i] + center;
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Thing> RadialDistinctThingsAround(IntVec3 center, Map map, float radius, bool useCenter)
		{
			int numCells = GenRadial.NumCellsInRadius(radius);
			HashSet<Thing> returnedThings = null;
			for (int i = (!useCenter) ? 1 : 0; i < numCells; i++)
			{
				IntVec3 cell = GenRadial.RadialPattern[i] + center;
				if (cell.InBounds(map))
				{
					List<Thing> thingList = cell.GetThingList(map);
					int j = 0;
					while (j < thingList.Count)
					{
						Thing t = thingList[j];
						if (t.def.size.x <= 1 || t.def.size.z <= 1)
						{
							goto IL_14A;
						}
						if (returnedThings == null)
						{
							returnedThings = new HashSet<Thing>();
						}
						if (!returnedThings.Contains(t))
						{
							returnedThings.Add(t);
							goto IL_14A;
						}
						IL_162:
						j++;
						continue;
						IL_14A:
						yield return t;
						goto IL_162;
					}
				}
			}
		}
	}
}
