using System;
using UnityEngine;

namespace Verse
{
	public static class Text
	{
		private static GameFont fontInt;

		private static TextAnchor anchorInt;

		private static bool wordWrapInt;

		public static readonly GUIStyle[] fontStyles;

		public static readonly GUIStyle[] textFieldStyles;

		public static readonly GUIStyle[] textAreaStyles;

		public static readonly GUIStyle[] textAreaReadOnlyStyles;

		private static readonly float[] lineHeights;

		private static readonly float[] spaceBetweenLines;

		private static GUIContent tmpTextGUIContent;

		private const int NumFonts = 3;

		public static GameFont Font
		{
			get
			{
				return Text.fontInt;
			}
			set
			{
				if (value == GameFont.Tiny && !LongEventHandler.AnyEventNowOrWaiting && !LanguageDatabase.activeLanguage.info.canBeTiny)
				{
					Text.fontInt = GameFont.Small;
				}
				else
				{
					Text.fontInt = value;
				}
			}
		}

		public static TextAnchor Anchor
		{
			get
			{
				return Text.anchorInt;
			}
			set
			{
				Text.anchorInt = value;
			}
		}

		public static bool WordWrap
		{
			get
			{
				return Text.wordWrapInt;
			}
			set
			{
				Text.wordWrapInt = value;
			}
		}

		public static float LineHeight
		{
			get
			{
				return Text.lineHeights[(int)Text.Font];
			}
		}

		public static float SpaceBetweenLines
		{
			get
			{
				return Text.spaceBetweenLines[(int)Text.Font];
			}
		}

		public static GUIStyle CurFontStyle
		{
			get
			{
				GUIStyle gUIStyle;
				switch (Text.fontInt)
				{
				case GameFont.Tiny:
					gUIStyle = Text.fontStyles[0];
					break;
				case GameFont.Small:
					gUIStyle = Text.fontStyles[1];
					break;
				case GameFont.Medium:
					gUIStyle = Text.fontStyles[2];
					break;
				default:
					throw new NotImplementedException();
				}
				gUIStyle.alignment = Text.anchorInt;
				gUIStyle.wordWrap = Text.wordWrapInt;
				return gUIStyle;
			}
		}

		public static GUIStyle CurTextFieldStyle
		{
			get
			{
				GameFont gameFont = Text.fontInt;
				if (gameFont == GameFont.Tiny)
				{
					return Text.textFieldStyles[0];
				}
				if (gameFont == GameFont.Small)
				{
					return Text.textFieldStyles[1];
				}
				if (gameFont != GameFont.Medium)
				{
					throw new NotImplementedException();
				}
				return Text.textFieldStyles[2];
			}
		}

		public static GUIStyle CurTextAreaStyle
		{
			get
			{
				GameFont gameFont = Text.fontInt;
				if (gameFont == GameFont.Tiny)
				{
					return Text.textAreaStyles[0];
				}
				if (gameFont == GameFont.Small)
				{
					return Text.textAreaStyles[1];
				}
				if (gameFont != GameFont.Medium)
				{
					throw new NotImplementedException();
				}
				return Text.textAreaStyles[2];
			}
		}

		public static GUIStyle CurTextAreaReadOnlyStyle
		{
			get
			{
				GameFont gameFont = Text.fontInt;
				if (gameFont == GameFont.Tiny)
				{
					return Text.textAreaReadOnlyStyles[0];
				}
				if (gameFont == GameFont.Small)
				{
					return Text.textAreaReadOnlyStyles[1];
				}
				if (gameFont != GameFont.Medium)
				{
					throw new NotImplementedException();
				}
				return Text.textAreaReadOnlyStyles[2];
			}
		}

		static Text()
		{
			Text.fontInt = GameFont.Small;
			Text.anchorInt = TextAnchor.UpperLeft;
			Text.wordWrapInt = true;
			Text.fontStyles = new GUIStyle[3];
			Text.textFieldStyles = new GUIStyle[3];
			Text.textAreaStyles = new GUIStyle[3];
			Text.textAreaReadOnlyStyles = new GUIStyle[3];
			Text.lineHeights = new float[3];
			Text.spaceBetweenLines = new float[3];
			Text.tmpTextGUIContent = new GUIContent();
			Font font = (Font)Resources.Load("Fonts/Calibri_tiny");
			Font font2 = (Font)Resources.Load("Fonts/Arial_small");
			Font font3 = (Font)Resources.Load("Fonts/Arial_medium");
			Text.fontStyles[0] = new GUIStyle(GUI.skin.label);
			Text.fontStyles[0].font = font;
			Text.fontStyles[1] = new GUIStyle(GUI.skin.label);
			Text.fontStyles[1].font = font2;
			Text.fontStyles[1].contentOffset = new Vector2(0f, -1f);
			Text.fontStyles[2] = new GUIStyle(GUI.skin.label);
			Text.fontStyles[2].font = font3;
			for (int i = 0; i < Text.textFieldStyles.Length; i++)
			{
				Text.textFieldStyles[i] = new GUIStyle(GUI.skin.textField);
				Text.textFieldStyles[i].alignment = TextAnchor.MiddleLeft;
			}
			Text.textFieldStyles[0].font = font;
			Text.textFieldStyles[1].font = font2;
			Text.textFieldStyles[2].font = font3;
			for (int j = 0; j < Text.textAreaStyles.Length; j++)
			{
				Text.textAreaStyles[j] = new GUIStyle(Text.textFieldStyles[j]);
				Text.textAreaStyles[j].alignment = TextAnchor.UpperLeft;
				Text.textAreaStyles[j].wordWrap = true;
			}
			for (int k = 0; k < Text.textAreaReadOnlyStyles.Length; k++)
			{
				Text.textAreaReadOnlyStyles[k] = new GUIStyle(Text.textAreaStyles[k]);
				GUIStyle gUIStyle = Text.textAreaReadOnlyStyles[k];
				gUIStyle.normal.background = null;
				gUIStyle.active.background = null;
				gUIStyle.onHover.background = null;
				gUIStyle.hover.background = null;
				gUIStyle.onFocused.background = null;
				gUIStyle.focused.background = null;
			}
			GUI.skin.settings.doubleClickSelectsWord = true;
			int num = 0;
			foreach (GameFont font4 in Enum.GetValues(typeof(GameFont)))
			{
				Text.Font = font4;
				Text.lineHeights[num] = Text.CalcHeight("W", 999f);
				Text.spaceBetweenLines[num] = Text.CalcHeight("W\nW", 999f) - Text.CalcHeight("W", 999f) * 2f;
				num++;
			}
			Text.Font = GameFont.Small;
		}

		public static float CalcHeight(string text, float width)
		{
			Text.tmpTextGUIContent.text = text;
			return Text.CurFontStyle.CalcHeight(Text.tmpTextGUIContent, width);
		}

		public static Vector2 CalcSize(string text)
		{
			Text.tmpTextGUIContent.text = text;
			return Text.CurFontStyle.CalcSize(Text.tmpTextGUIContent);
		}

		public static void StartOfOnGUI()
		{
			if (!Text.WordWrap)
			{
				Log.ErrorOnce("Word wrap was false at end of frame.", 764362, false);
				Text.WordWrap = true;
			}
			if (Text.Anchor != TextAnchor.UpperLeft)
			{
				Log.ErrorOnce("Alignment was " + Text.Anchor + " at end of frame.", 15558, false);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			Text.Font = GameFont.Small;
		}
	}
}
