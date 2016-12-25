using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class TexUI
	{
		public static readonly Texture2D HighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

		public static readonly Texture2D HighlightSelectedTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.94f, 0.5f, 0.18f));

		public static readonly Texture2D ArrowTexRight = ContentFinder<Texture2D>.Get("UI/Widgets/ArrowRight", true);

		public static readonly Texture2D ArrowTexLeft = ContentFinder<Texture2D>.Get("UI/Widgets/ArrowLeft", true);

		public static readonly Texture2D WinExpandWidget = ContentFinder<Texture2D>.Get("UI/Widgets/WinExpandWidget", true);

		public static readonly Texture2D ArrowTex = ContentFinder<Texture2D>.Get("UI/Misc/AlertFlashArrow", true);

		public static readonly Texture2D RotLeftTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft", true);

		public static readonly Texture2D RotRightTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight", true);

		public static readonly Texture2D AlphaAddTex = ContentFinder<Texture2D>.Get("Other/RoughAlphaAdd", true);

		public static readonly Texture2D GrayBg;

		public static readonly Color AvailResearchColor;

		public static readonly Color ActiveResearchColor;

		public static readonly Color FinishedResearchColor;

		public static readonly Color LockedResearchColor;

		public static readonly Color RelatedResearchColor;

		public static readonly Color HighlightBgResearchColor;

		public static readonly Color HighlightBorderResearchColor;

		public static readonly Color DefaultBorderResearchColor;

		public static readonly Color DefaultLineResearchColor;

		public static readonly Color HighlightLineResearchColor;

		public static readonly Texture2D FastFillTex;

		public static readonly GUIStyle FastFillStyle;

		public static readonly Texture2D TextBGBlack;

		public static readonly Texture2D GrayTextBG;

		public static readonly Texture2D FloatMenuOptionBG;

		static TexUI()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ColorInt colorInt = new ColorInt(51, 63, 51, 200);
			TexUI.GrayBg = SolidColorMaterials.NewSolidColorTexture(colorInt.ToColor);
			ColorInt colorInt2 = new ColorInt(32, 32, 32, 255);
			TexUI.AvailResearchColor = colorInt2.ToColor;
			ColorInt colorInt3 = new ColorInt(0, 64, 64, 255);
			TexUI.ActiveResearchColor = colorInt3.ToColor;
			ColorInt colorInt4 = new ColorInt(0, 64, 16, 255);
			TexUI.FinishedResearchColor = colorInt4.ToColor;
			ColorInt colorInt5 = new ColorInt(42, 42, 42, 255);
			TexUI.LockedResearchColor = colorInt5.ToColor;
			ColorInt colorInt6 = new ColorInt(0, 0, 0, 255);
			TexUI.RelatedResearchColor = colorInt6.ToColor;
			ColorInt colorInt7 = new ColorInt(30, 30, 30, 255);
			TexUI.HighlightBgResearchColor = colorInt7.ToColor;
			ColorInt colorInt8 = new ColorInt(160, 160, 160, 255);
			TexUI.HighlightBorderResearchColor = colorInt8.ToColor;
			ColorInt colorInt9 = new ColorInt(80, 80, 80, 255);
			TexUI.DefaultBorderResearchColor = colorInt9.ToColor;
			ColorInt colorInt10 = new ColorInt(60, 60, 60, 255);
			TexUI.DefaultLineResearchColor = colorInt10.ToColor;
			ColorInt colorInt11 = new ColorInt(51, 205, 217, 255);
			TexUI.HighlightLineResearchColor = colorInt11.ToColor;
			TexUI.FastFillTex = Texture2D.whiteTexture;
			TexUI.FastFillStyle = new GUIStyle
			{
				normal = new GUIStyleState
				{
					background = TexUI.FastFillTex
				}
			};
			TexUI.TextBGBlack = ContentFinder<Texture2D>.Get("UI/Widgets/TextBGBlack", true);
			TexUI.GrayTextBG = ContentFinder<Texture2D>.Get("UI/Overlays/GrayTextBG", true);
			TexUI.FloatMenuOptionBG = ContentFinder<Texture2D>.Get("UI/Widgets/FloatMenuOptionBG", true);
		}
	}
}
