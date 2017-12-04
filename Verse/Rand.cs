using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class Rand
	{
		private static Stack<ulong> stateStack = new Stack<ulong>();

		private static RandomNumberGenerator random = new RandomNumberGenerator_BasicHash();

		private static uint iterations = 0u;

		private static List<int> tmpRange = new List<int>();

		public static int Seed
		{
			set
			{
				Rand.random.seed = (uint)value;
				Rand.iterations = 0u;
			}
		}

		public static float Value
		{
			get
			{
				return Rand.random.GetFloat(Rand.iterations++);
			}
		}

		public static bool Bool
		{
			get
			{
				return Rand.Value < 0.5f;
			}
		}

		public static int Int
		{
			get
			{
				return Rand.random.GetInt(Rand.iterations++);
			}
		}

		public static Vector3 UnitVector3
		{
			get
			{
				Vector3 vector = new Vector3(Rand.Gaussian(0f, 1f), Rand.Gaussian(0f, 1f), Rand.Gaussian(0f, 1f));
				return vector.normalized;
			}
		}

		public static Vector2 UnitVector2
		{
			get
			{
				Vector2 vector = new Vector2(Rand.Gaussian(0f, 1f), Rand.Gaussian(0f, 1f));
				return vector.normalized;
			}
		}

		public static Vector3 PointOnDisc
		{
			get
			{
				Vector3 result;
				do
				{
					result = new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f) * 2f;
				}
				while (result.sqrMagnitude > 1f);
				return result;
			}
		}

		private static ulong StateCompressed
		{
			get
			{
				return (ulong)Rand.random.seed | (ulong)Rand.iterations << 32;
			}
			set
			{
				Rand.random.seed = (uint)(value & (ulong)-1);
				Rand.iterations = (uint)(value >> 32 & (ulong)-1);
			}
		}

		public static void EnsureStateStackEmpty()
		{
			if (Rand.stateStack.Any<ulong>())
			{
				Log.Warning("Random state stack is not empty. There were more calls to PushState than PopState. Fixing.");
				Rand.stateStack.Clear();
			}
		}

		public static float Gaussian(float centerX = 0f, float widthFactor = 1f)
		{
			float value = Rand.Value;
			float value2 = Rand.Value;
			float num = Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin(6.28318548f * value2);
			return num * widthFactor + centerX;
		}

		public static float GaussianAsymmetric(float centerX = 0f, float lowerWidthFactor = 1f, float upperWidthFactor = 1f)
		{
			float value = Rand.Value;
			float value2 = Rand.Value;
			float num = Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin(6.28318548f * value2);
			if (num <= 0f)
			{
				return num * lowerWidthFactor + centerX;
			}
			return num * upperWidthFactor + centerX;
		}

		public static void RandomizeStateFromTime()
		{
			Rand.Seed = DateTime.Now.GetHashCode();
		}

		public static int Range(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return min + Mathf.Abs(Rand.random.GetInt(Rand.iterations++) % (max - min));
		}

		public static int RangeInclusive(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return Rand.Range(min, max + 1);
		}

		public static float Range(float min, float max)
		{
			if (max <= min)
			{
				return min;
			}
			return Rand.Value * (max - min) + min;
		}

		public static bool Chance(float chance)
		{
			return chance > 0f && (chance >= 1f || Rand.Value < chance);
		}

		public static bool ChanceSeeded(float chance, int specialSeed)
		{
			ulong stateCompressed = Rand.StateCompressed;
			Rand.Seed = specialSeed;
			bool result = Rand.Chance(chance);
			Rand.StateCompressed = stateCompressed;
			return result;
		}

		public static float ValueSeeded(int specialSeed)
		{
			ulong stateCompressed = Rand.StateCompressed;
			Rand.Seed = specialSeed;
			float value = Rand.Value;
			Rand.StateCompressed = stateCompressed;
			return value;
		}

		public static T Element<T>(T a, T b)
		{
			return (!Rand.Bool) ? b : a;
		}

		public static T Element<T>(T a, T b, T c)
		{
			float value = Rand.Value;
			if (value < 0.33333f)
			{
				return a;
			}
			if (value < 0.66666f)
			{
				return b;
			}
			return c;
		}

		public static T Element<T>(T a, T b, T c, T d)
		{
			float value = Rand.Value;
			if (value < 0.25f)
			{
				return a;
			}
			if (value < 0.5f)
			{
				return b;
			}
			if (value < 0.75f)
			{
				return c;
			}
			return d;
		}

		public static T Element<T>(T a, T b, T c, T d, T e)
		{
			float value = Rand.Value;
			if (value < 0.2f)
			{
				return a;
			}
			if (value < 0.4f)
			{
				return b;
			}
			if (value < 0.6f)
			{
				return c;
			}
			if (value < 0.8f)
			{
				return d;
			}
			return e;
		}

		public static T Element<T>(T a, T b, T c, T d, T e, T f)
		{
			float value = Rand.Value;
			if (value < 0.16666f)
			{
				return a;
			}
			if (value < 0.33333f)
			{
				return b;
			}
			if (value < 0.5f)
			{
				return c;
			}
			if (value < 0.66666f)
			{
				return d;
			}
			if (value < 0.83333f)
			{
				return e;
			}
			return f;
		}

		public static void PushState()
		{
			Rand.stateStack.Push(Rand.StateCompressed);
		}

		public static void PushState(int replacementSeed)
		{
			Rand.PushState();
			Rand.Seed = replacementSeed;
		}

		public static void PopState()
		{
			Rand.StateCompressed = Rand.stateStack.Pop();
		}

		public static float ByCurve(SimpleCurve curve, int sampleCount = 100)
		{
			if (curve.PointsCount < 3)
			{
				throw new ArgumentException("curve has < 3 points");
			}
			if (curve[0].y > 0f || curve[curve.PointsCount - 1].y > 0f)
			{
				throw new ArgumentException("curve has start/end point with y > 0");
			}
			float x = curve[0].x;
			float x2 = curve[curve.PointsCount - 1].x;
			float num = (x2 - x) / (float)sampleCount;
			float num2 = 0f;
			for (int i = 0; i < sampleCount; i++)
			{
				float x3 = x + ((float)i + 0.5f) * num;
				float num3 = curve.Evaluate(x3);
				num2 += num3;
			}
			float num4 = Rand.Range(0f, num2);
			num2 = 0f;
			for (int j = 0; j < sampleCount; j++)
			{
				float num5 = x + ((float)j + 0.5f) * num;
				float num6 = curve.Evaluate(num5);
				num2 += num6;
				if (num2 > num4)
				{
					return num5 + Rand.Range(-num / 2f, num / 2f);
				}
			}
			throw new Exception("Reached end of Rand.ByCurve without choosing a point.");
		}

		public static bool MTBEventOccurs(float mtb, float mtbUnit, float checkDuration)
		{
			if (mtb == float.PositiveInfinity)
			{
				return false;
			}
			if (mtb <= 0f)
			{
				Log.Error("MTBEventOccurs with mtb=" + mtb);
				return true;
			}
			if (mtbUnit <= 0f)
			{
				Log.Error("MTBEventOccurs with mtbUnit=" + mtbUnit);
				return false;
			}
			if (checkDuration <= 0f)
			{
				Log.Error("MTBEventOccurs with checkDuration=" + checkDuration);
				return false;
			}
			double num = (double)checkDuration / ((double)mtb * (double)mtbUnit);
			if (num <= 0.0)
			{
				Log.Error(string.Concat(new object[]
				{
					"chancePerCheck is ",
					num,
					". mtb=",
					mtb,
					", mtbUnit=",
					mtbUnit,
					", checkDuration=",
					checkDuration
				}));
				return false;
			}
			double num2 = 1.0;
			if (num < 0.0001)
			{
				while (num < 0.0001)
				{
					num *= 8.0;
					num2 /= 8.0;
				}
				if ((double)Rand.Value > num2)
				{
					return false;
				}
			}
			return (double)Rand.Value < num;
		}

		internal static void LogRandTests()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int @int = Rand.Int;
			stringBuilder.AppendLine("Repeating single ValueSeeded with seed " + @int + ". This should give the same result:");
			for (int i = 0; i < 4; i++)
			{
				stringBuilder.AppendLine("   " + Rand.ValueSeeded(@int));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Long-term tests");
			for (int j = 0; j < 3; j++)
			{
				int num = 0;
				for (int k = 0; k < 5000000; k++)
				{
					if (Rand.MTBEventOccurs(250f, 60000f, 60f))
					{
						num++;
					}
				}
				string value = string.Concat(new object[]
				{
					"MTB=",
					250,
					" days, MTBUnit=",
					60000,
					", check duration=",
					60,
					" Simulated ",
					5000,
					" days (",
					5000000,
					" tests). Got ",
					num,
					" events."
				});
				stringBuilder.AppendLine(value);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Short-term tests");
			for (int l = 0; l < 5; l++)
			{
				int num2 = 0;
				for (int m = 0; m < 10000; m++)
				{
					if (Rand.MTBEventOccurs(1f, 24000f, 12000f))
					{
						num2++;
					}
				}
				string value2 = string.Concat(new object[]
				{
					"MTB=",
					1f,
					" days, MTBUnit=",
					24000f,
					", check duration=",
					12000f,
					", ",
					10000,
					" tests got ",
					num2,
					" events."
				});
				stringBuilder.AppendLine(value2);
			}
			for (int n = 0; n < 5; n++)
			{
				int num3 = 0;
				for (int num4 = 0; num4 < 10000; num4++)
				{
					if (Rand.MTBEventOccurs(2f, 24000f, 6000f))
					{
						num3++;
					}
				}
				string value3 = string.Concat(new object[]
				{
					"MTB=",
					2f,
					" days, MTBUnit=",
					24000f,
					", check duration=",
					6000f,
					", ",
					10000,
					" tests got ",
					num3,
					" events."
				});
				stringBuilder.AppendLine(value3);
			}
			Log.Message(stringBuilder.ToString());
		}

		public static int RandSeedForHour(this Thing t, int salt)
		{
			int seed = t.HashOffset();
			seed = Gen.HashCombineInt(seed, Find.TickManager.TicksAbs / 2500);
			return Gen.HashCombineInt(seed, salt);
		}

		public static bool TryRangeInclusiveWhere(int from, int to, Predicate<int> predicate, out int value)
		{
			int num = to - from + 1;
			if (num <= 0)
			{
				value = 0;
				return false;
			}
			int num2 = Mathf.Max(Mathf.RoundToInt(Mathf.Sqrt((float)num)), 5);
			for (int i = 0; i < num2; i++)
			{
				int num3 = Rand.RangeInclusive(from, to);
				if (predicate(num3))
				{
					value = num3;
					return true;
				}
			}
			Rand.tmpRange.Clear();
			for (int j = from; j <= to; j++)
			{
				Rand.tmpRange.Add(j);
			}
			Rand.tmpRange.Shuffle<int>();
			int k = 0;
			int count = Rand.tmpRange.Count;
			while (k < count)
			{
				if (predicate(Rand.tmpRange[k]))
				{
					value = Rand.tmpRange[k];
					return true;
				}
				k++;
			}
			value = 0;
			return false;
		}

		public static Vector3 PointOnSphereCap(Vector3 center, float angle)
		{
			if (angle <= 0f)
			{
				return center;
			}
			if (angle >= 180f)
			{
				return Rand.UnitVector3;
			}
			float num = Rand.Range(Mathf.Cos(angle * 0.0174532924f), 1f);
			float f = Rand.Range(0f, 6.28318548f);
			Vector3 point = new Vector3(Mathf.Sqrt(1f - num * num) * Mathf.Cos(f), Mathf.Sqrt(1f - num * num) * Mathf.Sin(f), num);
			return Quaternion.FromToRotation(Vector3.forward, center) * point;
		}
	}
}
