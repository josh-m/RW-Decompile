using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class DamageDef : Def
	{
		public Type workerClass = typeof(DamageWorker);

		public bool externalViolence;

		public bool hasForcefulImpact = true;

		public bool harmsHealth = true;

		public bool makesBlood = true;

		public bool canInterruptJobs = true;

		[MustTranslate]
		public string deathMessage = "{0} has been killed.";

		public ImpactSoundTypeDef impactSoundType;

		public DamageArmorCategoryDef armorCategory;

		public bool spreadOut;

		public bool execution;

		public RulePackDef combatLogRules;

		public bool isExplosive;

		public int explosionDamage = 10;

		public float explosionSnowMeltAmount = 1f;

		public float explosionBuildingDamageFactor = 1f;

		public bool explosionAffectOutsidePartsOnly = true;

		public ThingDef explosionCellMote;

		public Color explosionColorCenter = Color.white;

		public Color explosionColorEdge = Color.white;

		public ThingDef explosionInteriorMote;

		public float explosionHeatEnergyPerCell;

		public SoundDef soundExplosion;

		public bool harmAllLayersUntilOutside;

		public HediffDef hediff;

		public HediffDef hediffSkin;

		public HediffDef hediffSolid;

		public float stabChanceOfForcedInternal;

		public float stabPierceBonus;

		public SimpleCurve cutExtraTargetsCurve;

		public float cutCleaveBonus;

		public float bluntInnerHitFrequency;

		public FloatRange bluntInnerHitConverted;

		public FloatRange bluntInnerHitAdded;

		public float scratchSplitPercentage = 0.5f;

		public float biteDamageMultiplier = 1f;

		public List<DamageDefAdditionalHediff> additionalHediffs;

		[Unsaved]
		private DamageWorker workerInt;

		public DamageWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (DamageWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
