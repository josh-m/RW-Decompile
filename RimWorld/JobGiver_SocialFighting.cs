using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SocialFighting : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.RaceProps.Humanlike && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			Pawn otherPawn = ((MentalState_SocialFighting)pawn.MentalState).otherPawn;
			Verb verbToUse;
			if (!InteractionUtility.TryGetRandomVerbForSocialFight(pawn, out verbToUse))
			{
				return null;
			}
			return new Job(JobDefOf.SocialFight, otherPawn)
			{
				maxNumMeleeAttacks = 1,
				verbToUse = verbToUse
			};
		}
	}
}
