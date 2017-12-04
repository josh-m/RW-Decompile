using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class MentalBreakWorker
	{
		public MentalBreakDef def;

		public virtual float CommonalityFor(Pawn pawn)
		{
			return this.def.baseCommonality;
		}

		public virtual bool BreakCanOccur(Pawn pawn)
		{
			if (this.def.requiredTrait != null && (pawn.story == null || !pawn.story.traits.HasTrait(this.def.requiredTrait)))
			{
				return false;
			}
			if (this.def.mentalState != null && pawn.story != null && pawn.story.traits.allTraits.Any((Trait tr) => tr.CurrentData.disallowedMentalStates != null && tr.CurrentData.disallowedMentalStates.Contains(this.def.mentalState)))
			{
				return false;
			}
			if (this.def.mentalState != null && !this.def.mentalState.Worker.StateCanOccur(pawn))
			{
				return false;
			}
			if (pawn.story != null)
			{
				IEnumerable<MentalBreakDef> theOnlyAllowedMentalBreaks = pawn.story.traits.TheOnlyAllowedMentalBreaks;
				if (!theOnlyAllowedMentalBreaks.Contains(this.def) && theOnlyAllowedMentalBreaks.Any((MentalBreakDef x) => x.intensity == this.def.intensity && x.Worker.BreakCanOccur(pawn)))
				{
					return false;
				}
			}
			return !TutorSystem.TutorialMode || pawn.Faction != Faction.OfPlayer;
		}

		public virtual bool TryStart(Pawn pawn, Thought reason, bool causedByMood)
		{
			string text = (reason == null) ? null : reason.LabelCap;
			MentalStateHandler arg_33_0 = pawn.mindState.mentalStateHandler;
			MentalStateDef mentalState = this.def.mentalState;
			string reason2 = text;
			return arg_33_0.TryStartMentalState(mentalState, reason2, false, causedByMood, null);
		}

		protected bool TrySendLetter(Pawn pawn, string textKey, Thought reason)
		{
			if (!PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				return false;
			}
			string label = "MentalBreakLetterLabel".Translate() + ": " + this.def.label;
			string text = textKey.Translate(new object[]
			{
				pawn.Label
			}).CapitalizeFirst();
			if (reason != null)
			{
				text = text + "\n\n" + "FinalStraw".Translate(new object[]
				{
					reason.LabelCap
				});
			}
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn, null);
			return true;
		}
	}
}
