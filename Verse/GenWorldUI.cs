using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class GenWorldUI
	{
		public const float NameBGHeight = 12f;

		public const float NameBGExtraWidth = 4f;

		public const float LabelOffsetYStandard = -0.4f;

		public static readonly Texture2D OverlayHealthTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0f, 0.25f));

		public static readonly Color DefaultThingLabelColor = new Color(1f, 1f, 1f, 0.75f);

		public static Vector2 LabelDrawPosFor(Thing thing, float worldOffsetZ)
		{
			Vector3 drawPos = thing.DrawPos;
			drawPos.z += worldOffsetZ;
			Vector2 result = Find.Camera.WorldToScreenPoint(drawPos);
			result.y = (float)Screen.height - result.y;
			return result;
		}

		public static Vector2 LabelDrawPosFor(IntVec3 center)
		{
			Vector3 position = center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			Vector2 result = Find.Camera.WorldToScreenPoint(position);
			result.y = (float)Screen.height - result.y;
			result.y -= 1f;
			return result;
		}

		public static void DrawThingLabel(Thing thing, string text)
		{
			GenWorldUI.DrawThingLabel(thing, text, GenWorldUI.DefaultThingLabelColor);
		}

		public static void DrawThingLabel(Thing thing, string text, Color textColor)
		{
			GenWorldUI.DrawThingLabel(GenWorldUI.LabelDrawPosFor(thing, -0.4f), text, textColor);
		}

		public static void DrawThingLabel(Vector2 screenPos, string text, Color textColor)
		{
			Text.Font = GameFont.Tiny;
			float x = Text.CalcSize(text).x;
			Rect position = new Rect(screenPos.x - x / 2f - 4f, screenPos.y, x + 8f, 12f);
			GUI.DrawTexture(position, TexUI.GrayTextBG);
			GUI.color = textColor;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect = new Rect(screenPos.x - x / 2f, screenPos.y - 3f, x, 999f);
			Widgets.Label(rect, text);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public static void DrawPawnLabel(Pawn pawn, Vector2 pos, float alpha = 1f, float truncateToWidth = 9999f, Dictionary<string, string> truncatedLabelsCache = null)
		{
			GUI.color = new Color(1f, 1f, 1f, alpha);
			Text.Font = GameFont.Tiny;
			string text = pawn.NameStringShort.CapitalizeFirst().Truncate(truncateToWidth, truncatedLabelsCache);
			float num = text.GetWidthCached();
			if (num < 20f)
			{
				num = 20f;
			}
			Rect rect = new Rect(pos.x - num / 2f - 4f, pos.y, num + 8f, 12f);
			if (!pawn.RaceProps.Humanlike)
			{
				rect.y -= 4f;
			}
			GUI.DrawTexture(rect, TexUI.GrayTextBG);
			if (pawn.health.summaryHealth.SummaryHealthPercent < 0.999f)
			{
				Rect rect2 = rect.ContractedBy(1f);
				Widgets.FillableBar(rect2, pawn.health.summaryHealth.SummaryHealthPercent, GenWorldUI.OverlayHealthTex, BaseContent.ClearTex, false);
			}
			Color color = PawnNameColorUtility.PawnNameColorOf(pawn);
			color.a = alpha;
			GUI.color = color;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect3 = new Rect(rect.center.x - num / 2f, rect.y - 2f, num, 100f);
			Widgets.Label(rect3, text);
			if (pawn.Drafted)
			{
				Widgets.DrawLineHorizontal(pos.x - num / 2f, pos.y + 11f, num);
			}
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public static void DrawText(Vector2 worldPos, string text, Color textColor)
		{
			Vector3 position = new Vector3(worldPos.x, 0f, worldPos.y);
			Vector2 vector = Find.Camera.WorldToScreenPoint(position);
			vector.y = (float)Screen.height - vector.y;
			Text.Font = GameFont.Tiny;
			GUI.color = textColor;
			Text.Anchor = TextAnchor.UpperCenter;
			float x = Text.CalcSize(text).x;
			Widgets.Label(new Rect(vector.x - x / 2f, vector.y - 2f, x, 999f), text);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
