using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PawnSkinColors
	{
		private struct SkinColorData
		{
			public float whiteness;

			public float selector;

			public Color color;

			public SkinColorData(float whiteness, float selector, Color color)
			{
				this.whiteness = whiteness;
				this.selector = selector;
				this.color = color;
			}
		}

		private static readonly PawnSkinColors.SkinColorData[] SkinColors = new PawnSkinColors.SkinColorData[]
		{
			new PawnSkinColors.SkinColorData(0f, 0f, new Color(0.3882353f, 0.274509817f, 0.141176477f)),
			new PawnSkinColors.SkinColorData(0.1f, 0.05f, new Color(0.509803951f, 0.356862754f, 0.1882353f)),
			new PawnSkinColors.SkinColorData(0.25f, 0.2f, new Color(0.894117653f, 0.619607866f, 0.3529412f)),
			new PawnSkinColors.SkinColorData(0.5f, 0.285f, new Color(1f, 0.9372549f, 0.7411765f)),
			new PawnSkinColors.SkinColorData(0.75f, 0.785f, new Color(1f, 0.9372549f, 0.8352941f)),
			new PawnSkinColors.SkinColorData(1f, 1f, new Color(0.9490196f, 0.929411769f, 0.8784314f))
		};

		public static bool IsDarkSkin(Color color)
		{
			Color skinColor = PawnSkinColors.GetSkinColor(0.5f);
			return color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
		}

		public static Color GetSkinColor(float skinWhiteness)
		{
			int skinDataLeftIndexByWhiteness = PawnSkinColors.GetSkinDataLeftIndexByWhiteness(skinWhiteness);
			if (skinDataLeftIndexByWhiteness == PawnSkinColors.SkinColors.Length - 1)
			{
				return PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness].color;
			}
			float t = Mathf.InverseLerp(PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness].whiteness, PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness + 1].whiteness, skinWhiteness);
			return Color.Lerp(PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness].color, PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness + 1].color, t);
		}

		public static float RandomSkinWhiteness()
		{
			float value = Rand.Value;
			int num = 0;
			for (int i = 0; i < PawnSkinColors.SkinColors.Length; i++)
			{
				if (value < PawnSkinColors.SkinColors[i].selector)
				{
					break;
				}
				num = i;
			}
			if (num == PawnSkinColors.SkinColors.Length - 1)
			{
				return PawnSkinColors.SkinColors[num].whiteness;
			}
			float t = Mathf.InverseLerp(PawnSkinColors.SkinColors[num].selector, PawnSkinColors.SkinColors[num + 1].selector, value);
			return Mathf.Lerp(PawnSkinColors.SkinColors[num].whiteness, PawnSkinColors.SkinColors[num + 1].whiteness, t);
		}

		public static float GetWhitenessCommonalityFactor(float skinWhiteness)
		{
			int skinDataLeftIndexByWhiteness = PawnSkinColors.GetSkinDataLeftIndexByWhiteness(skinWhiteness);
			if (skinDataLeftIndexByWhiteness == PawnSkinColors.SkinColors.Length - 1)
			{
				return PawnSkinColors.GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness);
			}
			float t = Mathf.InverseLerp(PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness].whiteness, PawnSkinColors.SkinColors[skinDataLeftIndexByWhiteness + 1].whiteness, skinWhiteness);
			return Mathf.Lerp(PawnSkinColors.GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness), PawnSkinColors.GetSkinCommonalityFactor(skinDataLeftIndexByWhiteness + 1), t);
		}

		public static float GetRandomSkinColorSimilarTo(float value, float clampMin = 0f, float clampMax = 1f)
		{
			return Mathf.Clamp01(Mathf.Clamp(Rand.Gaussian(value, 0.05f), clampMin, clampMax));
		}

		private static float GetSkinCommonalityFactor(int skinDataIndex)
		{
			float num = 0f;
			for (int i = 0; i < PawnSkinColors.SkinColors.Length; i++)
			{
				num = Mathf.Max(num, PawnSkinColors.GetTotalAreaWhereClosestToSelector(i));
			}
			return PawnSkinColors.GetTotalAreaWhereClosestToSelector(skinDataIndex) / num;
		}

		private static float GetTotalAreaWhereClosestToSelector(int skinDataIndex)
		{
			float num = 0f;
			if (skinDataIndex == 0)
			{
				num += PawnSkinColors.SkinColors[skinDataIndex].selector;
			}
			else if (PawnSkinColors.SkinColors.Length > 1)
			{
				num += (PawnSkinColors.SkinColors[skinDataIndex].selector - PawnSkinColors.SkinColors[skinDataIndex - 1].selector) / 2f;
			}
			if (skinDataIndex == PawnSkinColors.SkinColors.Length - 1)
			{
				num += 1f - PawnSkinColors.SkinColors[skinDataIndex].selector;
			}
			else if (PawnSkinColors.SkinColors.Length > 1)
			{
				num += (PawnSkinColors.SkinColors[skinDataIndex + 1].selector - PawnSkinColors.SkinColors[skinDataIndex].selector) / 2f;
			}
			return num;
		}

		private static int GetSkinDataLeftIndexByWhiteness(float skinWhiteness)
		{
			int result = 0;
			for (int i = 0; i < PawnSkinColors.SkinColors.Length; i++)
			{
				if (skinWhiteness < PawnSkinColors.SkinColors[i].whiteness)
				{
					break;
				}
				result = i;
			}
			return result;
		}
	}
}
