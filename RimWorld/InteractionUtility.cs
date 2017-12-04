using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class InteractionUtility
	{
		public const float MaxInteractRange = 6f;

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		public static bool CanInitiateInteraction(Pawn pawn)
		{
			return pawn.interactions != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.Awake() && !pawn.IsBurning();
		}

		public static bool CanReceiveInteraction(Pawn pawn)
		{
			return pawn.Awake() && !pawn.IsBurning();
		}

		public static bool CanInitiateRandomInteraction(Pawn p)
		{
			return InteractionUtility.CanInitiateInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState && p.Faction != null;
		}

		public static bool CanReceiveRandomInteraction(Pawn p)
		{
			return InteractionUtility.CanReceiveInteraction(p) && p.RaceProps.Humanlike && !p.Downed && !p.InAggroMentalState;
		}

		public static bool IsGoodPositionForInteraction(Pawn p, Pawn recipient)
		{
			return InteractionUtility.IsGoodPositionForInteraction(p.Position, recipient.Position, p.Map);
		}

		public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
		{
			return cell.InHorDistOf(recipientCell, 6f) && GenSight.LineOfSight(cell, recipientCell, map, true, null, 0, 0);
		}

		public static bool HasAnyVerbForSocialFight(Pawn p)
		{
			if (p.Dead)
			{
				return false;
			}
			List<Verb> allVerbs = p.verbTracker.AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i] is Verb_MeleeAttack && allVerbs[i].IsStillUsableBy(p))
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryGetRandomVerbForSocialFight(Pawn p, out Verb verb)
		{
			if (p.Dead)
			{
				verb = null;
				return false;
			}
			List<Verb> allVerbs = p.verbTracker.AllVerbs;
			return (from x in allVerbs
			where x is Verb_MeleeAttack && x.IsStillUsableBy(p)
			select x).TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p, null), out verb);
		}

		public static bool HasAnySocialFightProvokingThought(Pawn pawn, Pawn otherPawn)
		{
			Thought thought;
			return InteractionUtility.TryGetRandomSocialFightProvokingThought(pawn, otherPawn, out thought);
		}

		public static bool TryGetRandomSocialFightProvokingThought(Pawn pawn, Pawn otherPawn, out Thought thought)
		{
			if (pawn.needs.mood == null)
			{
				thought = null;
				return false;
			}
			pawn.needs.mood.thoughts.GetSocialThoughts(otherPawn, InteractionUtility.tmpSocialThoughts);
			ISocialThought socialThought;
			bool result = InteractionUtility.tmpSocialThoughts.Where(delegate(ISocialThought x)
			{
				ThoughtDef def = ((Thought)x).def;
				return def != ThoughtDefOf.HadAngeringFight && def != ThoughtDefOf.HadCatharticFight && x.OpinionOffset() < 0f;
			}).TryRandomElementByWeight((ISocialThought x) => -x.OpinionOffset(), out socialThought);
			InteractionUtility.tmpSocialThoughts.Clear();
			thought = (Thought)socialThought;
			return result;
		}
	}
}
