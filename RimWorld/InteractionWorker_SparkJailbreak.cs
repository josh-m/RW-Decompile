using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class InteractionWorker_SparkJailbreak : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef)
		{
			if (!recipient.IsPrisoner || !recipient.guest.PrisonerIsSecure || !PrisonBreakUtility.CanParticipateInPrisonBreak(recipient))
			{
				letterText = null;
				letterLabel = null;
				letterDef = null;
				return;
			}
			PrisonBreakUtility.StartPrisonBreak(recipient, out letterText, out letterLabel, out letterDef);
			MentalState_Jailbreaker mentalState_Jailbreaker = initiator.MentalState as MentalState_Jailbreaker;
			if (mentalState_Jailbreaker != null)
			{
				mentalState_Jailbreaker.Notify_InducedPrisonerToEscape();
			}
		}
	}
}
