using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FoodRestrictionDatabase : IExposable
	{
		private List<FoodRestriction> foodRestrictions = new List<FoodRestriction>();

		public List<FoodRestriction> AllFoodRestrictions
		{
			get
			{
				return this.foodRestrictions;
			}
		}

		public FoodRestrictionDatabase()
		{
			this.GenerateStartingFoodRestrictions();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<FoodRestriction>(ref this.foodRestrictions, "foodRestrictions", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.FoodRestrictionDatabasePostLoadInit(this);
			}
		}

		public FoodRestriction DefaultFoodRestriction()
		{
			if (this.foodRestrictions.Count == 0)
			{
				this.MakeNewFoodRestriction();
			}
			return this.foodRestrictions[0];
		}

		public AcceptanceReport TryDelete(FoodRestriction foodRestriction)
		{
			foreach (Pawn current in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			{
				if (current.foodRestriction != null && current.foodRestriction.CurrentFoodRestriction == foodRestriction)
				{
					return new AcceptanceReport("FoodRestrictionInUse".Translate(current));
				}
			}
			foreach (Pawn current2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				if (current2.foodRestriction != null && current2.foodRestriction.CurrentFoodRestriction == foodRestriction)
				{
					current2.foodRestriction.CurrentFoodRestriction = null;
				}
			}
			this.foodRestrictions.Remove(foodRestriction);
			return AcceptanceReport.WasAccepted;
		}

		public FoodRestriction MakeNewFoodRestriction()
		{
			int arg_40_0;
			if (this.foodRestrictions.Any<FoodRestriction>())
			{
				arg_40_0 = this.foodRestrictions.Max((FoodRestriction o) => o.id) + 1;
			}
			else
			{
				arg_40_0 = 1;
			}
			int id = arg_40_0;
			FoodRestriction foodRestriction = new FoodRestriction(id, "FoodRestriction".Translate() + " " + id.ToString());
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.Foods, true, null, null);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, true, null, null);
			foodRestriction.filter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, true, null, null);
			this.foodRestrictions.Add(foodRestriction);
			return foodRestriction;
		}

		private void GenerateStartingFoodRestrictions()
		{
			FoodRestriction foodRestriction = this.MakeNewFoodRestriction();
			foodRestriction.label = "FoodRestrictionLavish".Translate();
			FoodRestriction foodRestriction2 = this.MakeNewFoodRestriction();
			foodRestriction2.label = "FoodRestrictionFine".Translate();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.ingestible != null && current.ingestible.preferability >= FoodPreferability.MealLavish && current != ThingDefOf.InsectJelly)
				{
					foodRestriction2.filter.SetAllow(current, false);
				}
			}
			FoodRestriction foodRestriction3 = this.MakeNewFoodRestriction();
			foodRestriction3.label = "FoodRestrictionSimple".Translate();
			foreach (ThingDef current2 in DefDatabase<ThingDef>.AllDefs)
			{
				if (current2.ingestible != null && current2.ingestible.preferability >= FoodPreferability.MealFine && current2 != ThingDefOf.InsectJelly)
				{
					foodRestriction3.filter.SetAllow(current2, false);
				}
			}
			foodRestriction3.filter.SetAllow(ThingDefOf.MealSurvivalPack, false);
			FoodRestriction foodRestriction4 = this.MakeNewFoodRestriction();
			foodRestriction4.label = "FoodRestrictionPaste".Translate();
			foreach (ThingDef current3 in DefDatabase<ThingDef>.AllDefs)
			{
				if (current3.ingestible != null && current3.ingestible.preferability >= FoodPreferability.MealSimple && current3 != ThingDefOf.MealNutrientPaste && current3 != ThingDefOf.InsectJelly && current3 != ThingDefOf.Pemmican)
				{
					foodRestriction4.filter.SetAllow(current3, false);
				}
			}
			FoodRestriction foodRestriction5 = this.MakeNewFoodRestriction();
			foodRestriction5.label = "FoodRestrictionRaw".Translate();
			foreach (ThingDef current4 in DefDatabase<ThingDef>.AllDefs)
			{
				if (current4.ingestible != null && current4.ingestible.preferability >= FoodPreferability.MealAwful)
				{
					foodRestriction5.filter.SetAllow(current4, false);
				}
			}
			foodRestriction5.filter.SetAllow(ThingDefOf.Chocolate, false);
			FoodRestriction foodRestriction6 = this.MakeNewFoodRestriction();
			foodRestriction6.label = "FoodRestrictionNothing".Translate();
			foodRestriction6.filter.SetDisallowAll(null, null);
		}
	}
}
