using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class StorytellerUI
	{
		private static Vector2 scrollPosition = default(Vector2);

		private static readonly Texture2D StorytellerHighlightTex = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/Highlight", true);

		internal static void DrawStorytellerSelectionInterface(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty)
		{
			GUI.BeginGroup(rect);
			if (chosenStoryteller != null && chosenStoryteller.listVisible)
			{
				Rect position = new Rect(390f, rect.height - Storyteller.PortraitSizeLarge.y - 1f, Storyteller.PortraitSizeLarge.x, Storyteller.PortraitSizeLarge.y);
				GUI.DrawTexture(position, chosenStoryteller.portraitLargeTex);
				Widgets.DrawLineHorizontal(0f, rect.height, rect.width);
			}
			Rect outRect = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x + 16f, rect.height);
			Rect viewRect = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x, (float)DefDatabase<StorytellerDef>.AllDefs.Count<StorytellerDef>() * (Storyteller.PortraitSizeTiny.y + 10f));
			Widgets.BeginScrollView(outRect, ref StorytellerUI.scrollPosition, viewRect);
			Rect rect2 = new Rect(0f, 0f, Storyteller.PortraitSizeTiny.x, Storyteller.PortraitSizeTiny.y);
			foreach (StorytellerDef current in from tel in DefDatabase<StorytellerDef>.AllDefs
			orderby tel.listOrder
			select tel)
			{
				if (current.listVisible)
				{
					if (Widgets.ButtonImage(rect2, current.portraitTinyTex))
					{
						TutorSystem.Notify_Event("ChooseStoryteller");
						chosenStoryteller = current;
					}
					if (chosenStoryteller == current)
					{
						GUI.DrawTexture(rect2, StorytellerUI.StorytellerHighlightTex);
					}
					rect2.y += rect2.height + 8f;
				}
			}
			Widgets.EndScrollView();
			Text.Font = GameFont.Small;
			Rect rect3 = new Rect(outRect.xMax + 8f, 0f, 240f, 999f);
			Widgets.Label(rect3, "HowStorytellersWork".Translate());
			if (chosenStoryteller != null && chosenStoryteller.listVisible)
			{
				Rect rect4 = new Rect(outRect.xMax + 8f, outRect.yMin + 200f, 290f, rect.height - 300f);
				Text.Font = GameFont.Medium;
				Rect rect5 = new Rect(rect4.x + 15f, rect4.y - 40f, 9999f, 40f);
				Widgets.Label(rect5, chosenStoryteller.label);
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
				Listing_Standard listing_Standard = new Listing_Standard(rect4);
				listing_Standard.Label(chosenStoryteller.description);
				listing_Standard.Gap(6f);
				foreach (DifficultyDef current2 in DefDatabase<DifficultyDef>.AllDefs)
				{
					Rect rect6 = listing_Standard.GetRect(30f);
					if (Mouse.IsOver(rect6))
					{
						Widgets.DrawHighlight(rect6);
					}
					TooltipHandler.TipRegion(rect6, current2.description);
					if (Widgets.RadioButtonLabeled(rect6, current2.LabelCap, difficulty == current2))
					{
						difficulty = current2;
					}
				}
				listing_Standard.Gap(30f);
				if (Current.ProgramState == ProgramState.Entry)
				{
					listing_Standard.CheckboxLabeled("PermadeathMode".Translate(), ref Find.GameInitData.permadeath, "PermadeathModeInfo".Translate());
				}
				listing_Standard.End();
			}
			GUI.EndGroup();
		}
	}
}
