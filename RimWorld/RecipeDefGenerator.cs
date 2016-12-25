using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	internal static class RecipeDefGenerator
	{
		[DebuggerHidden]
		public static IEnumerable<RecipeDef> ImpliedRecipeDefs()
		{
			foreach (RecipeDef r in RecipeDefGenerator.DefsFromRecipeMakers().Concat(RecipeDefGenerator.DrugAdministerDefs()))
			{
				yield return r;
			}
		}

		[DebuggerHidden]
		private static IEnumerable<RecipeDef> DefsFromRecipeMakers()
		{
			foreach (ThingDef def in from d in DefDatabase<ThingDef>.AllDefs
			where d.recipeMaker != null
			select d)
			{
				RecipeMakerProperties rm = def.recipeMaker;
				RecipeDef r = new RecipeDef();
				r.defName = "Make_" + def.defName;
				r.label = "RecipeMake".Translate(new object[]
				{
					def.label
				});
				r.jobString = "RecipeMakeJobString".Translate(new object[]
				{
					def.label
				});
				r.workAmount = (float)rm.workAmount;
				r.workSpeedStat = rm.workSpeedStat;
				r.efficiencyStat = rm.efficiencyStat;
				if (def.MadeFromStuff)
				{
					IngredientCount ic = new IngredientCount();
					ic.SetBaseCount((float)def.costStuffCount);
					ic.filter.SetAllowAllWhoCanMake(def);
					r.ingredients.Add(ic);
					r.fixedIngredientFilter.SetAllowAllWhoCanMake(def);
					r.productHasIngredientStuff = true;
				}
				if (def.costList != null)
				{
					foreach (ThingCountClass c in def.costList)
					{
						IngredientCount ic2 = new IngredientCount();
						ic2.SetBaseCount((float)c.count);
						ic2.filter.SetAllow(c.thingDef, true);
						r.ingredients.Add(ic2);
						r.fixedIngredientFilter.SetAllow(c.thingDef, true);
					}
				}
				r.defaultIngredientFilter = rm.defaultIngredientFilter;
				r.products.Add(new ThingCountClass(def, rm.productCount));
				r.skillRequirements = rm.skillRequirements.ListFullCopyOrNull<SkillRequirement>();
				r.workSkill = rm.workSkill;
				r.workSkillLearnFactor = rm.workSkillLearnPerTick;
				r.unfinishedThingDef = rm.unfinishedThingDef;
				r.recipeUsers = rm.recipeUsers.ListFullCopyOrNull<ThingDef>();
				r.effectWorking = rm.effectWorking;
				r.soundWorking = rm.soundWorking;
				r.researchPrerequisite = rm.researchPrerequisite;
				yield return r;
			}
		}

		[DebuggerHidden]
		private static IEnumerable<RecipeDef> DrugAdministerDefs()
		{
			foreach (ThingDef def in from d in DefDatabase<ThingDef>.AllDefs
			where d.IsDrug
			select d)
			{
				RecipeDef r = new RecipeDef();
				r.defName = "Administer_" + def.defName;
				r.label = "RecipeAdminister".Translate(new object[]
				{
					def.label
				});
				r.jobString = "RecipeAdministerJobString".Translate(new object[]
				{
					def.label
				});
				r.workerClass = typeof(Recipe_AdministerIngestible);
				r.targetsBodyPart = false;
				r.anesthesize = false;
				r.workAmount = (float)def.ingestible.baseIngestTicks;
				IngredientCount ic = new IngredientCount();
				ic.SetBaseCount(1f);
				ic.filter.SetAllow(def, true);
				r.ingredients.Add(ic);
				r.fixedIngredientFilter.SetAllow(def, true);
				r.recipeUsers = new List<ThingDef>();
				foreach (ThingDef fleshRace in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Pawn && d.race.IsFlesh))
				{
					r.recipeUsers.Add(fleshRace);
				}
				yield return r;
			}
		}
	}
}
