using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SummaryHealthHandler
	{
		private Pawn pawn;

		private float cachedSummaryHealthPercent = 1f;

		private bool dirty = true;

		public float SummaryHealthPercent
		{
			get
			{
				if (this.pawn.Dead)
				{
					return 0f;
				}
				if (this.dirty)
				{
					ProfilerThreadCheck.BeginSample("Recache summary health percent");
					List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
					float num = 1f;
					for (int i = 0; i < hediffs.Count; i++)
					{
						if (!(hediffs[i] is Hediff_MissingPart) && hediffs[i].Visible)
						{
							float num2 = hediffs[i].SummaryHealthPercentImpact;
							if (num2 > 0.95f)
							{
								num2 = 0.95f;
							}
							num *= 1f - num2;
						}
					}
					List<Hediff_MissingPart> missingPartsCommonAncestors = this.pawn.health.hediffSet.GetMissingPartsCommonAncestors();
					for (int j = 0; j < missingPartsCommonAncestors.Count; j++)
					{
						Hediff_MissingPart hediff_MissingPart = missingPartsCommonAncestors[j];
						if (!hediff_MissingPart.Part.def.Activities.NullOrEmpty<Pair<PawnCapacityDef, string>>() || !hediff_MissingPart.Part.parts.NullOrEmpty<BodyPartRecord>())
						{
							float num3 = hediff_MissingPart.SummaryHealthPercentImpact;
							if (num3 > 0.95f)
							{
								num3 = 0.95f;
							}
							num *= 1f - num3;
						}
					}
					this.cachedSummaryHealthPercent = Mathf.Clamp(num, 0.05f, 1f);
					ProfilerThreadCheck.EndSample();
					this.dirty = false;
				}
				return this.cachedSummaryHealthPercent;
			}
		}

		public SummaryHealthHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void Notify_HealthChanged()
		{
			this.dirty = true;
		}
	}
}
