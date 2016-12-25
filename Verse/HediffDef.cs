using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public class HediffDef : Def
	{
		public Type hediffClass = typeof(Hediff);

		public List<HediffCompProperties> comps;

		public bool isBad = true;

		public float initialSeverity = 0.001f;

		public bool naturallyHealed;

		public float lethalSeverity = -1f;

		public List<HediffStage> stages;

		public ThingDef spawnThingOnRemoved;

		public bool tendable;

		public float chanceToCauseNoPain;

		public bool makesSickThought;

		public bool makesAlert = true;

		public NeedDef causesNeed;

		public float minSeverity;

		public float maxSeverity = 3.40282347E+38f;

		public bool scenarioCanAdd;

		public bool displayWound;

		public Color defaultLabelColor = Color.white;

		public InjuryProps injuryProps;

		public AddedBodyPartProps addedPartProps;

		public bool IsAddiction
		{
			get
			{
				return typeof(Hediff_Addiction).IsAssignableFrom(this.hediffClass);
			}
		}

		public HediffCompProperties CompPropsFor(Type compClass)
		{
			if (this.comps != null)
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					if (this.comps[i].compClass == compClass)
					{
						return this.comps[i];
					}
				}
			}
			return null;
		}

		public bool HasComp(Type compClass)
		{
			return this.CompPropsFor(compClass) != null;
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (!this.tendable && this.CompPropsFor(typeof(HediffComp_Tendable)) != null)
			{
				this.tendable = true;
			}
		}

		public bool PossibleToDevelopImmunity()
		{
			HediffCompProperties hediffCompProperties = this.CompPropsFor(typeof(HediffComp_Immunizable));
			return hediffCompProperties != null && (hediffCompProperties.immunityPerDayNotSick > 0f || hediffCompProperties.immunityPerDaySick > 0f);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string err in base.ConfigErrors())
			{
				yield return err;
			}
			if (this.hediffClass == null)
			{
				yield return "hediffClass is null";
			}
			if (!this.comps.NullOrEmpty<HediffCompProperties>() && !typeof(HediffWithComps).IsAssignableFrom(this.hediffClass))
			{
				yield return "has comps but hediffClass is not HediffWithComps or subclass thereof";
			}
			if (this.minSeverity > this.initialSeverity)
			{
				yield return "minSeverity is greater than initialSeverity";
			}
			if (this.maxSeverity < this.initialSeverity)
			{
				yield return "maxSeverity is lower than initialSeverity";
			}
			if (this.comps != null)
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					foreach (string compErr in this.comps[i].ConfigErrors(this))
					{
						yield return this.comps[i] + ": " + compErr;
					}
				}
			}
			if (this.stages != null)
			{
				if (!typeof(Hediff_Addiction).IsAssignableFrom(this.hediffClass))
				{
					for (int j = 0; j < this.stages.Count; j++)
					{
						if (j >= 1 && this.stages[j].minSeverity <= this.stages[j - 1].minSeverity)
						{
							yield return "stages are not in order of minSeverity";
						}
					}
				}
				for (int k = 0; k < this.stages.Count; k++)
				{
					if (this.stages[k].makeImmuneTo != null && !this.stages[k].makeImmuneTo.HasComp(typeof(HediffComp_Immunizable)))
					{
						yield return "makes immune to hediff which doesn't have comp immunizable";
					}
				}
			}
		}

		[DebuggerHidden]
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (this.stages != null && this.stages.Count == 1)
			{
				foreach (StatDrawEntry de in this.stages[0].SpecialDisplayStats())
				{
					yield return de;
				}
			}
		}

		public static HediffDef Named(string defName)
		{
			return DefDatabase<HediffDef>.GetNamed(defName, true);
		}
	}
}
