using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class InteractionWorker_SparkJailbreak : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			if (!recipient.IsPrisoner || !recipient.guest.PrisonerIsSecure || !PrisonBreakUtility.CanParticipateInPrisonBreak(recipient))
			{
				return;
			}
			PrisonBreakUtility.StartPrisonBreak(recipient);
			MentalState_Jailbreaker mentalState_Jailbreaker = initiator.MentalState as MentalState_Jailbreaker;
			if (mentalState_Jailbreaker != null)
			{
				mentalState_Jailbreaker.Notify_InducedPrisonerToEscape();
			}
		}
	}
}
