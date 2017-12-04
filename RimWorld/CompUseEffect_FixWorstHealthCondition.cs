using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompUseEffect_FixWorstHealthCondition : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			Hediff hediff = this.FindLifeThreateningHediff(usedBy);
			if (hediff != null)
			{
				this.Cure(hediff);
				return;
			}
			if (HealthUtility.TicksUntilDeathDueToBloodLoss(usedBy) < 5000)
			{
				Hediff hediff2 = this.FindMostBleedingHediff(usedBy);
				if (hediff2 != null)
				{
					this.Cure(hediff2);
					return;
				}
			}
			Hediff hediff3 = this.FindImmunizableHediffWhichCanKill(usedBy);
			if (hediff3 != null)
			{
				this.Cure(hediff3);
				return;
			}
			Hediff hediff4 = this.FindCarcinoma(usedBy);
			if (hediff4 != null)
			{
				this.Cure(hediff4);
				return;
			}
			Hediff hediff5 = this.FindNonInjuryMiscBadHediff(usedBy, true);
			if (hediff5 != null)
			{
				this.Cure(hediff5);
				return;
			}
			Hediff hediff6 = this.FindNonInjuryMiscBadHediff(usedBy, false);
			if (hediff6 != null)
			{
				this.Cure(hediff6);
				return;
			}
			BodyPartRecord bodyPartRecord = this.FindBiggestMissingBodyPart(usedBy, 0.01f);
			if (bodyPartRecord != null)
			{
				this.Cure(bodyPartRecord, usedBy);
				return;
			}
			Hediff_Injury hediff_Injury = this.FindInjury(usedBy, usedBy.health.hediffSet.GetBrain());
			if (hediff_Injury != null)
			{
				this.Cure(hediff_Injury);
				return;
			}
			BodyPartRecord bodyPartRecord2 = this.FindBiggestMissingBodyPart(usedBy, 0f);
			if (bodyPartRecord2 != null)
			{
				this.Cure(bodyPartRecord2, usedBy);
				return;
			}
			Hediff_Addiction hediff_Addiction = this.FindAddiction(usedBy);
			if (hediff_Addiction != null)
			{
				this.Cure(hediff_Addiction);
				return;
			}
			Hediff_Injury hediff_Injury2 = this.FindOldInjury(usedBy);
			if (hediff_Injury2 != null)
			{
				this.Cure(hediff_Injury2);
				return;
			}
			Hediff_Injury hediff_Injury3 = this.FindInjury(usedBy, null);
			if (hediff_Injury3 != null)
			{
				this.Cure(hediff_Injury3);
				return;
			}
		}

		private Hediff FindLifeThreateningHediff(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.everCurableByItem)
				{
					HediffStage curStage = hediffs[i].CurStage;
					if (curStage != null && curStage.lifeThreatening)
					{
						float num2 = (hediffs[i].Part == null) ? 999f : hediffs[i].Part.coverageAbsWithChildren;
						if (hediff == null || num2 > num)
						{
							hediff = hediffs[i];
							num = num2;
						}
					}
				}
			}
			return hediff;
		}

		private Hediff FindMostBleedingHediff(Pawn pawn)
		{
			float num = 0f;
			Hediff hediff = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible)
				{
					float bleedRate = hediffs[i].BleedRate;
					if (bleedRate > 0f && (bleedRate > num || hediff == null))
					{
						num = bleedRate;
						hediff = hediffs[i];
					}
				}
			}
			return hediff;
		}

		private Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.everCurableByItem)
				{
					if (hediffs[i].TryGetComp<HediffComp_Immunizable>() != null)
					{
						if (this.CanKill(hediffs[i]))
						{
							float severity = hediffs[i].Severity;
							if (hediff == null || severity > num)
							{
								hediff = hediffs[i];
								num = severity;
							}
						}
					}
				}
			}
			return hediff;
		}

		private Hediff FindCarcinoma(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def == HediffDefOf.Carcinoma)
				{
					float num2 = (hediffs[i].Part == null) ? 999f : hediffs[i].Part.coverageAbsWithChildren;
					if (hediff == null || num2 > num)
					{
						hediff = hediffs[i];
						num = num2;
					}
				}
			}
			return hediff;
		}

		private Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.isBad && hediffs[i].def.everCurableByItem)
				{
					if (!(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && !(hediffs[i] is Hediff_Addiction) && !(hediffs[i] is Hediff_AddedPart))
					{
						if (!onlyIfCanKill || this.CanKill(hediffs[i]))
						{
							float num2 = (hediffs[i].Part == null) ? 999f : hediffs[i].Part.coverageAbsWithChildren;
							if (hediff == null || num2 > num)
							{
								hediff = hediffs[i];
								num = num2;
							}
						}
					}
				}
			}
			return hediff;
		}

		private BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
		{
			BodyPartRecord bodyPartRecord = null;
			foreach (Hediff_MissingPart current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
			{
				if (current.Part.coverageAbsWithChildren >= minCoverage)
				{
					if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(current.Part))
					{
						if (bodyPartRecord == null || current.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren)
						{
							bodyPartRecord = current.Part;
						}
					}
				}
			}
			return bodyPartRecord;
		}

		private Hediff_Addiction FindAddiction(Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
				if (hediff_Addiction != null && hediff_Addiction.Visible && hediff_Addiction.def.everCurableByItem)
				{
					return hediff_Addiction;
				}
			}
			return null;
		}

		private Hediff_Injury FindOldInjury(Pawn pawn)
		{
			Hediff_Injury hediff_Injury = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
				if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.IsOld())
				{
					if (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity)
					{
						hediff_Injury = hediff_Injury2;
					}
				}
			}
			return hediff_Injury;
		}

		private Hediff_Injury FindInjury(Pawn pawn, BodyPartRecord bodyPart = null)
		{
			Hediff_Injury hediff_Injury = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
				if (hediff_Injury2 != null && hediff_Injury2.Visible)
				{
					if (bodyPart == null || bodyPart == hediff_Injury2.Part)
					{
						if (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity)
						{
							hediff_Injury = hediff_Injury2;
						}
					}
				}
			}
			return hediff_Injury;
		}

		private void Cure(Hediff hediff)
		{
			Pawn pawn = hediff.pawn;
			pawn.health.RemoveHediff(hediff);
			if (hediff.def.cureAllAtOnceIfCuredByItem)
			{
				while (true)
				{
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def, false);
					if (firstHediffOfDef == null)
					{
						break;
					}
					pawn.health.RemoveHediff(firstHediffOfDef);
				}
			}
			Messages.Message("MessageHediffCuredByItem".Translate(new object[]
			{
				hediff.LabelBase
			}), pawn, MessageTypeDefOf.PositiveEvent);
		}

		private void Cure(BodyPartRecord part, Pawn pawn)
		{
			pawn.health.RestorePart(part, null, true);
			Messages.Message("MessageBodyPartCuredByItem".Translate(new object[]
			{
				part.def.label
			}), pawn, MessageTypeDefOf.PositiveEvent);
		}

		private bool CanKill(Hediff hediff)
		{
			if (hediff.def.stages != null)
			{
				for (int i = 0; i < hediff.def.stages.Count; i++)
				{
					if (hediff.def.stages[i].lifeThreatening)
					{
						return true;
					}
				}
			}
			return hediff.def.lethalSeverity >= 0f;
		}
	}
}
