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

		public static float CurCelestialSunGlow()
		{
			return GenCelestial.CelestialSunGlowPercent(Find.World.LongLatOf(Find.Map.WorldCoords).y, GenDate.DayOfYear, GenDate.CurrentDayPercent);
		}

		public static float CurShadowStrength()
		{
			return Mathf.Clamp01(Mathf.Abs(GenCelestial.CurCelestialSunGlow() - 0.6f) / 0.15f);
		}

		public static Vector2 CurShadowVector()
		{
			float currentDayPercent = GenDate.CurrentDayPercent;
			bool flag = GenCelestial.CurCelestialSunGlow() > 0.6f;
			float t;
			float num;
			float num2;
			if (flag)
			{
				t = currentDayPercent;
				num = -1.5f;
				num2 = 15f;
			}
			else
			{
				if (currentDayPercent > 0.5f)
				{
					t = Mathf.InverseLerp(0.5f, 1f, currentDayPercent) * 0.5f;
				}
				else
				{
					t = 0.5f + Mathf.InverseLerp(0f, 0.5f, currentDayPercent) * 0.5f;
				}
				num = -0.9f;
				num2 = 15f;
			}
			float num3 = Mathf.Lerp(num2, -num2, t);
			float y = num - 2.5f * (num3 * num3 / 100f);
			return new Vector2(num3, y);
		}

		private static float CelestialSunGlowPercent(float latitude, int dayOfYear, float dayPercent)
		{
			Vector3 vector = new Vector3(1f, 0f, 0f);
			vector = Quaternion.AngleAxis(latitude, new Vector3(0f, 0f, 1f)) * vector;
			Vector3 vector2 = new Vector3(100f, 0f, 0f);
			float num = (float)dayOfYear / 60f;
			float f = num * 3.14159274f * 2f;
			float num2 = -Mathf.Cos(f);
			vector2.y += num2 * 20f;
			float angle = (dayPercent - 0.5f) * 360f;
			vector2 = Quaternion.AngleAxis(angle, Vector3.up) * vector2;
			vector2 = Vector3.RotateTowards(vector2, vector, 0.331612557f, 9999999f);
			float num3 = Mathf.InverseLerp(60f, 0f, latitude);
			if (num3 > 0f)
			{
				vector2 = Vector3.RotateTowards(vector2, vector, 6.28318548f * (17f * num3 / 360f), 9999999f);
			}
			float value = Vector3.Dot(vector.normalized, vector2.normalized);
			float value2 = Mathf.InverseLerp(0f, 0.7f, value);
			return Mathf.Clamp01(value2);
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
