using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class Widgets
	{
		private enum RangeEnd : byte
		{
			None,
			Min,
			Max
		}

		public const float CheckboxSize = 24f;

		public const float RadioButtonSize = 24f;

		private const int FillableBarBorderWidth = 3;

		private const int MaxFillChangeArrowHeight = 16;

		private const int FillChangeArrowWidth = 8;

		private const float CloseButtonSize = 18f;

		private const float CloseButtonMargin = 4f;

		private const float SeparatorLabelHeight = 20f;

		public const float ListSeparatorHeight = 25f;

		public const float RangeControlIdealHeight = 31f;

		private const float RangeSliderSize = 16f;

		public const float InfoCardButtonSize = 24f;

		public static readonly GUIStyle EmptyStyle;

		private static readonly Color InactiveColor;

		private static readonly Texture2D DefaultBarBgTex;

		private static readonly Texture2D BarFullTexHor;

		public static readonly Texture2D CheckboxOnTex;

		public static readonly Texture2D CheckboxOffTex;

		public static readonly Texture2D CheckboxPartialTex;

		private static readonly Texture2D RadioButOnTex;

		private static readonly Texture2D RadioButOffTex;

		private static readonly Texture2D FillArrowTexRight;

		private static readonly Texture2D FillArrowTexLeft;

		private static readonly Texture2D ShadowAtlas;

		private static readonly Texture2D ButtonBGAtlas;

		private static readonly Texture2D ButtonBGAtlasMouseover;

		private static readonly Texture2D ButtonBGAtlasClick;

		private static readonly Texture2D FloatRangeSliderTex;

		private static Texture2D LineTexAA;

		private static readonly Rect LineRect;

		private static readonly Material LineMat;

		private static readonly Texture2D AltTexture;

		public static readonly Color NormalOptionColor;

		public static readonly Color MouseoverOptionColor;

		private static Dictionary<string, float> LabelCache;

		public static readonly Color SeparatorLabelColor;

		private static readonly Color SeparatorLineColor;

		public static readonly Texture2D ButtonSubtleAtlas;

		private static readonly Texture2D ButtonBarTex;

		private static readonly Color RangeControlTextColor;

		private static int draggingId;

		private static Widgets.RangeEnd curDragEnd;

		private static float lastDragSliderSoundTime;

		private static float FillableBarChangeRateDisplayRatio;

		public static int MaxFillableBarChangeRate;

		private static readonly Color WindowBGBorderColor;

		private static readonly Color WindowBGFillColor;

		private static readonly Color MenuSectionBGFillColor;

		private static readonly Color MenuSectionBGBorderColor;

		private static readonly Color TutorWindowBGFillColor;

		private static readonly Color TutorWindowBGBorderColor;

		static Widgets()
		{
			Widgets.EmptyStyle = new GUIStyle();
			Widgets.InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
			Widgets.DefaultBarBgTex = BaseContent.BlackTex;
			Widgets.BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
			Widgets.CheckboxOnTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn", true);
			Widgets.CheckboxOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff", true);
			Widgets.CheckboxPartialTex = ContentFinder<Texture2D>.Get("UI/Widgets/CheckPartial", true);
			Widgets.RadioButOnTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOn", true);
			Widgets.RadioButOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOff", true);
			Widgets.FillArrowTexRight = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowRight", true);
			Widgets.FillArrowTexLeft = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowLeft", true);
			Widgets.ShadowAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/DropShadow", true);
			Widgets.ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBG", true);
			Widgets.ButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGMouseover", true);
			Widgets.ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonBGClick", true);
			Widgets.FloatRangeSliderTex = ContentFinder<Texture2D>.Get("UI/Widgets/RangeSlider", true);
			Widgets.LineTexAA = null;
			Widgets.LineRect = new Rect(0f, 0f, 1f, 1f);
			Widgets.LineMat = null;
			Widgets.AltTexture = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
			Widgets.NormalOptionColor = new Color(0.8f, 0.85f, 1f);
			Widgets.MouseoverOptionColor = Color.yellow;
			Widgets.LabelCache = new Dictionary<string, float>();
			Widgets.SeparatorLabelColor = new Color(0.8f, 0.8f, 0.8f, 1f);
			Widgets.SeparatorLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
			Widgets.ButtonSubtleAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/ButtonSubtleAtlas", true);
			ColorInt colorInt = new ColorInt(78, 109, 129, 130);
			Widgets.ButtonBarTex = SolidColorMaterials.NewSolidColorTexture(colorInt.ToColor);
			Widgets.RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);
			Widgets.draggingId = 0;
			Widgets.curDragEnd = Widgets.RangeEnd.None;
			Widgets.lastDragSliderSoundTime = -1f;
			Widgets.FillableBarChangeRateDisplayRatio = 1E+08f;
			Widgets.MaxFillableBarChangeRate = 3;
			ColorInt colorInt2 = new ColorInt(97, 108, 122);
			Widgets.WindowBGBorderColor = colorInt2.ToColor;
			ColorInt colorInt3 = new ColorInt(21, 25, 29);
			Widgets.WindowBGFillColor = colorInt3.ToColor;
			ColorInt colorInt4 = new ColorInt(42, 43, 44);
			Widgets.MenuSectionBGFillColor = colorInt4.ToColor;
			ColorInt colorInt5 = new ColorInt(135, 135, 135);
			Widgets.MenuSectionBGBorderColor = colorInt5.ToColor;
			ColorInt colorInt6 = new ColorInt(133, 85, 44);
			Widgets.TutorWindowBGFillColor = colorInt6.ToColor;
			ColorInt colorInt7 = new ColorInt(176, 139, 61);
			Widgets.TutorWindowBGBorderColor = colorInt7.ToColor;
			Color color = new Color(1f, 1f, 1f, 0f);
			Widgets.LineTexAA = new Texture2D(1, 3, TextureFormat.ARGB32, false);
			Widgets.LineTexAA.name = "LineTexAA";
			Widgets.LineTexAA.SetPixel(0, 0, color);
			Widgets.LineTexAA.SetPixel(0, 1, Color.white);
			Widgets.LineTexAA.SetPixel(0, 2, color);
			Widgets.LineTexAA.Apply();
			Widgets.LineMat = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
		}

		public static void ThingIcon(Rect rect, Thing thing, float alpha = 1f)
		{
			GUI.color = thing.DrawColor;
			Texture resolvedIcon;
			if (!thing.def.uiIconPath.NullOrEmpty())
			{
				resolvedIcon = thing.def.uiIcon;
			}
			else if (thing is Pawn || thing is Corpse)
			{
				Pawn pawn = thing as Pawn;
				if (pawn == null)
				{
					pawn = ((Corpse)thing).InnerPawn;
				}
				if (!pawn.RaceProps.Humanlike)
				{
					if (!pawn.Drawer.renderer.graphics.AllResolved)
					{
						pawn.Drawer.renderer.graphics.ResolveAllGraphics();
					}
					Material matSingle = pawn.Drawer.renderer.graphics.nakedGraphic.MatSingle;
					resolvedIcon = matSingle.mainTexture;
					GUI.color = matSingle.color;
				}
				else
				{
					rect = rect.ScaledBy(1.8f);
					rect.y += 3f;
					rect = rect.Rounded();
					resolvedIcon = PortraitsCache.Get(pawn, new Vector2(rect.width, rect.height), default(Vector3), 1f);
				}
			}
			else
			{
				resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatSingle.mainTexture;
			}
			if (alpha != 1f)
			{
				Color color = GUI.color;
				color.a *= alpha;
				GUI.color = color;
			}
			Widgets.ThingIconWorker(rect, thing.def, resolvedIcon);
			GUI.color = Color.white;
		}

		public static void ThingIcon(Rect rect, ThingDef thingDef)
		{
			GUI.color = thingDef.graphicData.color;
			Widgets.ThingIconWorker(rect, thingDef, thingDef.uiIcon);
			GUI.color = Color.white;
		}

		private static void ThingIconWorker(Rect rect, ThingDef thingDef, Texture resolvedIcon)
		{
			float num = GenUI.IconDrawScale(thingDef);
			if (num != 1f)
			{
				Vector2 center = rect.center;
				rect.width *= num;
				rect.height *= num;
				rect.center = center;
			}
			GUI.DrawTexture(rect, resolvedIcon);
		}

		public static void DrawAltRect(Rect rect)
		{
			GUI.DrawTexture(rect, Widgets.AltTexture);
		}

		public static void ListSeparator(ref float curY, float width, string label)
		{
			Color color = GUI.color;
			curY += 3f;
			GUI.color = Widgets.SeparatorLabelColor;
			Rect rect = new Rect(0f, curY, width, 30f);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect, label);
			curY += 20f;
			GUI.color = Widgets.SeparatorLineColor;
			Widgets.DrawLineHorizontal(0f, curY, width);
			curY += 2f;
			GUI.color = color;
		}

		public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
		{
			float num = end.x - start.x;
			float num2 = end.y - start.y;
			float num3 = Mathf.Sqrt(num * num + num2 * num2);
			if (num3 < 0.01f)
			{
				return;
			}
			width *= 3f;
			float num4 = width * num2 / num3;
			float num5 = width * num / num3;
			Matrix4x4 identity = Matrix4x4.identity;
			identity.m00 = num;
			identity.m01 = -num4;
			identity.m03 = start.x + 0.5f * num4;
			identity.m10 = num2;
			identity.m11 = num5;
			identity.m13 = start.y - 0.5f * num5;
			GL.PushMatrix();
			GL.MultMatrix(identity);
			Graphics.DrawTexture(Widgets.LineRect, Widgets.LineTexAA, Widgets.LineRect, 0, 0, 0, 0, color, Widgets.LineMat);
			GL.PopMatrix();
		}

		public static void DrawLineHorizontal(float x, float y, float length)
		{
			Rect position = new Rect(x, y, length, 1f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
		}

		public static void DrawLineVertical(float x, float y, float length)
		{
			Rect position = new Rect(x, y, 1f, length);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
		}

		public static void DrawBoxSolid(Rect rect, Color color)
		{
			Color color2 = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = color2;
		}

		public static void DrawBox(Rect rect, int thickness = 1)
		{
			Vector2 b = new Vector2(rect.x, rect.y);
			Vector2 a = new Vector2(rect.x + rect.width, rect.y + rect.height);
			if (b.x > a.x)
			{
				float x = b.x;
				b.x = a.x;
				a.x = x;
			}
			if (b.y > a.y)
			{
				float y = b.y;
				b.y = a.y;
				a.y = y;
			}
			Vector3 vector = a - b;
			GUI.DrawTexture(new Rect(b.x, b.y, (float)thickness, vector.y), BaseContent.WhiteTex);
			GUI.DrawTexture(new Rect(a.x - (float)thickness, b.y, (float)thickness, vector.y), BaseContent.WhiteTex);
			GUI.DrawTexture(new Rect(b.x + (float)thickness, b.y, vector.x - (float)(thickness * 2), (float)thickness), BaseContent.WhiteTex);
			GUI.DrawTexture(new Rect(b.x + (float)thickness, a.y - (float)thickness, vector.x - (float)(thickness * 2), (float)thickness), BaseContent.WhiteTex);
		}

		public static void LabelCacheHeight(ref Rect rect, string label, bool renderLabel = true, bool forceInvalidation = false)
		{
			bool flag = Widgets.LabelCache.ContainsKey(label);
			if (forceInvalidation)
			{
				flag = false;
			}
			float height;
			if (flag)
			{
				height = Widgets.LabelCache[label];
			}
			else
			{
				height = Text.CurFontStyle.CalcHeight(new GUIContent(label), rect.width);
			}
			rect.height = height;
			if (renderLabel)
			{
				GUI.Label(rect, label, Text.CurFontStyle);
			}
		}

		public static void Label(Rect rect, GUIContent content)
		{
			GUI.Label(rect, content, Text.CurFontStyle);
		}

		public static void Label(Rect rect, string label)
		{
			GUI.Label(rect, label, Text.CurFontStyle);
		}

		public static void Checkbox(Vector2 topLeft, ref bool checkOn, float size = 24f, bool disabled = false)
		{
			Widgets.Checkbox(topLeft.x, topLeft.y, ref checkOn, size, disabled);
		}

		public static void Checkbox(float x, float y, ref bool checkOn, float size = 24f, bool disabled = false)
		{
			if (disabled)
			{
				GUI.color = Widgets.InactiveColor;
			}
			Rect rect = new Rect(x, y, size, size);
			Widgets.CheckboxDraw(x, y, checkOn, disabled, size);
			MouseoverSounds.DoRegion(rect);
			if (!disabled && Widgets.ButtonInvisible(rect, false))
			{
				checkOn = !checkOn;
				if (checkOn)
				{
					SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				}
			}
			if (disabled)
			{
				GUI.color = Color.white;
			}
		}

		public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, label);
			if (!disabled && Widgets.ButtonInvisible(rect, false))
			{
				checkOn = !checkOn;
				if (checkOn)
				{
					SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				}
			}
			Widgets.CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, disabled, 24f);
			Text.Anchor = anchor;
		}

		public static bool CheckboxLabeledSelectable(Rect rect, string label, ref bool selected, ref bool checkOn)
		{
			if (selected)
			{
				Widgets.DrawHighlight(rect);
			}
			Widgets.Label(rect, label);
			bool flag = selected;
			Rect butRect = rect;
			butRect.width -= 24f;
			if (!selected && Widgets.ButtonInvisible(butRect, false))
			{
				SoundDefOf.TickTiny.PlayOneShotOnCamera();
				selected = true;
			}
			Color color = GUI.color;
			GUI.color = Color.white;
			Widgets.CheckboxDraw(rect.xMax - 24f, rect.y, checkOn, false, 24f);
			GUI.color = color;
			Rect butRect2 = new Rect(rect.xMax - 24f, rect.y, 24f, 24f);
			if (Widgets.ButtonInvisible(butRect2, false))
			{
				checkOn = !checkOn;
				if (checkOn)
				{
					SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				}
			}
			return selected && !flag;
		}

		private static void CheckboxDraw(float x, float y, bool active, bool disabled, float size = 24f)
		{
			Color color = GUI.color;
			if (disabled)
			{
				GUI.color = Widgets.InactiveColor;
			}
			Texture2D image;
			if (active)
			{
				image = Widgets.CheckboxOnTex;
			}
			else
			{
				image = Widgets.CheckboxOffTex;
			}
			Rect position = new Rect(x, y, size, size);
			GUI.DrawTexture(position, image);
			if (disabled)
			{
				GUI.color = color;
			}
		}

		public static bool CheckboxMulti(Vector2 topLeft, MultiCheckboxState state, float size)
		{
			Texture2D tex;
			if (state == MultiCheckboxState.On)
			{
				tex = Widgets.CheckboxOnTex;
			}
			else if (state == MultiCheckboxState.Off)
			{
				tex = Widgets.CheckboxOffTex;
			}
			else
			{
				tex = Widgets.CheckboxPartialTex;
			}
			Rect rect = new Rect(topLeft.x, topLeft.y, size, size);
			MouseoverSounds.DoRegion(rect);
			if (Widgets.ButtonImage(rect, tex))
			{
				if (state == MultiCheckboxState.Off || state == MultiCheckboxState.Partial)
				{
					SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
				}
				return true;
			}
			return false;
		}

		public static bool RadioButton(Vector2 topLeft, bool chosen)
		{
			return Widgets.RadioButton(topLeft.x, topLeft.y, chosen);
		}

		public static bool RadioButton(float x, float y, bool chosen)
		{
			Rect rect = new Rect(x, y, 24f, 24f);
			MouseoverSounds.DoRegion(rect);
			Widgets.RadioButtonDraw(x, y, chosen);
			bool flag = Widgets.ButtonInvisible(rect, false);
			if (flag && !chosen)
			{
				SoundDefOf.RadioButtonClicked.PlayOneShotOnCamera();
			}
			return flag;
		}

		public static bool RadioButtonLabeled(Rect rect, string labelText, bool chosen)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, labelText);
			Text.Anchor = anchor;
			bool flag = Widgets.ButtonInvisible(rect, false);
			if (flag && !chosen)
			{
				SoundDefOf.RadioButtonClicked.PlayOneShotOnCamera();
			}
			Widgets.RadioButtonDraw(rect.x + rect.width - 24f, rect.y + rect.height / 2f - 12f, chosen);
			return flag;
		}

		private static void RadioButtonDraw(float x, float y, bool chosen)
		{
			Texture2D image;
			if (chosen)
			{
				image = Widgets.RadioButOnTex;
			}
			else
			{
				image = Widgets.RadioButOffTex;
			}
			Rect position = new Rect(x, y, 24f, 24f);
			GUI.DrawTexture(position, image);
		}

		public static bool ButtonText(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = false, bool active = true)
		{
			return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, Widgets.NormalOptionColor, active);
		}

		public static bool ButtonText(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active = true)
		{
			TextAnchor anchor = Text.Anchor;
			Color color = GUI.color;
			if (drawBackground)
			{
				Texture2D atlas = Widgets.ButtonBGAtlas;
				if (Mouse.IsOver(rect))
				{
					atlas = Widgets.ButtonBGAtlasMouseover;
					if (Input.GetMouseButton(0))
					{
						atlas = Widgets.ButtonBGAtlasClick;
					}
				}
				Widgets.DrawAtlas(rect, atlas);
			}
			if (doMouseoverSound)
			{
				MouseoverSounds.DoRegion(rect);
			}
			if (!drawBackground)
			{
				GUI.color = textColor;
				if (Mouse.IsOver(rect))
				{
					GUI.color = Widgets.MouseoverOptionColor;
				}
			}
			if (drawBackground)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
			}
			else
			{
				Text.Anchor = TextAnchor.MiddleLeft;
			}
			Widgets.Label(rect, label);
			Text.Anchor = anchor;
			GUI.color = color;
			return active && Widgets.ButtonInvisible(rect, false);
		}

		public static void DrawRectFast(Rect position, Color color, GUIContent content = null)
		{
			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = color;
			GUI.Box(position, content ?? GUIContent.none, TexUI.FastFillStyle);
			GUI.backgroundColor = backgroundColor;
		}

		public static bool CustomButtonText(ref Rect rect, string label, Color bgColor, Color textColor, Color borderColor, bool cacheHeight = false, int borderSize = 1, bool doMouseoverSound = true, bool active = true)
		{
			if (cacheHeight)
			{
				Widgets.LabelCacheHeight(ref rect, label, false, false);
			}
			Rect position = new Rect(rect);
			position.x += (float)borderSize;
			position.y += (float)borderSize;
			position.width -= (float)(borderSize * 2);
			position.height -= (float)(borderSize * 2);
			Widgets.DrawRectFast(rect, borderColor, null);
			Widgets.DrawRectFast(position, bgColor, null);
			TextAnchor anchor = Text.Anchor;
			Color color = GUI.color;
			if (doMouseoverSound)
			{
				MouseoverSounds.DoRegion(rect);
			}
			GUI.color = textColor;
			if (Mouse.IsOver(rect))
			{
				GUI.color = Widgets.MouseoverOptionColor;
			}
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, label);
			Text.Anchor = anchor;
			GUI.color = color;
			return active && Widgets.ButtonInvisible(rect, false);
		}

		public static bool ButtonTextSubtle(Rect rect, string label, float barPercent = 0f, float textLeftMargin = -1f, SoundDef mouseoverSound = null)
		{
			bool flag = false;
			if (Mouse.IsOver(rect))
			{
				flag = true;
				GUI.color = GenUI.MouseoverColor;
			}
			if (mouseoverSound != null)
			{
				MouseoverSounds.DoRegion(rect, mouseoverSound);
			}
			Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
			GUI.color = Color.white;
			if (barPercent > 0.001f)
			{
				Rect rect2 = rect.ContractedBy(1f);
				Widgets.FillableBar(rect2, barPercent, Widgets.ButtonBarTex, null, false);
			}
			Rect rect3 = new Rect(rect);
			if (textLeftMargin < 0f)
			{
				textLeftMargin = rect.width * 0.15f;
			}
			rect3.x += textLeftMargin;
			if (flag)
			{
				rect3.x += 2f;
				rect3.y -= 2f;
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Text.Font = GameFont.Small;
			Widgets.Label(rect3, label);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			return Widgets.ButtonInvisible(rect, false);
		}

		public static bool ButtonImage(Rect butRect, Texture2D tex)
		{
			return Widgets.ButtonImage(butRect, tex, Color.white);
		}

		public static bool ButtonImage(Rect butRect, Texture2D tex, Color baseColor)
		{
			return Widgets.ButtonImage(butRect, tex, baseColor, GenUI.MouseoverColor);
		}

		public static bool ButtonImage(Rect butRect, Texture2D tex, Color baseColor, Color mouseoverColor)
		{
			if (Mouse.IsOver(butRect))
			{
				GUI.color = mouseoverColor;
			}
			else
			{
				GUI.color = baseColor;
			}
			GUI.DrawTexture(butRect, tex);
			GUI.color = baseColor;
			return Widgets.ButtonInvisible(butRect, false);
		}

		public static bool CloseButtonFor(Rect rectToClose)
		{
			Rect butRect = new Rect(rectToClose.x + rectToClose.width - 18f - 4f, rectToClose.y + 4f, 18f, 18f);
			return Widgets.ButtonImage(butRect, TexButton.CloseXSmall);
		}

		public static bool ButtonInvisible(Rect butRect, bool doMouseoverSound = false)
		{
			if (doMouseoverSound)
			{
				MouseoverSounds.DoRegion(butRect);
			}
			return GUI.Button(butRect, string.Empty, Widgets.EmptyStyle);
		}

		public static string TextField(Rect rect, string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}
			return GUI.TextField(rect, text, Text.CurTextFieldStyle);
		}

		public static string TextField(Rect rect, string text, int maxLength, Regex inputValidator)
		{
			string text2 = Widgets.TextField(rect, text);
			if (text2.Length <= maxLength && Outfit.ValidNameRegex.IsMatch(text2))
			{
				return text2;
			}
			return text;
		}

		public static string TextArea(Rect rect, string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}
			return GUI.TextArea(rect, text, Text.CurTextAreaStyle);
		}

		public static string TextEntryLabeled(Rect rect, string label, string text)
		{
			Rect rect2 = rect.LeftHalf().Rounded();
			Rect rect3 = rect.RightHalf().Rounded();
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect2, label);
			Text.Anchor = anchor;
			if (rect.height <= 30f)
			{
				return Widgets.TextField(rect3, text);
			}
			return Widgets.TextArea(rect3, text);
		}

		public static void TextFieldNumeric<T>(Rect rect, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
		{
			if (buffer == null)
			{
				buffer = val.ToString();
			}
			string text = "TextField" + rect.y.ToString("F0") + rect.x.ToString("F0");
			GUI.SetNextControlName(text);
			string text2 = GUI.TextField(rect, buffer, Text.CurTextFieldStyle);
			if (GUI.GetNameOfFocusedControl() != text)
			{
				Widgets.ResolveParseNow<T>(buffer, ref val, ref buffer, min, max, true);
			}
			else if (text2 != buffer && Widgets.IsPartiallyOrFullyTypedNumber<T>(ref val, text2, min, max))
			{
				buffer = text2;
				if (text2.IsFullyTypedNumber<T>())
				{
					Widgets.ResolveParseNow<T>(text2, ref val, ref buffer, min, max, false);
				}
			}
		}

		private static void ResolveParseNow<T>(string edited, ref T val, ref string buffer, float min, float max, bool force)
		{
			if (typeof(T) == typeof(int))
			{
				if (edited.NullOrEmpty())
				{
					Widgets.ResetValue<T>(edited, ref val, ref buffer, min, max);
					return;
				}
				int num;
				if (int.TryParse(edited, out num))
				{
					val = (T)((object)Mathf.RoundToInt(Mathf.Clamp((float)num, min, max)));
					buffer = Widgets.ToStringTypedIn<T>(val);
					return;
				}
				if (force)
				{
					Widgets.ResetValue<T>(edited, ref val, ref buffer, min, max);
				}
			}
			else if (typeof(T) == typeof(float))
			{
				float value;
				if (float.TryParse(edited, out value))
				{
					val = (T)((object)Mathf.Clamp(value, min, max));
					buffer = Widgets.ToStringTypedIn<T>(val);
					return;
				}
				if (force)
				{
					Widgets.ResetValue<T>(edited, ref val, ref buffer, min, max);
				}
			}
			else
			{
				Log.Error("TextField<T> does not support " + typeof(T));
			}
		}

		private static void ResetValue<T>(string edited, ref T val, ref string buffer, float min, float max)
		{
			val = default(T);
			if (min > 0f)
			{
				val = (T)((object)Mathf.RoundToInt(min));
			}
			if (max < 0f)
			{
				val = (T)((object)Mathf.RoundToInt(max));
			}
			buffer = Widgets.ToStringTypedIn<T>(val);
		}

		private static string ToStringTypedIn<T>(T val)
		{
			if (typeof(T) == typeof(float))
			{
				return ((float)((object)val)).ToString("0.##########");
			}
			return val.ToString();
		}

		private static bool IsPartiallyOrFullyTypedNumber<T>(ref T val, string s, float min, float max)
		{
			if (s == string.Empty)
			{
				return true;
			}
			if (s[0] == '-' && min >= 0f)
			{
				return false;
			}
			if (s.Length > 1 && s[s.Length - 1] == '-')
			{
				return false;
			}
			if (s == "00")
			{
				return false;
			}
			if (s.Length > 12)
			{
				return false;
			}
			if (typeof(T) == typeof(float))
			{
				int num = s.CharacterCount('.');
				if (num <= 1 && s.ContainsOnlyCharacters("-.0123456789"))
				{
					return true;
				}
			}
			return s.IsFullyTypedNumber<T>();
		}

		private static bool IsFullyTypedNumber<T>(this string s)
		{
			if (s == string.Empty)
			{
				return false;
			}
			if (typeof(T) == typeof(float))
			{
				string[] array = s.Split(new char[]
				{
					'.'
				});
				if (array.Length > 2 || array.Length < 1)
				{
					return false;
				}
				if (!array[0].ContainsOnlyCharacters("-0123456789"))
				{
					return false;
				}
				if (array.Length == 2 && !array[1].ContainsOnlyCharacters("0123456789"))
				{
					return false;
				}
			}
			return typeof(T) != typeof(int) || s.ContainsOnlyCharacters("-0123456789");
		}

		private static bool ContainsOnlyCharacters(this string s, string allowedChars)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (!allowedChars.Contains(s[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static int CharacterCount(this string s, char c)
		{
			int num = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == c)
				{
					num++;
				}
			}
			return num;
		}

		public static void TextFieldNumericLabeled<T>(Rect rect, string label, ref T val, ref string buffer, float min = 0f, float max = 1E+09f) where T : struct
		{
			Rect rect2 = rect.LeftHalf().Rounded();
			Rect rect3 = rect.RightHalf().Rounded();
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect2, label);
			Text.Anchor = anchor;
			Widgets.TextFieldNumeric<T>(rect3, ref val, ref buffer, min, max);
		}

		public static void TextFieldPercent(Rect rect, ref float val, ref string buffer, float min = 0f, float max = 1f)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width - 25f, rect.height);
			Rect rect3 = new Rect(rect2.xMax, rect.y, 25f, rect2.height);
			Widgets.Label(rect3, "%");
			float num = val * 100f;
			Widgets.TextFieldNumeric<float>(rect2, ref num, ref buffer, min * 100f, max * 100f);
			val = num / 100f;
			if (val > max)
			{
				val = max;
				buffer = val.ToString();
			}
		}

		public static T ChangeType<T>(this object obj)
		{
			return (T)((object)Convert.ChangeType(obj, typeof(T)));
		}

		public static float HorizontalSlider(Rect rect, float value, float leftValue, float rightValue, bool middleAlignment = false, string label = null, string leftAlignedLabel = null, string rightAlignedLabel = null, float roundTo = -1f)
		{
			if (middleAlignment || !label.NullOrEmpty())
			{
				rect.y += Mathf.Round((rect.height - 16f) / 2f);
			}
			if (!label.NullOrEmpty())
			{
				rect.y += 5f;
			}
			float num = GUI.HorizontalSlider(rect, value, leftValue, rightValue);
			if (!label.NullOrEmpty() || !leftAlignedLabel.NullOrEmpty() || !rightAlignedLabel.NullOrEmpty())
			{
				TextAnchor anchor = Text.Anchor;
				GameFont font = Text.Font;
				Text.Font = GameFont.Tiny;
				float num2 = (!label.NullOrEmpty()) ? Text.CalcSize(label).y : 18f;
				rect.y = rect.y - num2 + 3f;
				if (!leftAlignedLabel.NullOrEmpty())
				{
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect, leftAlignedLabel);
				}
				if (!rightAlignedLabel.NullOrEmpty())
				{
					Text.Anchor = TextAnchor.UpperRight;
					Widgets.Label(rect, rightAlignedLabel);
				}
				if (!label.NullOrEmpty())
				{
					Text.Anchor = TextAnchor.UpperCenter;
					Widgets.Label(rect, label);
				}
				Text.Anchor = anchor;
				Text.Font = font;
			}
			if (roundTo > 0f)
			{
				num = (float)Mathf.RoundToInt(num / roundTo) * roundTo;
			}
			return num;
		}

		public static float FrequencyHorizontalSlider(Rect rect, float freq, float minFreq, float maxFreq, bool roundToInt = false)
		{
			float num;
			if (freq < 1f)
			{
				float x = 1f / freq;
				num = GenMath.LerpDouble(1f, 1f / minFreq, 0.5f, 1f, x);
			}
			else
			{
				num = GenMath.LerpDouble(maxFreq, 1f, 0f, 0.5f, freq);
			}
			string label;
			if (freq == 1f)
			{
				label = "EveryDay".Translate();
			}
			else if (freq < 1f)
			{
				label = "TimesPerDay".Translate(new object[]
				{
					(1f / freq).ToString("0.##")
				});
			}
			else
			{
				label = "EveryDays".Translate(new object[]
				{
					freq.ToString("0.##")
				});
			}
			float num2 = Widgets.HorizontalSlider(rect, num, 0f, 1f, true, label, null, null, -1f);
			if (num != num2)
			{
				float num3;
				if (num2 < 0.5f)
				{
					num3 = GenMath.LerpDouble(0.5f, 0f, 1f, maxFreq, num2);
					if (roundToInt)
					{
						num3 = Mathf.Round(num3);
					}
				}
				else
				{
					float num4 = GenMath.LerpDouble(1f, 0.5f, 1f / minFreq, 1f, num2);
					if (roundToInt)
					{
						num4 = Mathf.Round(num4);
					}
					num3 = 1f / num4;
				}
				freq = num3;
			}
			return freq;
		}

		public static void FloatRange(Rect rect, int id, ref FloatRange range, float min = 0f, float max = 1f, string labelKey = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
		{
			Rect rect2 = rect;
			rect2.xMin += 8f;
			rect2.xMax -= 8f;
			GUI.color = Widgets.RangeControlTextColor;
			string text = range.min.ToStringByStyle(valueStyle, ToStringNumberSense.Absolute) + " - " + range.max.ToStringByStyle(valueStyle, ToStringNumberSense.Absolute);
			if (labelKey != null)
			{
				text = labelKey.Translate(new object[]
				{
					text
				});
			}
			GameFont font = Text.Font;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
			float num = rect2.x + (rect2.width * range.min - min / (max - min));
			float num2 = rect2.x + (rect2.width * range.max - min / (max - min));
			Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, Widgets.FloatRangeSliderTex);
			Rect position3 = new Rect(num2 + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, Widgets.FloatRangeSliderTex);
			if (Widgets.curDragEnd != Widgets.RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
			{
				Widgets.draggingId = 0;
				Widgets.curDragEnd = Widgets.RangeEnd.None;
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			bool flag = false;
			if (Mouse.IsOver(rect) || Widgets.draggingId == id)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != Widgets.draggingId)
				{
					Widgets.draggingId = id;
					float x = Event.current.mousePosition.x;
					if (x < position2.xMax)
					{
						Widgets.curDragEnd = Widgets.RangeEnd.Min;
					}
					else if (x > position3.xMin)
					{
						Widgets.curDragEnd = Widgets.RangeEnd.Max;
					}
					else
					{
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						Widgets.curDragEnd = ((num3 >= num4) ? Widgets.RangeEnd.Max : Widgets.RangeEnd.Min);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (Widgets.curDragEnd != Widgets.RangeEnd.None && Event.current.type == EventType.MouseDrag))
				{
					float num5 = (Event.current.mousePosition.x - rect2.x) / rect2.width * (max - min) + min;
					num5 = Mathf.Clamp(num5, min, max);
					if (Widgets.curDragEnd == Widgets.RangeEnd.Min)
					{
						if (num5 != range.min)
						{
							range.min = num5;
							if (range.max < range.min)
							{
								range.max = range.min;
							}
							Widgets.CheckPlayDragSliderSound();
						}
					}
					else if (Widgets.curDragEnd == Widgets.RangeEnd.Max && num5 != range.max)
					{
						range.max = num5;
						if (range.min > range.max)
						{
							range.min = range.max;
						}
						Widgets.CheckPlayDragSliderSound();
					}
					Event.current.Use();
				}
			}
			Text.Font = font;
		}

		public static void IntRange(Rect rect, int id, ref IntRange range, int min = 0, int max = 100, string labelKey = null, int minWidth = 0)
		{
			Rect rect2 = rect;
			rect2.xMin += 8f;
			rect2.xMax -= 8f;
			GUI.color = Widgets.RangeControlTextColor;
			string text = range.min.ToStringCached() + " - " + range.max.ToStringCached();
			if (labelKey != null)
			{
				text = labelKey.Translate(new object[]
				{
					text
				});
			}
			GameFont font = Text.Font;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
			float num = rect2.x + rect2.width * (float)(range.min - min) / (float)(max - min);
			float num2 = rect2.x + rect2.width * (float)(range.max - min) / (float)(max - min);
			Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, Widgets.FloatRangeSliderTex);
			Rect position3 = new Rect(num2 + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, Widgets.FloatRangeSliderTex);
			if (Widgets.curDragEnd != Widgets.RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseDown))
			{
				Widgets.draggingId = 0;
				Widgets.curDragEnd = Widgets.RangeEnd.None;
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			bool flag = false;
			if (Mouse.IsOver(rect) || Widgets.draggingId == id)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != Widgets.draggingId)
				{
					Widgets.draggingId = id;
					float x = Event.current.mousePosition.x;
					if (x < position2.xMax)
					{
						Widgets.curDragEnd = Widgets.RangeEnd.Min;
					}
					else if (x > position3.xMin)
					{
						Widgets.curDragEnd = Widgets.RangeEnd.Max;
					}
					else
					{
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						Widgets.curDragEnd = ((num3 >= num4) ? Widgets.RangeEnd.Max : Widgets.RangeEnd.Min);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (Widgets.curDragEnd != Widgets.RangeEnd.None && Event.current.type == EventType.MouseDrag))
				{
					float num5 = (Event.current.mousePosition.x - rect2.x) / rect2.width * (float)(max - min) + (float)min;
					num5 = Mathf.Clamp(num5, (float)min, (float)max);
					int num6 = Mathf.RoundToInt(num5);
					if (Widgets.curDragEnd == Widgets.RangeEnd.Min)
					{
						if (num6 != range.min)
						{
							range.min = num6;
							if (range.min > max - minWidth)
							{
								range.min = max - minWidth;
							}
							int num7 = Mathf.Max(min, range.min + minWidth);
							if (range.max < num7)
							{
								range.max = num7;
							}
							Widgets.CheckPlayDragSliderSound();
						}
					}
					else if (Widgets.curDragEnd == Widgets.RangeEnd.Max && num6 != range.max)
					{
						range.max = num6;
						if (range.max < min + minWidth)
						{
							range.max = min + minWidth;
						}
						int num8 = Mathf.Min(max, range.max - minWidth);
						if (range.min > num8)
						{
							range.min = num8;
						}
						Widgets.CheckPlayDragSliderSound();
					}
					Event.current.Use();
				}
			}
			Text.Font = font;
		}

		private static void CheckPlayDragSliderSound()
		{
			if (Time.realtimeSinceStartup > Widgets.lastDragSliderSoundTime + 0.075f)
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				Widgets.lastDragSliderSoundTime = Time.realtimeSinceStartup;
			}
		}

		public static void QualityRange(Rect rect, int id, ref QualityRange range)
		{
			Rect rect2 = rect;
			rect2.xMin += 8f;
			rect2.xMax -= 8f;
			GUI.color = Widgets.RangeControlTextColor;
			string label;
			if (range == RimWorld.QualityRange.All)
			{
				label = "AnyQuality".Translate();
			}
			else if (range.max == range.min)
			{
				label = "OnlyQuality".Translate(new object[]
				{
					range.min.GetLabel()
				});
			}
			else
			{
				label = range.min.GetLabel() + " - " + range.max.GetLabel();
			}
			GameFont font = Text.Font;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(rect2, label);
			Text.Anchor = TextAnchor.UpperLeft;
			Rect position = new Rect(rect2.x, rect2.yMax - 8f - 1f, rect2.width, 2f);
			GUI.DrawTexture(position, BaseContent.WhiteTex);
			GUI.color = Color.white;
			int length = Enum.GetValues(typeof(QualityCategory)).Length;
			float num = rect2.x + rect2.width / (float)(length - 1) * (float)range.min;
			float num2 = rect2.x + rect2.width / (float)(length - 1) * (float)range.max;
			Rect position2 = new Rect(num - 16f, position.center.y - 8f, 16f, 16f);
			GUI.DrawTexture(position2, Widgets.FloatRangeSliderTex);
			Rect position3 = new Rect(num2 + 16f, position.center.y - 8f, -16f, 16f);
			GUI.DrawTexture(position3, Widgets.FloatRangeSliderTex);
			if (Widgets.curDragEnd != Widgets.RangeEnd.None && (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown))
			{
				Widgets.draggingId = 0;
				Widgets.curDragEnd = Widgets.RangeEnd.None;
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
			}
			bool flag = false;
			if (Mouse.IsOver(rect) || id == Widgets.draggingId)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && id != Widgets.draggingId)
				{
					Widgets.draggingId = id;
					float x = Event.current.mousePosition.x;
					if (x < position2.xMax)
					{
						Widgets.curDragEnd = Widgets.RangeEnd.Min;
					}
					else if (x > position3.xMin)
					{
						Widgets.curDragEnd = Widgets.RangeEnd.Max;
					}
					else
					{
						float num3 = Mathf.Abs(x - position2.xMax);
						float num4 = Mathf.Abs(x - (position3.x - 16f));
						Widgets.curDragEnd = ((num3 >= num4) ? Widgets.RangeEnd.Max : Widgets.RangeEnd.Min);
					}
					flag = true;
					Event.current.Use();
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
				}
				if (flag || (Widgets.curDragEnd != Widgets.RangeEnd.None && Event.current.type == EventType.MouseDrag))
				{
					float num5 = (Event.current.mousePosition.x - rect2.x) / rect2.width;
					int num6 = Mathf.RoundToInt(num5 * (float)(length - 1));
					num6 = Mathf.Clamp(num6, 0, length - 1);
					if (Widgets.curDragEnd == Widgets.RangeEnd.Min)
					{
						if (range.min != (QualityCategory)num6)
						{
							range.min = (QualityCategory)num6;
							if (range.max < range.min)
							{
								range.max = range.min;
							}
							SoundDefOf.DragSlider.PlayOneShotOnCamera();
						}
					}
					else if (Widgets.curDragEnd == Widgets.RangeEnd.Max && range.max != (QualityCategory)num6)
					{
						range.max = (QualityCategory)num6;
						if (range.min > range.max)
						{
							range.min = range.max;
						}
						SoundDefOf.DragSlider.PlayOneShotOnCamera();
					}
					Event.current.Use();
				}
			}
			Text.Font = font;
		}

		public static void FloatRangeWithTypeIn(Rect rect, int id, ref FloatRange fRange, float sliderMin = 0f, float sliderMax = 1f, ToStringStyle valueStyle = ToStringStyle.FloatTwo, string labelKey = null)
		{
			Rect rect2 = new Rect(rect);
			rect2.width = rect.width / 4f;
			Rect rect3 = new Rect(rect);
			rect3.width = rect.width / 2f;
			rect3.x = rect.x + rect.width / 4f;
			rect3.height = rect.height / 2f;
			rect3.width -= rect.height;
			Rect butRect = new Rect(rect3);
			butRect.x = rect3.xMax;
			butRect.height = rect.height;
			butRect.width = rect.height;
			Rect rect4 = new Rect(rect);
			rect4.x = rect.x + rect.width * 0.75f;
			rect4.width = rect.width / 4f;
			Widgets.FloatRange(rect3, id, ref fRange, sliderMin, sliderMax, labelKey, valueStyle);
			if (Widgets.ButtonImage(butRect, TexButton.RangeMatch))
			{
				fRange.max = fRange.min;
			}
			float.TryParse(Widgets.TextField(rect2, fRange.min.ToString()), out fRange.min);
			float.TryParse(Widgets.TextField(rect4, fRange.max.ToString()), out fRange.max);
		}

		public static void FillableBar(Rect rect, float fillPercent)
		{
			Widgets.FillableBar(rect, fillPercent, Widgets.BarFullTexHor);
		}

		public static void FillableBar(Rect rect, float fillPercent, Texture2D fillTex)
		{
			bool doBorder = rect.height > 15f && rect.width > 20f;
			Widgets.FillableBar(rect, fillPercent, fillTex, Widgets.DefaultBarBgTex, doBorder);
		}

		public static void FillableBar(Rect rect, float fillPercent, Texture2D fillTex, Texture2D bgTex, bool doBorder)
		{
			if (doBorder)
			{
				GUI.DrawTexture(rect, BaseContent.BlackTex);
				rect = rect.ContractedBy(3f);
			}
			if (bgTex != null)
			{
				GUI.DrawTexture(rect, bgTex);
			}
			rect.width *= fillPercent;
			GUI.DrawTexture(rect, fillTex);
		}

		public static void FillableBarLabeled(Rect rect, float fillPercent, int labelWidth, string label)
		{
			if (fillPercent < 0f)
			{
				fillPercent = 0f;
			}
			if (fillPercent > 1f)
			{
				fillPercent = 1f;
			}
			Rect rect2 = rect;
			rect2.width = (float)labelWidth;
			Widgets.Label(rect2, label);
			Rect rect3 = rect;
			rect3.x += (float)labelWidth;
			rect3.width -= (float)labelWidth;
			Widgets.FillableBar(rect3, fillPercent);
		}

		public static void FillableBarChangeArrows(Rect barRect, float changeRate)
		{
			int changeRate2 = (int)(changeRate * Widgets.FillableBarChangeRateDisplayRatio);
			Widgets.FillableBarChangeArrows(barRect, changeRate2);
		}

		public static void FillableBarChangeArrows(Rect barRect, int changeRate)
		{
			if (changeRate == 0)
			{
				return;
			}
			if (changeRate > Widgets.MaxFillableBarChangeRate)
			{
				changeRate = Widgets.MaxFillableBarChangeRate;
			}
			if (changeRate < -Widgets.MaxFillableBarChangeRate)
			{
				changeRate = -Widgets.MaxFillableBarChangeRate;
			}
			float num = barRect.height;
			if (num > 16f)
			{
				num = 16f;
			}
			int num2 = Mathf.Abs(changeRate);
			float y = barRect.y + barRect.height / 2f - num / 2f;
			float num3;
			float num4;
			Texture2D image;
			if (changeRate > 0)
			{
				num3 = barRect.x + barRect.width + 2f;
				num4 = 8f;
				image = Widgets.FillArrowTexRight;
			}
			else
			{
				num3 = barRect.x - 8f - 2f;
				num4 = -8f;
				image = Widgets.FillArrowTexLeft;
			}
			for (int i = 0; i < num2; i++)
			{
				Rect position = new Rect(num3, y, 8f, num);
				GUI.DrawTexture(position, image);
				num3 += num4;
			}
		}

		public static void DrawWindowBackground(Rect rect)
		{
			GUI.color = Widgets.WindowBGFillColor;
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = Widgets.WindowBGBorderColor;
			Widgets.DrawBox(rect, 1);
			GUI.color = Color.white;
		}

		public static void DrawMenuSection(Rect rect, bool drawTop = true)
		{
			GUI.color = Widgets.MenuSectionBGFillColor;
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = Widgets.MenuSectionBGBorderColor;
			Widgets.DrawBox(rect, 1);
			GUI.color = Color.white;
		}

		public static void DrawWindowBackgroundTutor(Rect rect)
		{
			GUI.color = Widgets.TutorWindowBGFillColor;
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = Widgets.TutorWindowBGBorderColor;
			Widgets.DrawBox(rect, 1);
			GUI.color = Color.white;
		}

		public static void DrawShadowAround(Rect rect)
		{
			Rect rect2 = rect.ContractedBy(-9f);
			rect2.x += 2f;
			rect2.y += 2f;
			Widgets.DrawAtlas(rect2, Widgets.ShadowAtlas);
		}

		public static void DrawAtlas(Rect rect, Texture2D atlas)
		{
			Widgets.DrawAtlas(rect, atlas, true);
		}

		public static void DrawAtlas(Rect rect, Texture2D atlas, bool drawTop)
		{
			rect.x = Mathf.Round(rect.x);
			rect.y = Mathf.Round(rect.y);
			rect.width = Mathf.Round(rect.width);
			rect.height = Mathf.Round(rect.height);
			float num = (float)atlas.width * 0.25f;
			num = Mathf.Floor(GenMath.Min(num, rect.height / 2f, rect.width / 2f));
			GUI.BeginGroup(rect);
			Rect drawRect;
			Rect uvRect;
			if (drawTop)
			{
				drawRect = new Rect(0f, 0f, num, num);
				uvRect = new Rect(0f, 0f, 0.25f, 0.25f);
				Widgets.DrawTexturePart(drawRect, uvRect, atlas);
				drawRect = new Rect(rect.width - num, 0f, num, num);
				uvRect = new Rect(0.75f, 0f, 0.25f, 0.25f);
				Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			}
			drawRect = new Rect(0f, rect.height - num, num, num);
			uvRect = new Rect(0f, 0.75f, 0.25f, 0.25f);
			Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(rect.width - num, rect.height - num, num, num);
			uvRect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);
			Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(num, num, rect.width - num * 2f, rect.height - num * 2f);
			if (!drawTop)
			{
				drawRect.height += num;
				drawRect.y -= num;
			}
			uvRect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
			Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			if (drawTop)
			{
				drawRect = new Rect(num, 0f, rect.width - num * 2f, num);
				uvRect = new Rect(0.25f, 0f, 0.5f, 0.25f);
				Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			}
			drawRect = new Rect(num, rect.height - num, rect.width - num * 2f, num);
			uvRect = new Rect(0.25f, 0.75f, 0.5f, 0.25f);
			Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(0f, num, num, rect.height - num * 2f);
			if (!drawTop)
			{
				drawRect.height += num;
				drawRect.y -= num;
			}
			uvRect = new Rect(0f, 0.25f, 0.25f, 0.5f);
			Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			drawRect = new Rect(rect.width - num, num, num, rect.height - num * 2f);
			if (!drawTop)
			{
				drawRect.height += num;
				drawRect.y -= num;
			}
			uvRect = new Rect(0.75f, 0.25f, 0.25f, 0.5f);
			Widgets.DrawTexturePart(drawRect, uvRect, atlas);
			GUI.EndGroup();
		}

		public static Rect ToUVRect(this Rect r, Vector2 texSize)
		{
			return new Rect(r.x / texSize.x, r.y / texSize.y, r.width / texSize.x, r.height / texSize.y);
		}

		public static void DrawTexturePart(Rect drawRect, Rect uvRect, Texture2D tex)
		{
			uvRect.y = 1f - uvRect.y - uvRect.height;
			GUI.DrawTextureWithTexCoords(drawRect, tex, uvRect);
		}

		public static void ScrollHorizontal(Rect outRect, ref Vector2 scrollPosition, Rect viewRect, float ScrollWheelSpeed = 20f)
		{
			if (Event.current.type == EventType.ScrollWheel && Mouse.IsOver(outRect))
			{
				scrollPosition.x += Event.current.delta.y * ScrollWheelSpeed;
				float num = 1f;
				float num2 = viewRect.width - outRect.width - 1f;
				if (scrollPosition.x < num)
				{
					scrollPosition.x = num;
				}
				if (scrollPosition.x > num2)
				{
					scrollPosition.x = num2;
				}
				Event.current.Use();
			}
		}

		public static void BeginScrollView(Rect outRect, ref Vector2 scrollPosition, Rect viewRect)
		{
			Vector2 vector = scrollPosition;
			Vector2 vector2 = GUI.BeginScrollView(outRect, scrollPosition, viewRect);
			Vector2 vector3;
			if (Event.current.type == EventType.MouseDown)
			{
				vector3 = vector;
			}
			else
			{
				vector3 = vector2;
			}
			if (Event.current.type == EventType.ScrollWheel && Mouse.IsOver(outRect))
			{
				vector3 += Event.current.delta * 40f;
				Event.current.Use();
			}
			scrollPosition = vector3;
		}

		public static void EndScrollView()
		{
			GUI.EndScrollView();
		}

		public static void DrawHighlightSelected(Rect rect)
		{
			GUI.DrawTexture(rect, TexUI.HighlightSelectedTex);
		}

		public static void DrawHighlightIfMouseover(Rect rect)
		{
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
		}

		public static void DrawHighlight(Rect rect)
		{
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}

		public static bool InfoCardButton(float x, float y, Thing thing)
		{
			IConstructible constructible = thing as IConstructible;
			if (constructible != null)
			{
				ThingDef thingDef = thing.def.entityDefToBuild as ThingDef;
				if (thingDef != null)
				{
					return Widgets.InfoCardButton(x, y, thingDef, constructible.UIStuff());
				}
				return Widgets.InfoCardButton(x, y, thing.def.entityDefToBuild);
			}
			else
			{
				if (Widgets.InfoCardButtonWorker(x, y))
				{
					Find.WindowStack.Add(new Dialog_InfoCard(thing));
					return true;
				}
				return false;
			}
		}

		public static bool InfoCardButton(float x, float y, Def def)
		{
			if (Widgets.InfoCardButtonWorker(x, y))
			{
				Find.WindowStack.Add(new Dialog_InfoCard(def));
				return true;
			}
			return false;
		}

		public static bool InfoCardButton(float x, float y, ThingDef thingDef, ThingDef stuffDef)
		{
			if (Widgets.InfoCardButtonWorker(x, y))
			{
				Find.WindowStack.Add(new Dialog_InfoCard(thingDef, stuffDef));
				return true;
			}
			return false;
		}

		private static bool InfoCardButtonWorker(float x, float y)
		{
			Rect rect = new Rect(x, y, 24f, 24f);
			TooltipHandler.TipRegion(rect, "DefInfoTip".Translate());
			bool result = Widgets.ButtonImage(rect, TexButton.Info);
			UIHighlighter.HighlightOpportunity(rect, "InfoCard");
			return result;
		}

		public static void DrawTextureFitted(Rect outerRect, Texture2D tex, float scale)
		{
			Widgets.DrawTextureFitted(outerRect, tex, scale, new Vector2((float)tex.width, (float)tex.height), new Rect(0f, 0f, 1f, 1f));
		}

		public static void DrawTextureFitted(Rect outerRect, Texture2D tex, float scale, Vector2 texProportions, Rect texCoords)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Rect position = new Rect(0f, 0f, texProportions.x, texProportions.y);
			float num;
			if (position.width / position.height < outerRect.width / outerRect.height)
			{
				num = outerRect.height / position.height;
			}
			else
			{
				num = outerRect.width / position.width;
			}
			num *= scale;
			position.width *= num;
			position.height *= num;
			position.x = outerRect.x + outerRect.width / 2f - position.width / 2f;
			position.y = outerRect.y + outerRect.height / 2f - position.height / 2f;
			GUI.DrawTextureWithTexCoords(position, tex, texCoords);
		}

		public static void DrawTextureRotated(Vector2 center, Texture2D tex, float angle, float scale = 1f)
		{
			float num = (float)tex.width * scale;
			float num2 = (float)tex.height * scale;
			Rect rect = new Rect(center.x - num / 2f, center.y - num2 / 2f, num, num2);
			Widgets.DrawTextureRotated(rect, tex, angle);
		}

		public static void DrawTextureRotated(Rect rect, Texture2D tex, float angle)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			Matrix4x4 matrix = GUI.matrix;
			UI.RotateAroundPivot(angle, rect.center);
			GUI.DrawTexture(rect, tex);
			GUI.matrix = matrix;
		}
	}
}
