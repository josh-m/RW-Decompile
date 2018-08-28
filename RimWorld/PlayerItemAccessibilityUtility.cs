using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PlayerItemAccessibilityUtility
	{
		private static List<Thing> cachedAccessibleThings = new List<Thing>();

		private static List<ThingDefCount> cachedPossiblyAccessibleThings = new List<ThingDefCount>();

		private static HashSet<ThingDef> cachedMakeableItemDefs = new HashSet<ThingDef>();

		private static int cachedAccessibleThingsForTile = -1;

		private static int cachedAccessibleThingsForFrame = -1;

		private static List<Thing> tmpThings = new List<Thing>();

		private static HashSet<ThingDef> tmpWorkTables = new HashSet<ThingDef>();

		private const float MaxDistanceInTilesToConsiderAccessible = 5f;

		public static bool Accessible(ThingDef thing, int count, Map map)
		{
			PlayerItemAccessibilityUtility.CacheAccessibleThings(map.Tile);
			int num = 0;
			for (int i = 0; i < PlayerItemAccessibilityUtility.cachedAccessibleThings.Count; i++)
			{
				if (PlayerItemAccessibilityUtility.cachedAccessibleThings[i].def == thing)
				{
					num += PlayerItemAccessibilityUtility.cachedAccessibleThings[i].stackCount;
				}
			}
			return num >= count;
		}

		public static bool PossiblyAccessible(ThingDef thing, int count, Map map)
		{
			if (PlayerItemAccessibilityUtility.Accessible(thing, count, map))
			{
				return true;
			}
			PlayerItemAccessibilityUtility.CacheAccessibleThings(map.Tile);
			int num = 0;
			for (int i = 0; i < PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Count; i++)
			{
				if (PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings[i].ThingDef == thing)
				{
					num += PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings[i].Count;
				}
			}
			return num >= count;
		}

		public static bool PlayerCanMake(ThingDef thing, Map map)
		{
			PlayerItemAccessibilityUtility.CacheAccessibleThings(map.Tile);
			return PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Contains(thing);
		}

		private static void CacheAccessibleThings(int nearTile)
		{
			if (nearTile == PlayerItemAccessibilityUtility.cachedAccessibleThingsForTile && RealTime.frameCount == PlayerItemAccessibilityUtility.cachedAccessibleThingsForFrame)
			{
				return;
			}
			PlayerItemAccessibilityUtility.cachedAccessibleThings.Clear();
			PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Clear();
			PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Clear();
			WorldGrid worldGrid = Find.WorldGrid;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				float num = worldGrid.ApproxDistanceInTiles(nearTile, maps[i].Tile);
				if (num <= 5f)
				{
					ThingOwnerUtility.GetAllThingsRecursively(maps[i], PlayerItemAccessibilityUtility.tmpThings, false, null);
					PlayerItemAccessibilityUtility.cachedAccessibleThings.AddRange(PlayerItemAccessibilityUtility.tmpThings);
				}
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				if (caravans[j].IsPlayerControlled)
				{
					float num2 = worldGrid.ApproxDistanceInTiles(nearTile, caravans[j].Tile);
					if (num2 <= 5f)
					{
						ThingOwnerUtility.GetAllThingsRecursively(caravans[j], PlayerItemAccessibilityUtility.tmpThings, false, null);
						PlayerItemAccessibilityUtility.cachedAccessibleThings.AddRange(PlayerItemAccessibilityUtility.tmpThings);
					}
				}
			}
			for (int k = 0; k < PlayerItemAccessibilityUtility.cachedAccessibleThings.Count; k++)
			{
				Thing thing = PlayerItemAccessibilityUtility.cachedAccessibleThings[k];
				PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(thing.def, thing.stackCount));
				if (GenLeaving.CanBuildingLeaveResources(thing, DestroyMode.Deconstruct))
				{
					List<ThingDefCountClass> list = thing.CostListAdjusted();
					for (int l = 0; l < list.Count; l++)
					{
						int num3 = Mathf.RoundToInt((float)list[l].count * thing.def.resourcesFractionWhenDeconstructed);
						if (num3 > 0)
						{
							PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(list[l].thingDef, num3));
							PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(list[l].thingDef);
						}
					}
				}
				Plant plant = thing as Plant;
				if (plant != null && (plant.HarvestableNow || plant.HarvestableSoon))
				{
					int num4 = Mathf.RoundToInt(plant.def.plant.harvestYield * Find.Storyteller.difficulty.cropYieldFactor);
					if (num4 > 0)
					{
						PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(plant.def.plant.harvestedThingDef, num4));
						PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(plant.def.plant.harvestedThingDef);
					}
				}
				if (!thing.def.butcherProducts.NullOrEmpty<ThingDefCountClass>())
				{
					for (int m = 0; m < thing.def.butcherProducts.Count; m++)
					{
						PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(thing.def.butcherProducts[m]);
						PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(thing.def.butcherProducts[m].thingDef);
					}
				}
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					if (pawn.RaceProps.meatDef != null)
					{
						int num5 = Mathf.RoundToInt(pawn.GetStatValue(StatDefOf.MeatAmount, true));
						if (num5 > 0)
						{
							PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(pawn.RaceProps.meatDef, num5));
							PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(pawn.RaceProps.meatDef);
						}
					}
					if (pawn.RaceProps.leatherDef != null)
					{
						int num6 = GenMath.RoundRandom(pawn.GetStatValue(StatDefOf.LeatherAmount, true));
						if (num6 > 0)
						{
							PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(pawn.RaceProps.leatherDef, num6));
							PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(pawn.RaceProps.leatherDef);
						}
					}
					if (!pawn.RaceProps.Humanlike)
					{
						PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
						if (curKindLifeStage.butcherBodyPart != null)
						{
							PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(curKindLifeStage.butcherBodyPart.thing, 1));
							PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(curKindLifeStage.butcherBodyPart.thing);
						}
					}
				}
				if (thing.def.smeltable)
				{
					List<ThingDefCountClass> list2 = thing.CostListAdjusted();
					for (int n = 0; n < list2.Count; n++)
					{
						if (!list2[n].thingDef.intricate)
						{
							int num7 = Mathf.RoundToInt((float)list2[n].count * 0.25f);
							if (num7 > 0)
							{
								PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(list2[n].thingDef, num7));
								PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(list2[n].thingDef);
							}
						}
					}
				}
				if (thing.def.smeltable && !thing.def.smeltProducts.NullOrEmpty<ThingDefCountClass>())
				{
					for (int num8 = 0; num8 < thing.def.smeltProducts.Count; num8++)
					{
						PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(thing.def.smeltProducts[num8]);
						PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(thing.def.smeltProducts[num8].thingDef);
					}
				}
			}
			int num9 = 0;
			for (int num10 = 0; num10 < PlayerItemAccessibilityUtility.cachedAccessibleThings.Count; num10++)
			{
				Pawn pawn2 = PlayerItemAccessibilityUtility.cachedAccessibleThings[num10] as Pawn;
				if (pawn2 != null && pawn2.IsFreeColonist && !pawn2.Dead && !pawn2.Downed && pawn2.workSettings.WorkIsActive(WorkTypeDefOf.Crafting))
				{
					num9++;
				}
			}
			if (num9 > 0)
			{
				PlayerItemAccessibilityUtility.tmpWorkTables.Clear();
				for (int num11 = 0; num11 < PlayerItemAccessibilityUtility.cachedAccessibleThings.Count; num11++)
				{
					Building_WorkTable building_WorkTable = PlayerItemAccessibilityUtility.cachedAccessibleThings[num11] as Building_WorkTable;
					if (building_WorkTable != null && building_WorkTable.Spawned && PlayerItemAccessibilityUtility.tmpWorkTables.Add(building_WorkTable.def))
					{
						List<RecipeDef> allRecipes = building_WorkTable.def.AllRecipes;
						for (int num12 = 0; num12 < allRecipes.Count; num12++)
						{
							if (allRecipes[num12].AvailableNow)
							{
								if (allRecipes[num12].products.Any<ThingDefCountClass>())
								{
									if (!allRecipes[num12].PotentiallyMissingIngredients(null, building_WorkTable.Map).Any<ThingDef>())
									{
										ThingDef stuffDef = (!allRecipes[num12].products[0].thingDef.MadeFromStuff) ? null : GenStuff.DefaultStuffFor(allRecipes[num12].products[0].thingDef);
										float num13 = allRecipes[num12].WorkAmountTotal(stuffDef);
										if (num13 > 0f)
										{
											int num14 = Mathf.FloorToInt((float)(num9 * 60000 * 5) * 0.09f / num13);
											if (num14 > 0)
											{
												for (int num15 = 0; num15 < allRecipes[num12].products.Count; num15++)
												{
													PlayerItemAccessibilityUtility.cachedPossiblyAccessibleThings.Add(new ThingDefCount(allRecipes[num12].products[num15].thingDef, allRecipes[num12].products[num15].count * num14));
													PlayerItemAccessibilityUtility.cachedMakeableItemDefs.Add(allRecipes[num12].products[num15].thingDef);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			PlayerItemAccessibilityUtility.cachedAccessibleThingsForTile = nearTile;
			PlayerItemAccessibilityUtility.cachedAccessibleThingsForFrame = RealTime.frameCount;
		}

		public static bool PlayerOrQuestRewardHas(ThingFilter thingFilter)
		{
			ThingRequest bestThingRequest = thingFilter.BestThingRequest;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Thing> list = maps[i].listerThings.ThingsMatching(bestThingRequest);
				for (int j = 0; j < list.Count; j++)
				{
					if (thingFilter.Allows(list[j]))
					{
						return true;
					}
				}
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int k = 0; k < caravans.Count; k++)
			{
				if (caravans[k].IsPlayerControlled)
				{
					List<Thing> list2 = CaravanInventoryUtility.AllInventoryItems(caravans[k]);
					for (int l = 0; l < list2.Count; l++)
					{
						if (thingFilter.Allows(list2[l]))
						{
							return true;
						}
					}
				}
			}
			List<Site> sites = Find.WorldObjects.Sites;
			for (int m = 0; m < sites.Count; m++)
			{
				ItemStashContentsComp component = sites[m].GetComponent<ItemStashContentsComp>();
				if (component != null)
				{
					ThingOwner contents = component.contents;
					for (int n = 0; n < contents.Count; n++)
					{
						if (thingFilter.Allows(contents[n]))
						{
							return true;
						}
					}
				}
				DefeatAllEnemiesQuestComp component2 = sites[m].GetComponent<DefeatAllEnemiesQuestComp>();
				if (component2 != null)
				{
					ThingOwner rewards = component2.rewards;
					for (int num = 0; num < rewards.Count; num++)
					{
						if (thingFilter.Allows(rewards[num]))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static bool PlayerOrQuestRewardHas(ThingDef thingDef)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].listerThings.ThingsOfDef(thingDef).Count > 0)
				{
					return true;
				}
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				if (caravans[j].IsPlayerControlled)
				{
					List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravans[j]);
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].def == thingDef)
						{
							return true;
						}
					}
				}
			}
			List<Site> sites = Find.WorldObjects.Sites;
			for (int l = 0; l < sites.Count; l++)
			{
				ItemStashContentsComp component = sites[l].GetComponent<ItemStashContentsComp>();
				if (component != null)
				{
					ThingOwner contents = component.contents;
					for (int m = 0; m < contents.Count; m++)
					{
						if (contents[m].def == thingDef)
						{
							return true;
						}
					}
				}
				DefeatAllEnemiesQuestComp component2 = sites[l].GetComponent<DefeatAllEnemiesQuestComp>();
				if (component2 != null)
				{
					ThingOwner rewards = component2.rewards;
					for (int n = 0; n < rewards.Count; n++)
					{
						if (rewards[n].def == thingDef)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static bool ItemStashHas(ThingDef thingDef)
		{
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				ItemStashContentsComp component = allWorldObjects[i].GetComponent<ItemStashContentsComp>();
				if (component != null)
				{
					ThingOwner contents = component.contents;
					for (int j = 0; j < contents.Count; j++)
					{
						if (contents[j].def == thingDef)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
