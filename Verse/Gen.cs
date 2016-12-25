using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class Gen
	{
		public static Vector3 TrueCenter(this Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				return pawn.Drawer.DrawPos;
			}
			return Gen.TrueCenter(t.Position, t.Rotation, t.def.size, t.def.Altitude);
		}

		public static Vector3 TrueCenter(IntVec3 loc, Rot4 rotation, IntVec2 thingSize, float altitude)
		{
			Vector3 result = loc.ToVector3ShiftedWithAltitude(altitude);
			if (thingSize.x != 1 || thingSize.z != 1)
			{
				if (rotation.IsHorizontal)
				{
					int x = thingSize.x;
					thingSize.x = thingSize.z;
					thingSize.z = x;
				}
				switch (rotation.AsInt)
				{
				case 0:
					if (thingSize.x % 2 == 0)
					{
						result.x += 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z += 0.5f;
					}
					break;
				case 1:
					if (thingSize.x % 2 == 0)
					{
						result.x += 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z -= 0.5f;
					}
					break;
				case 2:
					if (thingSize.x % 2 == 0)
					{
						result.x -= 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z -= 0.5f;
					}
					break;
				case 3:
					if (thingSize.x % 2 == 0)
					{
						result.x -= 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z += 0.5f;
					}
					break;
				}
			}
			return result;
		}

		public static Vector3 AveragePosition(List<IntVec3> cells)
		{
			return new Vector3((float)cells.Average((IntVec3 c) => c.x) + 0.5f, 0f, (float)cells.Average((IntVec3 c) => c.z) + 0.5f);
		}

		public static T RandomEnumValue<T>(bool disallowFirstValue)
		{
			int min = (!disallowFirstValue) ? 0 : 1;
			int num = Rand.Range(min, Enum.GetValues(typeof(T)).Length);
			return (T)((object)num);
		}

		public static Vector3 RandomHorizontalVector(float max)
		{
			return new Vector3(Rand.Range(-max, max), 0f, Rand.Range(-max, max));
		}

		public static int GetBitCountOf(long lValue)
		{
			int num = 0;
			while (lValue != 0L)
			{
				lValue &= lValue - 1L;
				num++;
			}
			return num;
		}

		[DebuggerHidden]
		public static IEnumerable<T> GetAllSelectedItems<T>(this Enum value)
		{
			int valueAsInt = Convert.ToInt32(value);
			foreach (object item in Enum.GetValues(typeof(T)))
			{
				int itemAsInt = Convert.ToInt32(item);
				if (itemAsInt == (valueAsInt & itemAsInt))
				{
					yield return (T)((object)item);
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<T> YieldSingle<T>(T val)
		{
			yield return val;
		}

		public static int HashCombine<T>(int seed, T obj)
		{
			int num = (obj != null) ? obj.GetHashCode() : 0;
			return (int)((long)seed ^ (long)num + (long)((ulong)-1640531527) + (long)((long)seed << 6) + (long)(seed >> 2));
		}

		public static int HashCombineStruct<T>(int seed, T obj) where T : struct
		{
			return (int)((long)seed ^ (long)obj.GetHashCode() + (long)((ulong)-1640531527) + (long)((long)seed << 6) + (long)(seed >> 2));
		}

		public static int HashCombineInt(int seed, int value)
		{
			return (int)((long)seed ^ (long)value + (long)((ulong)-1640531527) + (long)((long)seed << 6) + (long)(seed >> 2));
		}

		public static int HashOffset(this int baseInt)
		{
			return Gen.HashCombineInt(baseInt, 169495093);
		}

		public static int HashOffset(this Thing t)
		{
			return t.thingIDNumber.HashOffset();
		}

		public static int HashOffset(this WorldObject o)
		{
			return o.ID.HashOffset();
		}

		public static bool IsHashIntervalTick(this Thing t, int interval)
		{
			return (Find.TickManager.TicksGame + t.thingIDNumber.HashOffset()) % interval == 0;
		}

		public static int HashOffsetTicks(this Thing t, int interval)
		{
			return Find.TickManager.TicksGame + t.thingIDNumber.HashOffset();
		}

		public static bool IsHashIntervalTick(this WorldObject o, int interval)
		{
			return (Find.TickManager.TicksGame + o.ID.HashOffset()) % interval == 0;
		}
	}
}
