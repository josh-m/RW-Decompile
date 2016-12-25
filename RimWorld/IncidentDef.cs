using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class IncidentDef : Def
	{
		public Type workerClass;

		public IncidentTargetType targetType;

		public float baseChance;

		public IncidentCategory category = IncidentCategory.Misc;

		public IncidentPopulationEffect populationEffect = IncidentPopulationEffect.None;

		public int earliestDay;

		public int minRefireDays;

		public int minDifficulty;

		public bool pointsScaleable;

		public float minThreatPoints = -1f;

		public List<BiomeDef> allowedBiomes;

		public List<string> tags;

		public List<string> refireCheckTags;

		[MustTranslate]
		public string letterText;

		[MustTranslate]
		public string letterLabel;

		public LetterType letterType;

		public MapConditionDef mapCondition;

		public FloatRange durationDays;

		public List<PawnKindDef> pawnKinds;

		public HediffDef diseaseIncident;

		public FloatRange diseaseVictimFractionRange = new FloatRange(0f, 0.49f);

		public int diseaseMaxVictims = 99999;

		public List<BiomeDiseaseRecord> diseaseBiomeRecords;

		public List<BodyPartDef> diseasePartsToAffect;

		public ThingDef shipPart;

		public List<MTBByBiome> mtbDaysByBiome;

		[Unsaved]
		private IncidentWorker workerInt;

		[Unsaved]
		private List<IncidentDef> cachedRefireCheckIncidents;

		public bool NeedsParms
		{
			get
			{
				return this.category == IncidentCategory.ThreatSmall || this.category == IncidentCategory.ThreatBig;
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
			if (this.targetType == IncidentTargetType.None)
			{
				yield return "no target type";
			}
		}

		public bool TargetTypeAllowed(IncidentTargetType target)
		{
			return (byte)(this.targetType & target) != 0;
		}

		public bool TargetAllowed(IIncidentTarget target)
		{
			Map map = target as Map;
			if (map != null)
			{
				if (this.TargetTypeAllowed(IncidentTargetType.BaseMap) && map.IsPlayerHome)
				{
					return true;
				}
				if (this.TargetTypeAllowed(IncidentTargetType.TempMap) && !map.IsPlayerHome)
				{
					return true;
				}
			}
			return this.TargetTypeAllowed(IncidentTargetType.Caravan) && target is Caravan;
		}
	}
}
