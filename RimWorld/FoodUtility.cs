using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FoodUtility
	{
		private static HashSet<Thing> filtered = new HashSet<Thing>();

		private static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
		{
			{
				new CurvePoint(-100f, -600f),
				true
			},
			{
				new CurvePoint(-10f, -100f),
				true
			},
			{
				new CurvePoint(-5f, -70f),
				true
			},
			{
				new CurvePoint(-1f, -50f),
				true
			},
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(100f, 800f),
				true
			}
		};

		private static List<Pawn> tmpPredatorCandidates = new List<Pawn>();

		private static List<ThoughtDef> ingestThoughts = new List<ThoughtDef>();

		public static bool WillEat(this Pawn p, Thing food, Pawn getter = null)
		{
			if (!p.RaceProps.CanEverEat(food))
			{
				return false;
			}
			if (p.foodRestriction != null)
			{
				FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
				if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && (food.def.IsWithinCategory(ThingCategoryDefOf.Foods) || food.def.IsWithinCategory(ThingCategoryDefOf.Corpses)))
				{
					return false;
				}
			}
			return true;
		}

		public static bool WillEat(this Pawn p, ThingDef food, Pawn getter = null)
		{
			if (!p.RaceProps.CanEverEat(food))
			{
				return false;
			}
			if (p.foodRestriction != null)
			{
				FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
				if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && food.IsWithinCategory(currentRespectedRestriction.filter.DisplayRootCategory.catDef))
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryFindBestFoodSourceFor(Pawn getter, Pawn eater, bool desperate, out Thing foodSource, out ThingDef foodDef, bool canRefillDispenser = true, bool canUseInventory = true, bool allowForbidden = false, bool allowCorpse = true, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false)
		{
			bool flag = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
			bool flag2 = !eater.IsTeetotaler();
			Thing thing = null;
			if (canUseInventory)
			{
				if (flag)
				{
					thing = FoodUtility.BestFoodInInventory(getter, eater, FoodPreferability.MealAwful, FoodPreferability.MealLavish, 0f, false);
				}
				if (thing != null)
				{
					if (getter.Faction != Faction.OfPlayer)
					{
						foodSource = thing;
						foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
						return true;
					}
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
					{
						foodSource = thing;
						foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
						return true;
					}
				}
			}
			bool allowPlant = getter == eater;
			ThingDef thingDef;
			Thing thing2 = FoodUtility.BestFoodSourceOnMap(getter, eater, desperate, out thingDef, FoodPreferability.MealLavish, allowPlant, flag2, allowCorpse, true, canRefillDispenser, allowForbidden, allowSociallyImproper, allowHarvest, forceScanWholeMap);
			if (thing == null && thing2 == null)
			{
				if (canUseInventory && flag)
				{
					FoodPreferability minFoodPref = FoodPreferability.DesperateOnly;
					bool allowDrug = flag2;
					thing = FoodUtility.BestFoodInInventory(getter, eater, minFoodPref, FoodPreferability.MealLavish, 0f, allowDrug);
					if (thing != null)
					{
						foodSource = thing;
						foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
						return true;
					}
				}
				if (thing2 == null && getter == eater && (getter.RaceProps.predator || (getter.IsWildMan() && !getter.IsPrisoner)))
				{
					Pawn pawn = FoodUtility.BestPawnToHuntForPredator(getter, forceScanWholeMap);
					if (pawn != null)
					{
						foodSource = pawn;
						foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
						return true;
					}
				}
				foodSource = null;
				foodDef = null;
				return false;
			}
			if (thing == null && thing2 != null)
			{
				foodSource = thing2;
				foodDef = thingDef;
				return true;
			}
			ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(thing, false);
			if (thing2 == null)
			{
				foodSource = thing;
				foodDef = finalIngestibleDef;
				return true;
			}
			float num = FoodUtility.FoodOptimality(eater, thing2, thingDef, (float)(getter.Position - thing2.Position).LengthManhattan, false);
			float num2 = FoodUtility.FoodOptimality(eater, thing, finalIngestibleDef, 0f, false);
			num2 -= 32f;
			if (num > num2)
			{
				foodSource = thing2;
				foodDef = thingDef;
				return true;
			}
			foodSource = thing;
			foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, false);
			return true;
		}

		public static ThingDef GetFinalIngestibleDef(Thing foodSource, bool harvest = false)
		{
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
			if (building_NutrientPasteDispenser != null)
			{
				return building_NutrientPasteDispenser.DispensableDef;
			}
			Pawn pawn = foodSource as Pawn;
			if (pawn != null)
			{
				return pawn.RaceProps.corpseDef;
			}
			if (harvest)
			{
				Plant plant = foodSource as Plant;
				if (plant != null && plant.HarvestableNow && plant.def.plant.harvestedThingDef.IsIngestible)
				{
					return plant.def.plant.harvestedThingDef;
				}
			}
			return foodSource.def;
		}

		public static Thing BestFoodInInventory(Pawn holder, Pawn eater = null, FoodPreferability minFoodPref = FoodPreferability.NeverForNutrition, FoodPreferability maxFoodPref = FoodPreferability.MealLavish, float minStackNutrition = 0f, bool allowDrug = false)
		{
			if (holder.inventory == null)
			{
				return null;
			}
			if (eater == null)
			{
				eater = holder;
			}
			ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Thing thing = innerContainer[i];
				if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, holder) && thing.def.ingestible.preferability >= minFoodPref && thing.def.ingestible.preferability <= maxFoodPref && (allowDrug || !thing.def.IsDrug))
				{
					float num = thing.GetStatValue(StatDefOf.Nutrition, true) * (float)thing.stackCount;
					if (num >= minStackNutrition)
					{
						return thing;
					}
				}
			}
			return null;
		}

		public static Thing BestFoodSourceOnMap(Pawn getter, Pawn eater, bool desperate, out ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false)
		{
			FoodUtility.<BestFoodSourceOnMap>c__AnonStorey0 <BestFoodSourceOnMap>c__AnonStorey = new FoodUtility.<BestFoodSourceOnMap>c__AnonStorey0();
			<BestFoodSourceOnMap>c__AnonStorey.allowDispenserFull = allowDispenserFull;
			<BestFoodSourceOnMap>c__AnonStorey.maxPref = maxPref;
			<BestFoodSourceOnMap>c__AnonStorey.eater = eater;
			<BestFoodSourceOnMap>c__AnonStorey.getter = getter;
			<BestFoodSourceOnMap>c__AnonStorey.allowForbidden = allowForbidden;
			<BestFoodSourceOnMap>c__AnonStorey.allowDispenserEmpty = allowDispenserEmpty;
			<BestFoodSourceOnMap>c__AnonStorey.allowSociallyImproper = allowSociallyImproper;
			<BestFoodSourceOnMap>c__AnonStorey.allowCorpse = allowCorpse;
			<BestFoodSourceOnMap>c__AnonStorey.allowDrug = allowDrug;
			<BestFoodSourceOnMap>c__AnonStorey.desperate = desperate;
			<BestFoodSourceOnMap>c__AnonStorey.forceScanWholeMap = forceScanWholeMap;
			foodDef = null;
			<BestFoodSourceOnMap>c__AnonStorey.getterCanManipulate = (<BestFoodSourceOnMap>c__AnonStorey.getter.RaceProps.ToolUser && <BestFoodSourceOnMap>c__AnonStorey.getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
			if (!<BestFoodSourceOnMap>c__AnonStorey.getterCanManipulate && <BestFoodSourceOnMap>c__AnonStorey.getter != <BestFoodSourceOnMap>c__AnonStorey.eater)
			{
				Log.Error(string.Concat(new object[]
				{
					<BestFoodSourceOnMap>c__AnonStorey.getter,
					" tried to find food to bring to ",
					<BestFoodSourceOnMap>c__AnonStorey.eater,
					" but ",
					<BestFoodSourceOnMap>c__AnonStorey.getter,
					" is incapable of Manipulation."
				}), false);
				return null;
			}
			if (<BestFoodSourceOnMap>c__AnonStorey.eater.NonHumanlikeOrWildMan())
			{
				<BestFoodSourceOnMap>c__AnonStorey.minPref = FoodPreferability.NeverForNutrition;
			}
			else if (<BestFoodSourceOnMap>c__AnonStorey.desperate)
			{
				<BestFoodSourceOnMap>c__AnonStorey.minPref = FoodPreferability.DesperateOnly;
			}
			else
			{
				<BestFoodSourceOnMap>c__AnonStorey.minPref = ((<BestFoodSourceOnMap>c__AnonStorey.eater.needs.food.CurCategory < HungerCategory.UrgentlyHungry) ? FoodPreferability.MealAwful : FoodPreferability.RawBad);
			}
			<BestFoodSourceOnMap>c__AnonStorey.foodValidator = delegate(Thing t)
			{
				Building_NutrientPasteDispenser building_NutrientPasteDispenser = t as Building_NutrientPasteDispenser;
				if (building_NutrientPasteDispenser != null)
				{
					if (!<BestFoodSourceOnMap>c__AnonStorey.allowDispenserFull || !<BestFoodSourceOnMap>c__AnonStorey.getterCanManipulate || ThingDefOf.MealNutrientPaste.ingestible.preferability < <BestFoodSourceOnMap>c__AnonStorey.minPref || ThingDefOf.MealNutrientPaste.ingestible.preferability > <BestFoodSourceOnMap>c__AnonStorey.maxPref || !<BestFoodSourceOnMap>c__AnonStorey.eater.WillEat(ThingDefOf.MealNutrientPaste, <BestFoodSourceOnMap>c__AnonStorey.getter) || (t.Faction != <BestFoodSourceOnMap>c__AnonStorey.getter.Faction && t.Faction != <BestFoodSourceOnMap>c__AnonStorey.getter.HostFaction) || (!<BestFoodSourceOnMap>c__AnonStorey.allowForbidden && t.IsForbidden(<BestFoodSourceOnMap>c__AnonStorey.getter)) || (!building_NutrientPasteDispenser.powerComp.PowerOn || (!<BestFoodSourceOnMap>c__AnonStorey.allowDispenserEmpty && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())) || !t.InteractionCell.Standable(t.Map) || !FoodUtility.IsFoodSourceOnMapSociallyProper(t, <BestFoodSourceOnMap>c__AnonStorey.getter, <BestFoodSourceOnMap>c__AnonStorey.eater, <BestFoodSourceOnMap>c__AnonStorey.allowSociallyImproper) || <BestFoodSourceOnMap>c__AnonStorey.getter.IsWildMan() || !<BestFoodSourceOnMap>c__AnonStorey.getter.Map.reachability.CanReachNonLocal(<BestFoodSourceOnMap>c__AnonStorey.getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(<BestFoodSourceOnMap>c__AnonStorey.getter, Danger.Some, TraverseMode.ByPawn, false)))
					{
						return false;
					}
				}
				else if (t.def.ingestible.preferability < <BestFoodSourceOnMap>c__AnonStorey.minPref || t.def.ingestible.preferability > <BestFoodSourceOnMap>c__AnonStorey.maxPref || !<BestFoodSourceOnMap>c__AnonStorey.eater.WillEat(t, <BestFoodSourceOnMap>c__AnonStorey.getter) || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow || (!<BestFoodSourceOnMap>c__AnonStorey.allowCorpse && t is Corpse) || (!<BestFoodSourceOnMap>c__AnonStorey.allowDrug && t.def.IsDrug) || (!<BestFoodSourceOnMap>c__AnonStorey.allowForbidden && t.IsForbidden(<BestFoodSourceOnMap>c__AnonStorey.getter)) || (!<BestFoodSourceOnMap>c__AnonStorey.desperate && t.IsNotFresh()) || (t.IsDessicated() || !FoodUtility.IsFoodSourceOnMapSociallyProper(t, <BestFoodSourceOnMap>c__AnonStorey.getter, <BestFoodSourceOnMap>c__AnonStorey.eater, <BestFoodSourceOnMap>c__AnonStorey.allowSociallyImproper) || (!<BestFoodSourceOnMap>c__AnonStorey.getter.AnimalAwareOf(t) && !<BestFoodSourceOnMap>c__AnonStorey.forceScanWholeMap)) || !<BestFoodSourceOnMap>c__AnonStorey.getter.CanReserve(t, 1, -1, null, false))
				{
					return false;
				}
				return true;
			};
			ThingRequest thingRequest;
			if ((<BestFoodSourceOnMap>c__AnonStorey.eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) != FoodTypeFlags.None && allowPlant)
			{
				thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
			}
			else
			{
				thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
			}
			if (<BestFoodSourceOnMap>c__AnonStorey.getter.RaceProps.Humanlike)
			{
				FoodUtility.<BestFoodSourceOnMap>c__AnonStorey0 arg_20A_0 = <BestFoodSourceOnMap>c__AnonStorey;
				Pawn eater2 = <BestFoodSourceOnMap>c__AnonStorey.eater;
				IntVec3 position = <BestFoodSourceOnMap>c__AnonStorey.getter.Position;
				List<Thing> searchSet = <BestFoodSourceOnMap>c__AnonStorey.getter.Map.listerThings.ThingsMatching(thingRequest);
				PathEndMode peMode = PathEndMode.ClosestTouch;
				TraverseParms traverseParams = TraverseParms.For(<BestFoodSourceOnMap>c__AnonStorey.getter, Danger.Deadly, TraverseMode.ByPawn, false);
				Predicate<Thing> validator = <BestFoodSourceOnMap>c__AnonStorey.foodValidator;
				arg_20A_0.bestThing = FoodUtility.SpawnedFoodSearchInnerScan(eater2, position, searchSet, peMode, traverseParams, 9999f, validator);
				if (allowHarvest && <BestFoodSourceOnMap>c__AnonStorey.getterCanManipulate)
				{
					int searchRegionsMax;
					if (<BestFoodSourceOnMap>c__AnonStorey.forceScanWholeMap && <BestFoodSourceOnMap>c__AnonStorey.bestThing == null)
					{
						searchRegionsMax = -1;
					}
					else
					{
						searchRegionsMax = 30;
					}
					Thing thing = GenClosest.ClosestThingReachable(<BestFoodSourceOnMap>c__AnonStorey.getter.Position, <BestFoodSourceOnMap>c__AnonStorey.getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), PathEndMode.Touch, TraverseParms.For(<BestFoodSourceOnMap>c__AnonStorey.getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate(Thing x)
					{
						Plant plant = (Plant)x;
						if (!plant.HarvestableNow)
						{
							return false;
						}
						ThingDef harvestedThingDef = plant.def.plant.harvestedThingDef;
						return harvestedThingDef.IsNutritionGivingIngestible && <BestFoodSourceOnMap>c__AnonStorey.eater.WillEat(harvestedThingDef, <BestFoodSourceOnMap>c__AnonStorey.getter) && <BestFoodSourceOnMap>c__AnonStorey.getter.CanReserve(plant, 1, -1, null, false) && (<BestFoodSourceOnMap>c__AnonStorey.allowForbidden || !plant.IsForbidden(<BestFoodSourceOnMap>c__AnonStorey.getter)) && (<BestFoodSourceOnMap>c__AnonStorey.bestThing == null || FoodUtility.GetFinalIngestibleDef(<BestFoodSourceOnMap>c__AnonStorey.bestThing, false).ingestible.preferability < harvestedThingDef.ingestible.preferability);
					}, null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
					if (thing != null)
					{
						<BestFoodSourceOnMap>c__AnonStorey.bestThing = thing;
						foodDef = FoodUtility.GetFinalIngestibleDef(thing, true);
					}
				}
				if (foodDef == null && <BestFoodSourceOnMap>c__AnonStorey.bestThing != null)
				{
					foodDef = FoodUtility.GetFinalIngestibleDef(<BestFoodSourceOnMap>c__AnonStorey.bestThing, false);
				}
			}
			else
			{
				int maxRegionsToScan = FoodUtility.GetMaxRegionsToScan(<BestFoodSourceOnMap>c__AnonStorey.getter, <BestFoodSourceOnMap>c__AnonStorey.forceScanWholeMap);
				FoodUtility.filtered.Clear();
				foreach (Thing current in GenRadial.RadialDistinctThingsAround(<BestFoodSourceOnMap>c__AnonStorey.getter.Position, <BestFoodSourceOnMap>c__AnonStorey.getter.Map, 2f, true))
				{
					Pawn pawn = current as Pawn;
					if (pawn != null && pawn != <BestFoodSourceOnMap>c__AnonStorey.getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
					{
						FoodUtility.filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
					}
				}
				bool flag = !<BestFoodSourceOnMap>c__AnonStorey.allowForbidden && ForbidUtility.CaresAboutForbidden(<BestFoodSourceOnMap>c__AnonStorey.getter, true) && <BestFoodSourceOnMap>c__AnonStorey.getter.playerSettings != null && <BestFoodSourceOnMap>c__AnonStorey.getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
				Predicate<Thing> predicate = (Thing t) => <BestFoodSourceOnMap>c__AnonStorey.foodValidator(t) && !FoodUtility.filtered.Contains(t) && (t is Building_NutrientPasteDispenser || t.def.ingestible.preferability > FoodPreferability.DesperateOnly) && !t.IsNotFresh();
				FoodUtility.<BestFoodSourceOnMap>c__AnonStorey0 arg_475_0 = <BestFoodSourceOnMap>c__AnonStorey;
				IntVec3 position = <BestFoodSourceOnMap>c__AnonStorey.getter.Position;
				Map map = <BestFoodSourceOnMap>c__AnonStorey.getter.Map;
				ThingRequest thingReq = thingRequest;
				PathEndMode peMode = PathEndMode.ClosestTouch;
				TraverseParms traverseParams = TraverseParms.For(<BestFoodSourceOnMap>c__AnonStorey.getter, Danger.Deadly, TraverseMode.ByPawn, false);
				Predicate<Thing> validator = predicate;
				bool ignoreEntirelyForbiddenRegions = flag;
				arg_475_0.bestThing = GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, maxRegionsToScan, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
				FoodUtility.filtered.Clear();
				if (<BestFoodSourceOnMap>c__AnonStorey.bestThing == null)
				{
					<BestFoodSourceOnMap>c__AnonStorey.desperate = true;
					FoodUtility.<BestFoodSourceOnMap>c__AnonStorey0 arg_4EF_0 = <BestFoodSourceOnMap>c__AnonStorey;
					position = <BestFoodSourceOnMap>c__AnonStorey.getter.Position;
					map = <BestFoodSourceOnMap>c__AnonStorey.getter.Map;
					thingReq = thingRequest;
					peMode = PathEndMode.ClosestTouch;
					traverseParams = TraverseParms.For(<BestFoodSourceOnMap>c__AnonStorey.getter, Danger.Deadly, TraverseMode.ByPawn, false);
					validator = <BestFoodSourceOnMap>c__AnonStorey.foodValidator;
					ignoreEntirelyForbiddenRegions = flag;
					arg_4EF_0.bestThing = GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, maxRegionsToScan, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
				}
				if (<BestFoodSourceOnMap>c__AnonStorey.bestThing != null)
				{
					foodDef = FoodUtility.GetFinalIngestibleDef(<BestFoodSourceOnMap>c__AnonStorey.bestThing, false);
				}
			}
			return <BestFoodSourceOnMap>c__AnonStorey.bestThing;
		}

		private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
		{
			if (getter.RaceProps.Humanlike)
			{
				return -1;
			}
			if (forceScanWholeMap)
			{
				return -1;
			}
			if (getter.Faction == Faction.OfPlayer)
			{
				return 100;
			}
			return 30;
		}

		private static bool IsFoodSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
		{
			if (!allowSociallyImproper)
			{
				bool animalsCare = !getter.RaceProps.Animal;
				if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
				{
					return false;
				}
			}
			return true;
		}

		public static float FoodOptimality(Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory = false)
		{
			float num = 300f;
			num -= dist;
			FoodPreferability preferability = foodDef.ingestible.preferability;
			if (preferability != FoodPreferability.NeverForNutrition)
			{
				if (preferability != FoodPreferability.DesperateOnly)
				{
					if (preferability == FoodPreferability.DesperateOnlyForHumanlikes)
					{
						if (eater.RaceProps.Humanlike)
						{
							num -= 150f;
						}
					}
				}
				else
				{
					num -= 150f;
				}
				CompRottable compRottable = foodSource.TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					if (compRottable.Stage == RotStage.Dessicated)
					{
						return -9999999f;
					}
					if (!takingToInventory && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
					{
						num += 12f;
					}
				}
				if (eater.needs != null && eater.needs.mood != null)
				{
					List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(eater, foodSource, foodDef);
					for (int i = 0; i < list.Count; i++)
					{
						num += FoodUtility.FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect);
					}
				}
				if (foodDef.ingestible != null)
				{
					if (eater.RaceProps.Humanlike)
					{
						num += foodDef.ingestible.optimalityOffsetHumanlikes;
					}
					else if (eater.RaceProps.Animal)
					{
						num += foodDef.ingestible.optimalityOffsetFeedingAnimals;
					}
				}
				return num;
			}
			return -9999999f;
		}

		private static Thing SpawnedFoodSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null)
		{
			if (searchSet == null)
			{
				return null;
			}
			Pawn pawn = traverseParams.pawn ?? eater;
			int num = 0;
			int num2 = 0;
			Thing result = null;
			float num3 = -3.40282347E+38f;
			for (int i = 0; i < searchSet.Count; i++)
			{
				Thing thing = searchSet[i];
				num2++;
				float num4 = (float)(root - thing.Position).LengthManhattan;
				if (num4 <= maxDistance)
				{
					float num5 = FoodUtility.FoodOptimality(eater, thing, FoodUtility.GetFinalIngestibleDef(thing, false), num4, false);
					if (num5 >= num3)
					{
						if (pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams))
						{
							if (thing.Spawned)
							{
								if (validator == null || validator(thing))
								{
									result = thing;
									num3 = num5;
									num++;
								}
							}
						}
					}
				}
			}
			return result;
		}

		public static void DebugFoodSearchFromMouse_Update()
		{
			IntVec3 root = UI.MouseCell();
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null)
			{
				return;
			}
			if (pawn.Map != Find.CurrentMap)
			{
				return;
			}
			Thing thing = FoodUtility.SpawnedFoodSearchInnerScan(pawn, root, Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false), 9999f, null);
			if (thing != null)
			{
				GenDraw.DrawLineBetween(root.ToVector3Shifted(), thing.Position.ToVector3Shifted());
			}
		}

		public static void DebugFoodSearchFromMouse_OnGUI()
		{
			IntVec3 a = UI.MouseCell();
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null)
			{
				return;
			}
			if (pawn.Map != Find.CurrentMap)
			{
				return;
			}
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			foreach (Thing current in Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree))
			{
				ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(current, false);
				float num = FoodUtility.FoodOptimality(pawn, current, finalIngestibleDef, (a - current.Position).LengthHorizontal, false);
				Vector2 vector = current.DrawPos.MapToUIPosition();
				Rect rect = new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f);
				string text = num.ToString("F0");
				List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(pawn, current, finalIngestibleDef);
				for (int i = 0; i < list.Count; i++)
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n",
						list[i].defName,
						"(",
						FoodUtility.FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect).ToString("F0"),
						")"
					});
				}
				Widgets.Label(rect, text);
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private static Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
		{
			if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
			{
				return null;
			}
			bool flag = false;
			float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
			if (summaryHealthPercent < 0.25f)
			{
				flag = true;
			}
			FoodUtility.tmpPredatorCandidates.Clear();
			int maxRegionsToScan = FoodUtility.GetMaxRegionsToScan(predator, forceScanWholeMap);
			if (maxRegionsToScan < 0)
			{
				FoodUtility.tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
			}
			else
			{
				TraverseParms traverseParms = TraverseParms.For(predator, Danger.Deadly, TraverseMode.ByPawn, false);
				RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, true), delegate(Region x)
				{
					List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
					for (int j = 0; j < list.Count; j++)
					{
						FoodUtility.tmpPredatorCandidates.Add((Pawn)list[j]);
					}
					return false;
				}, 999999, RegionType.Set_Passable);
			}
			Pawn pawn = null;
			float num = 0f;
			bool tutorialMode = TutorSystem.TutorialMode;
			for (int i = 0; i < FoodUtility.tmpPredatorCandidates.Count; i++)
			{
				Pawn pawn2 = FoodUtility.tmpPredatorCandidates[i];
				if (predator.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
				{
					if (predator != pawn2)
					{
						if (!flag || pawn2.Downed)
						{
							if (FoodUtility.IsAcceptablePreyFor(predator, pawn2))
							{
								if (predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
								{
									if (!pawn2.IsForbidden(predator))
									{
										if (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
										{
											float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
											if (preyScoreFor > num || pawn == null)
											{
												num = preyScoreFor;
												pawn = pawn2;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			FoodUtility.tmpPredatorCandidates.Clear();
			return pawn;
		}

		public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
		{
			if (!prey.RaceProps.canBePredatorPrey)
			{
				return false;
			}
			if (!prey.RaceProps.IsFlesh)
			{
				return false;
			}
			if (!Find.Storyteller.difficulty.predatorsHuntHumanlikes && prey.RaceProps.Humanlike)
			{
				return false;
			}
			if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
			{
				return false;
			}
			if (!prey.Downed)
			{
				if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
				{
					return false;
				}
				float num = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
				float num2 = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;
				if (num >= num2)
				{
					return false;
				}
			}
			return (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (predator.Faction == null || prey.HostFaction == null || predator.HostileTo(prey)) && (predator.Faction != Faction.OfPlayer || prey.Faction != Faction.OfPlayer) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
		}

		public static float GetPreyScoreFor(Pawn predator, Pawn prey)
		{
			float num = prey.kindDef.combatPower / predator.kindDef.combatPower;
			float num2 = prey.health.summaryHealth.SummaryHealthPercent;
			float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor;
			float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
			if (prey.Downed)
			{
				num2 = Mathf.Min(num2, 0.2f);
			}
			float num3 = -lengthHorizontal - 56f * num2 * num2 * num * bodySizeFactor;
			if (prey.RaceProps.Humanlike)
			{
				num3 -= 35f;
			}
			return num3;
		}

		public static void DebugDrawPredatorFoodSource()
		{
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null)
			{
				return;
			}
			Thing thing;
			ThingDef thingDef;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out thing, out thingDef, false, false, false, true, false, false, false))
			{
				GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), thing.Position.ToVector3Shifted());
				if (!(thing is Pawn))
				{
					Pawn pawn2 = FoodUtility.BestPawnToHuntForPredator(pawn, true);
					if (pawn2 != null)
					{
						GenDraw.DrawLineBetween(pawn.Position.ToVector3Shifted(), pawn2.Position.ToVector3Shifted());
					}
				}
			}
		}

		public static List<ThoughtDef> ThoughtsFromIngesting(Pawn ingester, Thing foodSource, ThingDef foodDef)
		{
			FoodUtility.ingestThoughts.Clear();
			if (ingester.needs == null || ingester.needs.mood == null)
			{
				return FoodUtility.ingestThoughts;
			}
			if (!ingester.story.traits.HasTrait(TraitDefOf.Ascetic) && foodDef.ingestible.tasteThought != null)
			{
				FoodUtility.ingestThoughts.Add(foodDef.ingestible.tasteThought);
			}
			CompIngredients compIngredients = foodSource.TryGetComp<CompIngredients>();
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
			if (FoodUtility.IsHumanlikeMeat(foodDef) && ingester.RaceProps.Humanlike)
			{
				FoodUtility.ingestThoughts.Add((!ingester.story.traits.HasTrait(TraitDefOf.Cannibal)) ? ThoughtDefOf.AteHumanlikeMeatDirect : ThoughtDefOf.AteHumanlikeMeatDirectCannibal);
			}
			else if (compIngredients != null)
			{
				for (int i = 0; i < compIngredients.ingredients.Count; i++)
				{
					FoodUtility.AddIngestThoughtsFromIngredient(compIngredients.ingredients[i], ingester, FoodUtility.ingestThoughts);
				}
			}
			else if (building_NutrientPasteDispenser != null)
			{
				Thing thing = building_NutrientPasteDispenser.FindFeedInAnyHopper();
				if (thing != null)
				{
					FoodUtility.AddIngestThoughtsFromIngredient(thing.def, ingester, FoodUtility.ingestThoughts);
				}
			}
			if (foodDef.ingestible.specialThoughtDirect != null)
			{
				FoodUtility.ingestThoughts.Add(foodDef.ingestible.specialThoughtDirect);
			}
			if (foodSource.IsNotFresh())
			{
				FoodUtility.ingestThoughts.Add(ThoughtDefOf.AteRottenFood);
			}
			return FoodUtility.ingestThoughts;
		}

		private static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, List<ThoughtDef> ingestThoughts)
		{
			if (ingredient.ingestible == null)
			{
				return;
			}
			if (ingester.RaceProps.Humanlike && FoodUtility.IsHumanlikeMeat(ingredient))
			{
				ingestThoughts.Add((!ingester.story.traits.HasTrait(TraitDefOf.Cannibal)) ? ThoughtDefOf.AteHumanlikeMeatAsIngredient : ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal);
			}
			else if (ingredient.ingestible.specialThoughtAsIngredient != null)
			{
				ingestThoughts.Add(ingredient.ingestible.specialThoughtAsIngredient);
			}
		}

		public static bool IsHumanlikeMeat(ThingDef def)
		{
			return def.ingestible.sourceDef != null && def.ingestible.sourceDef.race != null && def.ingestible.sourceDef.race.Humanlike;
		}

		public static bool IsHumanlikeMeatOrHumanlikeCorpse(Thing thing)
		{
			if (FoodUtility.IsHumanlikeMeat(thing.def))
			{
				return true;
			}
			Corpse corpse = thing as Corpse;
			return corpse != null && corpse.InnerPawn.RaceProps.Humanlike;
		}

		public static int WillIngestStackCountOf(Pawn ingester, ThingDef def, float singleFoodNutrition)
		{
			int num = Mathf.Min(def.ingestible.maxNumToIngestAtOnce, FoodUtility.StackCountForNutrition(ingester.needs.food.NutritionWanted, singleFoodNutrition));
			if (num < 1)
			{
				num = 1;
			}
			return num;
		}

		public static float GetBodyPartNutrition(Corpse corpse, BodyPartRecord part)
		{
			return FoodUtility.GetBodyPartNutrition(corpse.GetStatValue(StatDefOf.Nutrition, true), corpse.InnerPawn, part);
		}

		public static float GetBodyPartNutrition(float currentCorpseNutrition, Pawn pawn, BodyPartRecord part)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(pawn.RaceProps.body.corePart);
			if (coverageOfNotMissingNaturalParts <= 0f)
			{
				return 0f;
			}
			float coverageOfNotMissingNaturalParts2 = hediffSet.GetCoverageOfNotMissingNaturalParts(part);
			float num = coverageOfNotMissingNaturalParts2 / coverageOfNotMissingNaturalParts;
			return currentCorpseNutrition * num;
		}

		public static int StackCountForNutrition(float wantedNutrition, float singleFoodNutrition)
		{
			if (wantedNutrition <= 0.0001f)
			{
				return 0;
			}
			return Mathf.Max(Mathf.RoundToInt(wantedNutrition / singleFoodNutrition), 1);
		}

		public static bool ShouldBeFedBySomeone(Pawn pawn)
		{
			return FeedPatientUtility.ShouldBeFed(pawn) || WardenFeedUtility.ShouldBeFed(pawn);
		}

		public static void AddFoodPoisoningHediff(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
		{
			pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, pawn, null), null, null, null);
			if (PawnUtility.ShouldSendNotificationAbout(pawn) && MessagesRepeatAvoider.MessageShowAllowed("MessageFoodPoisoning-" + pawn.thingIDNumber, 0.1f))
			{
				string text = "MessageFoodPoisoning".Translate(pawn.LabelShort, ingestible.LabelCapNoCount, cause.ToStringHuman().CapitalizeFirst(), pawn.Named("PAWN"), ingestible.Named("FOOD")).CapitalizeFirst();
				Messages.Message(text, pawn, MessageTypeDefOf.NegativeEvent, true);
			}
		}

		public static bool Starving(this Pawn p)
		{
			return p.needs != null && p.needs.food != null && p.needs.food.Starving;
		}

		public static float GetNutrition(Thing foodSource, ThingDef foodDef)
		{
			if (foodSource == null || foodDef == null)
			{
				return 0f;
			}
			if (foodSource.def == foodDef)
			{
				return foodSource.GetStatValue(StatDefOf.Nutrition, true);
			}
			return foodDef.GetStatValueAbstract(StatDefOf.Nutrition, null);
		}
	}
}
