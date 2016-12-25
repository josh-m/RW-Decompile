using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenCelestial
	{
		public const float ShadowMaxLengthDay = 15f;

		public const float ShadowMaxLengthNight = 15f;

		private const float ShadowGlowLerpSpan = 0.15f;

		private const float ShadowDayNightThreshold = 0.6f;

		public static float CurCelestialSunGlow(Map map)
		{
			return GenCelestial.CelestialSunGlowPercent(Find.WorldGrid.LongLatOf(map.Tile).y, GenLocalDate.DayOfYear(map), GenLocalDate.DayPercent(map));
		}

		public static float CurShadowStrength(Map map)
		{
			return Mathf.Clamp01(Mathf.Abs(GenCelestial.CurCelestialSunGlow(map) - 0.6f) / 0.15f);
		}

		public static Vector2 CurShadowVector(Map map)
		{
			float num = GenLocalDate.DayPercent(map);
			bool flag = GenCelestial.CurCelestialSunGlow(map) > 0.6f;
			float t;
			float num2;
			float num3;
			if (flag)
			{
				t = num;
				num2 = -1.5f;
				num3 = 15f;
			}
			else
			{
				if (num > 0.5f)
				{
					t = Mathf.InverseLerp(0.5f, 1f, num) * 0.5f;
				}
				else
				{
					t = 0.5f + Mathf.InverseLerp(0f, 0.5f, num) * 0.5f;
				}
				num2 = -0.9f;
				num3 = 15f;
			}
			float num4 = Mathf.Lerp(num3, -num3, t);
			float y = num2 - 2.5f * (num4 * num4 / 100f);
			return new Vector2(num4, y);
		}

		public static Vector3 CurSunPositionInWorldSpace()
		{
			return GenCelestial.SunPosition((float)GenDate.DayOfYear((long)GenTicks.TicksAbs, 0f), GenDate.DayPercent((long)GenTicks.TicksAbs, 0f), new Vector3(0f, 0f, -1f));
		}

		private static Vector3 SunPosition(float latitude, int dayOfYear, float dayPercent)
		{
			latitude = Mathf.Abs(latitude);
			Vector3 target = GenCelestial.SurfaceNormal(latitude);
			Vector3 current = GenCelestial.SunPosition((float)dayOfYear, dayPercent, new Vector3(1f, 0f, 0f));
			current = Vector3.RotateTowards(current, target, 0.331612557f, 9999999f);
			float num = Mathf.InverseLerp(60f, 0f, latitude);
			if (num > 0f)
			{
				current = Vector3.RotateTowards(current, target, 6.28318548f * (17f * num / 360f), 9999999f);
			}
			return current.normalized;
		}

		private static Vector3 SunPosition(float dayOfYear, float dayPercent, Vector3 initialSunPos)
		{
			Vector3 point = initialSunPos * 100f;
			float num = dayOfYear / 60f;
			float f = num * 3.14159274f * 2f;
			float num2 = -Mathf.Cos(f);
			point.y += num2 * 20f;
			float angle = (dayPercent - 0.5f) * 360f;
			point = Quaternion.AngleAxis(angle, Vector3.up) * point;
			return point.normalized;
		}

		private static float CelestialSunGlowPercent(float latitude, int dayOfYear, float dayPercent)
		{
			latitude = Mathf.Abs(latitude);
			Vector3 vector = GenCelestial.SurfaceNormal(latitude);
			Vector3 rhs = GenCelestial.SunPosition(latitude, dayOfYear, dayPercent);
			float value = Vector3.Dot(vector.normalized, rhs);
			float value2 = Mathf.InverseLerp(0f, 0.7f, value);
			return Mathf.Clamp01(value2);
		}

		private static Vector3 SurfaceNormal(float latitude)
		{
			Vector3 vector = new Vector3(1f, 0f, 0f);
			vector = Quaternion.AngleAxis(latitude, new Vector3(0f, 0f, 1f)) * vector;
			return vector;
		}

		public static void LogSunGlowForYear()
		{
			for (int i = 0; i <= 90; i += 10)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Sun visibility percents for latitude " + i + ", for each hour of each day of the year");
				stringBuilder.AppendLine("---------------------------------------");
				stringBuilder.Append("Day/hr".PadRight(6));
				for (int j = 0; j < 24; j += 2)
				{
					stringBuilder.Append((j.ToString() + "h").PadRight(6));
				}
				stringBuilder.AppendLine();
				for (int k = 0; k < 60; k += 5)
				{
					stringBuilder.Append(k.ToString().PadRight(6));
					for (int l = 0; l < 24; l += 2)
					{
						stringBuilder.Append(GenCelestial.CelestialSunGlowPercent((float)i, k, (float)l / 24f).ToString("F3").PadRight(6));
					}
					stringBuilder.AppendLine();
				}
				Log.Message(stringBuilder.ToString());
			}
		}
	}
}
