using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse
{
	public class RaceProperties
	{
		public Intelligence intelligence;

		public FleshType fleshType;

		private ThingDef bloodDef;

		public bool hasGenders = true;

		public bool needsRest = true;

		public ThinkTreeDef thinkTreeMain;

		public ThinkTreeDef thinkTreeConstant;

		public PawnNameCategory nameCategory;

		public FoodTypeFlags foodType;

		public BodyDef body;

		public Type deathActionWorkerClass;

		public List<AnimalBiomeRecord> wildBiomes;

		public SimpleCurve ageGenerationCurve;

		public bool makesFootprints;

		public int executionRange = 2;

		public float lifeExpectancy = 10f;

		public List<HediffGiverSetDef> hediffGiverSets;

		public float petness;

		public bool packAnimal;

		public bool herdAnimal;

		public bool predator;

		public float maxPreyBodySize = 99999f;

		public float wildness = -1f;

		public float nuzzleMtbHours = -1f;

		public float manhunterOnDamageChance;

		public float manhunterOnTameFailChance;

		public bool canBePredatorPrey = true;

		public float gestationPeriodDays = 10f;

		public SimpleCurve litterSizeCurve;

		public float mateMtbHours = 0.1f;

		public List<string> untrainableTags;

		public List<string> trainableTags;

		public TrainableIntelligence trainableIntelligence = TrainableIntelligence.Intermediate;

		private RulePackDef nameGenerator;

		private RulePackDef nameGeneratorFemale;

		public float nameOnTameChance;

		public float nameOnNuzzleChance;

		public float baseBodySize = 1f;

		public float baseHealthScale = 1f;

		public float baseHungerRate = 1f;

		public List<LifeStageAge> lifeStageAges = new List<LifeStageAge>();

		public Color leatherColor = ColorLibrary.Leather;

		public string leatherLabel;

		public float leatherCommonalityFactor = 1f;

		public float leatherInsulation = 1.1f;

		public List<StatModifier> leatherStatFactors;

		public float leatherMarketValueFactor = 1f;

		public string meatLabel;

		public Color meatColor;

		public ThingDef useMeatFrom;

		public ThingDef useLeatherFrom;

		public ShadowData specialShadowData;

		public IntRange soundCallIntervalRange;

		public SoundDef soundMeleeHitPawn;

		public SoundDef soundMeleeHitBuilding;

		public SoundDef soundMeleeMiss;

		[Unsaved]
		private DeathActionWorker deathActionWorkerInt;

		[Unsaved]
		public ThingDef meatDef;

		[Unsaved]
		public ThingDef leatherDef;

		[Unsaved]
		public ThingDef corpseDef;

		public bool Humanlike
		{
			get
			{
				return this.intelligence >= Intelligence.Humanlike;
			}
		}

		public bool ToolUser
		{
			get
			{
				return this.intelligence >= Intelligence.ToolUser;
			}
		}

		public bool Animal
		{
			get
			{
				return !this.ToolUser && this.IsFlesh;
			}
		}

		public bool EatsFood
		{
			get
			{
				return this.foodType != FoodTypeFlags.None;
			}
		}

		public float FoodLevelPercentageWantEat
		{
			get
			{
				switch (this.ResolvedDietCategory)
				{
				case DietCategory.NeverEats:
					return 0.5f;
				case DietCategory.Herbivorous:
					return 0.65f;
				case DietCategory.Dendrovorous:
					return 0.65f;
				case DietCategory.Ovivorous:
					return 0.5f;
				case DietCategory.Omnivorous:
					return 0.3f;
				case DietCategory.Carnivorous:
					return 0.3f;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public DietCategory ResolvedDietCategory
		{
			get
			{
				if (!this.EatsFood)
				{
					return DietCategory.NeverEats;
				}
				if (this.Eats(FoodTypeFlags.Tree))
				{
					return DietCategory.Dendrovorous;
				}
				if (this.Eats(FoodTypeFlags.Meat))
				{
					if (this.Eats(FoodTypeFlags.VegetableOrFruit) || this.Eats(FoodTypeFlags.Plant))
					{
						return DietCategory.Omnivorous;
					}
					return DietCategory.Carnivorous;
				}
				else
				{
					if (this.Eats(FoodTypeFlags.AnimalProduct))
					{
						return DietCategory.Ovivorous;
					}
					return DietCategory.Herbivorous;
				}
			}
		}

		public DeathActionWorker DeathActionWorker
		{
			get
			{
				if (this.deathActionWorkerInt == null && this.deathActionWorkerClass != null)
				{
					this.deathActionWorkerInt = (DeathActionWorker)Activator.CreateInstance(this.deathActionWorkerClass);
				}
				return this.deathActionWorkerInt;
			}
		}

		public bool IsMechanoid
		{
			get
			{
				return this.fleshType == FleshType.Mechanoid;
			}
		}

		public bool IsFlesh
		{
			get
			{
				return this.fleshType != FleshType.Mechanoid;
			}
		}

		public ThingDef BloodDef
		{
			get
			{
				if (this.bloodDef != null)
				{
					return this.bloodDef;
				}
				if (this.IsFlesh)
				{
					return ThingDefOf.FilthBlood;
				}
				return null;
			}
		}

		public RaceProperties()
		{
			ColorInt colorInt = new ColorInt(141, 56, 52);
			this.meatColor = colorInt.ToColor;
			this.soundCallIntervalRange = new IntRange(2000, 4000);
			base..ctor();
		}

		public RulePackDef GetNameGenerator(Gender gender)
		{
			if (gender == Gender.Female && this.nameGeneratorFemale != null)
			{
				return this.nameGeneratorFemale;
			}
			return this.nameGenerator;
		}

		public bool WillAutomaticallyEat(Thing t)
		{
			return t.def.ingestible != null && this.CanEverEat(t);
		}

		public bool CanEverEat(Thing t)
		{
			return this.CanEverEat(t.def);
		}

		public bool CanEverEat(ThingDef t)
		{
			return this.EatsFood && t.ingestible != null && t.ingestible.preferability != FoodPreferability.Undefined && this.Eats(t.ingestible.foodType);
		}

		public bool Eats(FoodTypeFlags food)
		{
			return this.EatsFood && (this.foodType & food) != FoodTypeFlags.None;
		}

		public void ResolveReferencesSpecial()
		{
			if (this.useMeatFrom != null)
			{
				this.meatDef = this.useMeatFrom.race.meatDef;
			}
			if (this.useLeatherFrom != null)
			{
				this.leatherDef = this.useLeatherFrom.race.leatherDef;
			}
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors()
		{
			if (this.soundMeleeHitPawn == null)
			{
				yield return "soundMeleeHitPawn is null";
			}
			if (this.soundMeleeHitBuilding == null)
			{
				yield return "soundMeleeHitBuilding is null";
			}
			if (this.soundMeleeMiss == null)
			{
				yield return "soundMeleeMiss is null";
			}
			if (this.predator && !this.Eats(FoodTypeFlags.Meat))
			{
				yield return "predator but doesn't eat meat";
			}
			for (int i = 0; i < this.lifeStageAges.Count; i++)
			{
				for (int j = 0; j < i; j++)
				{
					if (this.lifeStageAges[j].minAge > this.lifeStageAges[i].minAge)
					{
						yield return "lifeStages minAges are not in ascending order";
					}
				}
			}
			if (this.litterSizeCurve != null)
			{
				foreach (string e in this.litterSizeCurve.ConfigErrors("litterSizeCurve"))
				{
					yield return e;
				}
			}
			if (this.nameOnTameChance > 0f && this.nameGenerator == null)
			{
				yield return "can be named, but has no nameGenerator";
			}
			if (this.Animal && this.wildness < 0f)
			{
				yield return "is animal but wildness is not defined";
			}
			if (this.useMeatFrom != null && this.useMeatFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use meat from non-pawn " + this.useMeatFrom;
			}
			if (this.useMeatFrom != null && this.useMeatFrom.race.useMeatFrom != null)
			{
				yield return string.Concat(new object[]
				{
					"tries to use meat from ",
					this.useMeatFrom,
					" which uses meat from ",
					this.useMeatFrom.race.useMeatFrom
				});
			}
			if (this.useLeatherFrom != null && this.useLeatherFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use leather from non-pawn " + this.useLeatherFrom;
			}
			if (this.useLeatherFrom != null && this.useLeatherFrom.race.useLeatherFrom != null)
			{
				yield return string.Concat(new object[]
				{
					"tries to use leather from ",
					this.useLeatherFrom,
					" which uses leather from ",
					this.useLeatherFrom.race.useLeatherFrom
				});
			}
		}

		[DebuggerHidden]
		internal IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Race".Translate(), parentDef.LabelCap, 2000);
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Diet".Translate(), this.foodType.ToHumanString().CapitalizeFirst(), 0);
			if (this.wildness >= 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Wildness".Translate(), this.wildness.ToStringPercent(), 0)
				{
					overrideReportText = "WildnessExplanation".Translate()
				};
			}
			if (this.intelligence < Intelligence.Humanlike)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "TrainableIntelligence".Translate(), this.trainableIntelligence.GetLabel().CapitalizeFirst(), 0);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "StatsReport_LifeExpectancy".Translate(), this.lifeExpectancy.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Absolute), 0);
			if (this.intelligence < Intelligence.Humanlike)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "AnimalFilthRate".Translate(), (PawnUtility.AnimalFilthChancePerCell(parentDef, parentDef.race.baseBodySize) * 1000f).ToString("F2"), 0)
				{
					overrideReportText = "AnimalFilthRateExplanation".Translate(new object[]
					{
						1000.ToString()
					})
				};
			}
			if (this.packAnimal)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "PackAnimal".Translate(), "Yes".Translate(), 0)
				{
					overrideReportText = "PackAnimalExplanation".Translate()
				};
			}
		}
	}
}
