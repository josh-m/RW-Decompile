using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	internal class Recipe_RemoveBodyPart : Recipe_MedicalOperation
	{
		private const float ViolationGoodwillImpact = 20f;

		[DebuggerHidden]
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(null, null);
			foreach (BodyPartRecord part in parts)
			{
				if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
				{
					yield return part;
				}
				if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
				{
					yield return part;
				}
				if (part != pawn.RaceProps.body.corePart && !part.def.dontSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == this.<part>__2))
				{
					yield return part;
				}
			}
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			return pawn.Faction != billDoerFaction && HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
		{
			bool flag = MedicalRecipesUtility.IsClean(pawn, part);
			bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (base.CheckSurgeryFail(billDoer, pawn, ingredients))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
				{
					billDoer,
					pawn
				});
				MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position);
				MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position);
			}
			BodyPartDamageInfo value = new BodyPartDamageInfo(part, false, null);
			DamageInfo dinfo = new DamageInfo(DamageDefOf.SurgicalCut, 99999, null, new BodyPartDamageInfo?(value), null);
			pawn.TakeDamage(dinfo);
			if (flag)
			{
				if (pawn.Dead)
				{
					ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.OrganHarvesting);
				}
				else
				{
					ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
				}
			}
			if (flag2)
			{
				pawn.Faction.AffectGoodwillWith(billDoer.Faction, -20f);
			}
		}
	}
}
