using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class HediffCompProperties
	{
		public Type compClass;

		public int healIntervalTicksStanding = 50;

		public int healIntervalTicksInBed = 50;

		public IntRange disappearsAfterTicks = default(IntRange);

		public int tendDuration = -1;

		[LoadAlias("labelTreated")]
		public string labelTended;

		[LoadAlias("labelTreatedWell")]
		public string labelTendedWell;

		[LoadAlias("labelTreatedInner")]
		public string labelTendedInner;

		[LoadAlias("labelTreatedWellInner")]
		public string labelTendedWellInner;

		[LoadAlias("labelSolidTreated")]
		public string labelSolidTended;

		[LoadAlias("labelSolidTreatedWell")]
		public string labelSolidTendedWell;

		public bool tendAllAtOnce;

		public int disappearsAtTendedCount = -1;

		public float infectionChance = 0.1f;

		public List<VerbProperties> verbs;

		public bool sendLetterWhenDiscovered;

		public string discoverLetterLabel;

		public string discoverLetterText;

		public float severityPerDayTendedOffset;

		public float severityPerDay;

		public float immunityPerDayNotSick;

		public float immunityPerDaySick;

		public float severityPerDayNotImmune;

		public float severityPerDayImmune;

		public FloatRange severityPerDayNotImmuneRandomFactor = new FloatRange(1f, 1f);

		public float severityPerDayGrowing;

		public float severityPerDayRemission;

		public FloatRange severityPerDayGrowingRandomFactor = new FloatRange(1f, 1f);

		public FloatRange severityPerDayRemissionRandomFactor = new FloatRange(1f, 1f);

		public float becomeOldChance = 1f;

		public string oldLabel;

		public string instantlyOldLabel;

		public EffecterDef stateEffecter;

		public IntRange severityIndices = new IntRange(-1, -1);

		public ChemicalDef chemical;

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			if (this.compClass == null)
			{
				yield return "compClass is null";
			}
			for (int i = 0; i < parentDef.comps.Count; i++)
			{
				if (parentDef.comps[i] != this && parentDef.comps[i].compClass == this.compClass)
				{
					yield return "two comps with same compClass: " + this.compClass;
				}
			}
			if (this.severityPerDayTendedOffset != 0f && this.compClass != typeof(HediffComp_Tendable) && this.compClass != typeof(HediffComp_GrowthMode))
			{
				yield return "non-zero severityPerDayTendedOffset won't do anything since compClass is " + this.compClass;
			}
		}
	}
}
