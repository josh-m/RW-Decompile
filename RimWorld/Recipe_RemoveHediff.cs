using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class Recipe_RemoveHediff : Recipe_Surgery
	{
		[DebuggerHidden]
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < allHediffs.Count; i++)
			{
				if (allHediffs[i].Part != null)
				{
					if (allHediffs[i].def == recipe.removesHediff)
					{
						yield return allHediffs[i].Part;
					}
				}
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
		{
			if (billDoer != null)
			{
				if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
				{
					billDoer,
					pawn
				});
				if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
				{
					string text;
					if (!this.recipe.successfullyRemovedHediffMessage.NullOrEmpty())
					{
						text = string.Format(this.recipe.successfullyRemovedHediffMessage, billDoer.LabelShort, pawn.LabelShort);
					}
					else
					{
						text = "MessageSuccessfullyRemovedHediff".Translate(new object[]
						{
							billDoer.LabelShort,
							pawn.LabelShort,
							this.recipe.removesHediff.label
						});
					}
					Messages.Message(text, pawn, MessageSound.Benefit);
				}
			}
			Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == this.recipe.removesHediff && x.Part == part);
			pawn.health.RemoveHediff(hediff);
		}
	}
}
