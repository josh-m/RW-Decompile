using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompProperties_Drug : CompProperties
	{
		public ChemicalDef chemical;

		public float addictiveness;

		public float minToleranceToAddict;

		public float existingAddictionSeverityOffset = 0.1f;

		public float needLevelOffset = 1f;

		public FloatRange overdoseSeverityOffset = FloatRange.Zero;

		public float largeOverdoseChance;

		public bool isCombatEnhancingDrug;

		public float listOrder;

		public bool Addictive
		{
			get
			{
				return this.addictiveness > 0f;
			}
		}

		public bool CanCauseOverdose
		{
			get
			{
				return this.overdoseSeverityOffset.TrueMax > 0f;
			}
		}

		public CompProperties_Drug()
		{
			this.compClass = typeof(CompDrug);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string e in base.ConfigErrors(parentDef))
			{
				yield return e;
			}
			if (this.Addictive && this.chemical == null)
			{
				yield return "addictive but chemical is null";
			}
		}

		[DebuggerHidden]
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry s in base.SpecialDisplayStats(req))
			{
				yield return s;
			}
			if (this.Addictive)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Addictiveness".Translate(), this.addictiveness.ToStringPercent(), 0, string.Empty);
			}
		}
	}
}
