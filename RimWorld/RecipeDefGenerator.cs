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
			foreach (ThingDef t in from d in DefDatabase<ThingDef>.AllDefs
			where d.recipeMaker != null
			select d)
			{
				RecipeMakerProperties rm = t.recipeMaker;
				RecipeDef r = new RecipeDef();
				r.defName = "Make_" + t.defName;
				r.label = "RecipeMake".Translate(new object[]
				{
					t.label
				});
				r.jobString = "RecipeMakeJobString".Translate(new object[]
				{
					t.label
				});
				r.workAmount = (float)rm.workAmount;
				r.workSpeedStat = rm.workSpeedStat;
				r.efficiencyStat = rm.efficiencyStat;
				if (t.MadeFromStuff)
				{
					IngredientCount ic = new IngredientCount();
					ic.SetBaseCount((float)t.costStuffCount);
					ic.filter.SetAllowAllWhoCanMake(t);
					r.ingredients.Add(ic);
					r.fixedIngredientFilter.SetAllowAllWhoCanMake(t);
					r.productHasIngredientStuff = true;
				}
				if (t.costList != null)
				{
					foreach (ThingCount c in t.costList)
					{
						IngredientCount ic2 = new IngredientCount();
						ic2.SetBaseCount((float)c.count);
						ic2.filter.SetAllow(c.thingDef, true);
						r.ingredients.Add(ic2);
						r.fixedIngredientFilter.SetAllow(c.thingDef, true);
					}
				}
				r.defaultIngredientFilter = rm.defaultIngredientFilter;
				r.products.Add(new ThingCount(t, rm.productCount));
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
	}
}
