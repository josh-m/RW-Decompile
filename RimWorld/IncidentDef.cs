using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentDef : Def
	{
		public Type workerClass;

		public IncidentCategory category;

		public List<IncidentTargetTypeDef> targetTypes;

		public float baseChance;

		public IncidentPopulationEffect populationEffect;

		public int earliestDay;

		public int minPopulation;

		public float minRefireDays;

		public int minDifficulty;

		public bool pointsScaleable;

		public float minThreatPoints = -1f;

		public List<BiomeDef> allowedBiomes;

		public List<string> tags;

		public List<string> refireCheckTags;

		public SimpleCurve chanceFactorByPopulationCurve;

		[MustTranslate]
		public string letterText;

		[MustTranslate]
		public string letterLabel;

		public LetterDef letterDef;

		public GameConditionDef gameCondition;

		public FloatRange durationDays;

		public HediffDef diseaseIncident;

		public FloatRange diseaseVictimFractionRange = new FloatRange(0f, 0.49f);

		public int diseaseMaxVictims = 99999;

		public List<BiomeDiseaseRecord> diseaseBiomeRecords;

		public List<BodyPartDef> diseasePartsToAffect;

		public ThingDef shipPart;

		public List<MTBByBiome> mtbDaysByBiome;

		public TaleDef tale;

		[Unsaved]
		private IncidentWorker workerInt;

		[Unsaved]
		private List<IncidentDef> cachedRefireCheckIncidents;

		public bool NeedsParms
		{
			get
			{
				return this.category == IncidentCategory.ThreatSmall || this.category == IncidentCategory.ThreatBig || this.category == IncidentCategory.RaidBeacon;
			}
		}

		public IncidentWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (IncidentWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public List<IncidentDef> RefireCheckIncidents
		{
			get
			{
				if (this.refireCheckTags == null)
				{
					return null;
				}
				if (this.cachedRefireCheckIncidents == null)
				{
					this.cachedRefireCheckIncidents = new List<IncidentDef>();
					List<IncidentDef> allDefsListForReading = DefDatabase<IncidentDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (this.ShouldDoRefireCheckWith(allDefsListForReading[i]))
						{
							this.cachedRefireCheckIncidents.Add(allDefsListForReading[i]);
						}
					}
				}
				return this.cachedRefireCheckIncidents;
			}
		}

		public static IncidentDef Named(string defName)
		{
			return DefDatabase<IncidentDef>.GetNamed(defName, true);
		}

		private bool ShouldDoRefireCheckWith(IncidentDef other)
		{
			if (other.tags == null)
			{
				return false;
			}
			if (other == this)
			{
				return false;
			}
			for (int i = 0; i < other.tags.Count; i++)
			{
				for (int j = 0; j < this.refireCheckTags.Count; j++)
				{
					if (other.tags[i] == this.refireCheckTags[j])
					{
						return true;
					}
				}
			}
			return false;
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string c in base.ConfigErrors())
			{
				yield return c;
			}
			if (this.category == IncidentCategory.Undefined)
			{
				yield return "category is undefined.";
			}
			if (this.targetTypes == null || this.targetTypes.Count == 0)
			{
				yield return "no target type";
			}
			if (this.TargetTypeAllowed(IncidentTargetTypeDefOf.World))
			{
				if (this.targetTypes.Any((IncidentTargetTypeDef tt) => tt != IncidentTargetTypeDefOf.World))
				{
					yield return "allows world target type along with other targets. World targeting incidents should only target the world.";
				}
			}
			if (this.TargetTypeAllowed(IncidentTargetTypeDefOf.World) && this.allowedBiomes != null)
			{
				yield return "world-targeting incident has a biome restriction list";
			}
		}

		public bool TargetTypeAllowed(IncidentTargetTypeDef target)
		{
			return this.targetTypes.Contains(target);
		}

		public bool TargetAllowed(IIncidentTarget target)
		{
			return this.targetTypes.Intersect(target.AcceptedTypes()).Any<IncidentTargetTypeDef>();
		}
	}
}
