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

		public static readonly Texture2D TextBGBlack;

		public static readonly Texture2D GrayTextBG;

		public static readonly Texture2D FloatMenuOptionBG;

		static TexUI()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ColorInt colorInt = new ColorInt(51, 63, 51, 200);
			TexUI.GrayBg = SolidColorMaterials.NewSolidColorTexture(colorInt.ToColor);
			TexUI.TextBGBlack = ContentFinder<Texture2D>.Get("UI/Widgets/TextBGBlack", true);
			TexUI.GrayTextBG = ContentFinder<Texture2D>.Get("UI/Overlays/GrayTextBG", true);
			TexUI.FloatMenuOptionBG = ContentFinder<Texture2D>.Get("UI/Widgets/FloatMenuOptionBG", true);
		}
	}
}
