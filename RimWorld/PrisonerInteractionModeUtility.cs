using System;
using Verse;

namespace RimWorld
{
	public static class PrisonerInteractionModeUtility
	{
		public static string GetLabel(this PrisonerInteractionMode mode)
		{
			switch (mode)
			{
			case PrisonerInteractionMode.NoInteraction:
				return "PrisonerNoInteraction".Translate();
			case PrisonerInteractionMode.Chat:
				return "PrisonerFriendlyChat".Translate();
			case PrisonerInteractionMode.AttemptRecruit:
				return "PrisonerAttemptRecruit".Translate();
			case PrisonerInteractionMode.Release:
				return "PrisonerRelease".Translate();
			case PrisonerInteractionMode.Execution:
				return "PrisonerExecution".Translate();
			default:
				return "Mode needs label";
			}
		}
	}
}
