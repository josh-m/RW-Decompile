using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class HostilityResponseModeUtility
	{
		private static readonly Texture2D IgnoreIcon = Resources.Load<Texture2D>("Textures/UI/Icons/HostilityResponse/Ignore");

		private static readonly Texture2D AttackIcon = Resources.Load<Texture2D>("Textures/UI/Icons/HostilityResponse/Attack");

		private static readonly Texture2D FleeIcon = Resources.Load<Texture2D>("Textures/UI/Icons/HostilityResponse/Flee");

		public static Texture2D GetIcon(this HostilityResponseMode response)
		{
			switch (response)
			{
			case HostilityResponseMode.Ignore:
				return HostilityResponseModeUtility.IgnoreIcon;
			case HostilityResponseMode.Attack:
				return HostilityResponseModeUtility.AttackIcon;
			case HostilityResponseMode.Flee:
				return HostilityResponseModeUtility.FleeIcon;
			default:
				return BaseContent.BadTex;
			}
		}

		public static HostilityResponseMode GetNextResponse(Pawn pawn)
		{
			switch (pawn.playerSettings.hostilityResponse)
			{
			case HostilityResponseMode.Ignore:
				if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					return HostilityResponseMode.Flee;
				}
				return HostilityResponseMode.Attack;
			case HostilityResponseMode.Attack:
				return HostilityResponseMode.Flee;
			case HostilityResponseMode.Flee:
				return HostilityResponseMode.Ignore;
			default:
				return HostilityResponseMode.Ignore;
			}
		}

		public static string GetLabel(this HostilityResponseMode response)
		{
			return ("HostilityResponseMode_" + response).Translate();
		}

		public static void DrawResponseButton(Vector2 pos, Pawn pawn)
		{
			Texture2D icon = pawn.playerSettings.hostilityResponse.GetIcon();
			Rect rect = new Rect(pos.x, pos.y, 24f, 24f);
			if (Widgets.ButtonImage(rect, icon))
			{
				pawn.playerSettings.hostilityResponse = HostilityResponseModeUtility.GetNextResponse(pawn);
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HostilityResponse, KnowledgeAmount.SpecificInteraction);
			}
			UIHighlighter.HighlightOpportunity(rect, "HostilityResponse");
			TooltipHandler.TipRegion(rect, string.Concat(new string[]
			{
				"HostilityReponseTip".Translate(),
				"\n\n",
				"HostilityResponseCurrentMode".Translate(),
				": ",
				pawn.playerSettings.hostilityResponse.GetLabel()
			}));
		}
	}
}
