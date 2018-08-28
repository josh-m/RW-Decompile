using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class PawnKindDef : Def
	{
		public ThingDef race;

		public FactionDef defaultFactionType;

		[NoTranslate]
		public List<string> backstoryCategories;

		[MustTranslate]
		public string labelPlural;

		public List<PawnKindLifeStage> lifeStages = new List<PawnKindLifeStage>();

		public float backstoryCryptosleepCommonality;

		public int minGenerationAge;

		public int maxGenerationAge = 999999;

		public bool factionLeader;

		public bool destroyGearOnDrop;

		public bool isFighter = true;

		public float combatPower = -1f;

		public bool canArriveManhunter = true;

		public bool canBeSapper;

		public float baseRecruitDifficulty = 0.5f;

		public bool aiAvoidCover;

		public FloatRange fleeHealthThresholdRange = new FloatRange(-0.4f, 0.4f);

		public QualityCategory itemQuality = QualityCategory.Normal;

		public bool forceNormalGearQuality;

		public FloatRange gearHealthRange = FloatRange.One;

		public FloatRange weaponMoney = FloatRange.Zero;

		[NoTranslate]
		public List<string> weaponTags;

		public FloatRange apparelMoney = FloatRange.Zero;

		public List<ThingDef> apparelRequired;

		[NoTranslate]
		public List<string> apparelTags;

		public float apparelAllowHeadgearChance = 1f;

		public bool apparelIgnoreSeasons;

		public Color apparelColor = Color.white;

		public FloatRange techHediffsMoney = FloatRange.Zero;

		[NoTranslate]
		public List<string> techHediffsTags;

		public float techHediffsChance;

		public List<ThingDefCountClass> fixedInventory = new List<ThingDefCountClass>();

		public PawnInventoryOption inventoryOptions;

		public float invNutrition;

		public ThingDef invFoodDef;

		public float chemicalAddictionChance;

		public float combatEnhancingDrugsChance;

		public IntRange combatEnhancingDrugsCount = IntRange.zero;

		public bool trader;

		[MustTranslate]
		public string labelMale;

		[MustTranslate]
		public string labelMalePlural;

		[MustTranslate]
		public string labelFemale;

		[MustTranslate]
		public string labelFemalePlural;

		public IntRange wildGroupSize = IntRange.one;

		public float ecoSystemWeight = 1f;

		public RaceProperties RaceProps
		{
			get
			{
				return this.race.race;
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			for (int i = 0; i < this.lifeStages.Count; i++)
			{
				this.lifeStages[i].ResolveReferences();
			}
		}

		public string GetLabelPlural(int count = -1)
		{
			if (!this.labelPlural.NullOrEmpty())
			{
				return this.labelPlural;
			}
			return Find.ActiveLanguageWorker.Pluralize(this.label, count);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string err in base.ConfigErrors())
			{
				yield return err;
			}
			if (this.race == null)
			{
				yield return "no race";
			}
			else if (this.RaceProps.Humanlike && this.backstoryCategories.NullOrEmpty<string>())
			{
				yield return "Humanlike needs backstoryCategories.";
			}
			if (this.baseRecruitDifficulty > 1.0001f)
			{
				yield return this.defName + " recruitDifficulty is greater than 1. 1 means impossible to recruit.";
			}
			if (this.combatPower < 0f)
			{
				yield return this.defName + " has no combatPower.";
			}
			if (this.weaponMoney != FloatRange.Zero)
			{
				float minCost = 999999f;
				int i;
				for (i = 0; i < this.weaponTags.Count; i++)
				{
					IEnumerable<ThingDef> source = from d in DefDatabase<ThingDef>.AllDefs
					where d.weaponTags != null && d.weaponTags.Contains(this.$this.weaponTags[i])
					select d;
					if (source.Any<ThingDef>())
					{
						minCost = Mathf.Min(minCost, source.Min(new Func<ThingDef, float>(PawnWeaponGenerator.CheapestNonDerpPriceFor)));
					}
				}
				if (minCost > this.weaponMoney.min)
				{
					yield return string.Concat(new object[]
					{
						"Cheapest weapon with one of my weaponTags costs ",
						minCost,
						" but weaponMoney min is ",
						this.weaponMoney.min,
						", so could end up weaponless."
					});
				}
			}
			if (!this.RaceProps.Humanlike && this.lifeStages.Count != this.RaceProps.lifeStageAges.Count)
			{
				yield return string.Concat(new object[]
				{
					"PawnKindDef defines ",
					this.lifeStages.Count,
					" lifeStages while race def defines ",
					this.RaceProps.lifeStageAges.Count
				});
			}
			if (this.apparelRequired != null)
			{
				for (int k = 0; k < this.apparelRequired.Count; k++)
				{
					for (int j = k + 1; j < this.apparelRequired.Count; j++)
					{
						if (!ApparelUtility.CanWearTogether(this.apparelRequired[k], this.apparelRequired[j], this.race.race.body))
						{
							yield return string.Concat(new object[]
							{
								"required apparel can't be worn together (",
								this.apparelRequired[k],
								", ",
								this.apparelRequired[j],
								")"
							});
						}
					}
				}
			}
		}

		public static PawnKindDef Named(string defName)
		{
			return DefDatabase<PawnKindDef>.GetNamed(defName, true);
		}
	}
}
