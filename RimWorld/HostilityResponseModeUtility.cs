using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class HostilityResponseModeUtility
	{
		private static readonly Texture2D IgnoreIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Ignore", true);

		private static readonly Texture2D AttackIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Attack", true);

		private static readonly Texture2D FleeIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Flee", true);

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

		public static void DrawResponseButton(Rect rect, Pawn pawn, bool paintable)
		{
			Func<Pawn, HostilityResponseMode> getPayload = new Func<Pawn, HostilityResponseMode>(HostilityResponseModeUtility.DrawResponseButton_GetResponse);
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>>>(HostilityResponseModeUtility.DrawResponseButton_GenerateMenu);
			Texture2D icon = pawn.playerSettings.hostilityResponse.GetIcon();
			Widgets.Dropdown<Pawn, HostilityResponseMode>(rect, pawn, getPayload, menuGenerator, null, icon, null, null, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HostilityResponse, KnowledgeAmount.SpecificInteraction);
			}, paintable);
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

		private static HostilityResponseMode DrawResponseButton_GetResponse(Pawn pawn)
		{
			return pawn.playerSettings.hostilityResponse;
		}

		[DebuggerHidden]
		private static IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>> DrawResponseButton_GenerateMenu(Pawn p)
		{
			foreach (HostilityResponseMode response in Enum.GetValues(typeof(HostilityResponseMode)))
			{
				if (response != HostilityResponseMode.Attack || !p.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					yield return new Widgets.DropdownMenuElement<HostilityResponseMode>
					{
						option = new FloatMenuOption(response.GetLabel(), delegate
						{
							p.playerSettings.hostilityResponse = response;
						}, MenuOptionPriority.Default, null, null, 0f, null, null),
						payload = response
					};
				}
			}
		}
	}
}
