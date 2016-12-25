using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class Autotests_ColonyMaker
	{
		private const int OverRectSize = 100;

		private static CellRect overRect;

		private static BoolGrid usedCells;

		private static Map Map
		{
			get
			{
				return Find.VisibleMap;
			}
		}

		public static void MakeColony_Full()
		{
			Autotests_ColonyMaker.MakeColony(new ColonyMakerFlag[]
			{
				ColonyMakerFlag.ConduitGrid,
				ColonyMakerFlag.PowerPlants,
				ColonyMakerFlag.Batteries,
				ColonyMakerFlag.WorkTables,
				ColonyMakerFlag.AllBuildings,
				ColonyMakerFlag.AllItems,
				ColonyMakerFlag.Filth,
				ColonyMakerFlag.ColonistsMany,
				ColonyMakerFlag.ColonistsHungry,
				ColonyMakerFlag.ColonistsTired,
				ColonyMakerFlag.ColonistsInjured,
				ColonyMakerFlag.ColonistsDiseased,
				ColonyMakerFlag.Beds,
				ColonyMakerFlag.Stockpiles,
				ColonyMakerFlag.GrowingZones
			});
		}

		public static void MakeColony_Animals()
		{
			Autotests_ColonyMaker.MakeColony(new ColonyMakerFlag[1]);
		}

		public static void MakeColony(params ColonyMakerFlag[] flags)
		{
			bool godMode = DebugSettings.godMode;
			DebugSettings.godMode = true;
			Thing.allowDestroyNonDestroyable = true;
			if (Autotests_ColonyMaker.usedCells == null)
			{
				Autotests_ColonyMaker.usedCells = new BoolGrid(Autotests_ColonyMaker.Map);
			}
			else
			{
				Autotests_ColonyMaker.usedCells.ClearAndResizeTo(Autotests_ColonyMaker.Map);
			}
			Autotests_ColonyMaker.overRect = new CellRect(Autotests_ColonyMaker.Map.Center.x - 50, Autotests_ColonyMaker.Map.Center.z - 50, 100, 100);
			Autotests_ColonyMaker.DeleteAllSpawnedPawns();
			GenDebug.ClearArea(Autotests_ColonyMaker.overRect, Find.VisibleMap);
			if (flags.Contains(ColonyMakerFlag.Animals))
			{
				foreach (PawnKindDef current in from k in DefDatabase<PawnKindDef>.AllDefs
				where k.RaceProps.Animal
				select k)
				{
					CellRect cellRect;
					if (!Autotests_ColonyMaker.TryGetFreeRect(6, 3, out cellRect))
					{
						return;
					}
					cellRect = cellRect.ContractedBy(1);
					foreach (IntVec3 current2 in cellRect)
					{
						Autotests_ColonyMaker.Map.terrainGrid.SetTerrain(current2, TerrainDefOf.Concrete);
					}
					GenSpawn.Spawn(PawnGenerator.GeneratePawn(current, null), cellRect.Cells.ElementAt(0), Autotests_ColonyMaker.Map);
					IntVec3 intVec = cellRect.Cells.ElementAt(1);
					Pawn p = (Pawn)GenSpawn.Spawn(PawnGenerator.GeneratePawn(current, null), intVec, Autotests_ColonyMaker.Map);
					HealthUtility.GiveInjuriesToKill(p);
					Corpse thing = (Corpse)intVec.GetThingList(Find.VisibleMap).First((Thing t) => t is Corpse);
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress += 1200000f;
					}
					if (current.RaceProps.leatherDef != null)
					{
						GenSpawn.Spawn(current.RaceProps.leatherDef, cellRect.Cells.ElementAt(2), Autotests_ColonyMaker.Map);
					}
					if (current.RaceProps.meatDef != null)
					{
						GenSpawn.Spawn(current.RaceProps.meatDef, cellRect.Cells.ElementAt(3), Autotests_ColonyMaker.Map);
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.ConduitGrid))
			{
				Designator_Build designator_Build = new Designator_Build(ThingDefOf.PowerConduit);
				for (int i = Autotests_ColonyMaker.overRect.minX; i < Autotests_ColonyMaker.overRect.maxX; i++)
				{
					for (int j = Autotests_ColonyMaker.overRect.minZ; j < Autotests_ColonyMaker.overRect.maxZ; j += 7)
					{
						designator_Build.DesignateSingleCell(new IntVec3(i, 0, j));
					}
				}
				for (int k2 = Autotests_ColonyMaker.overRect.minZ; k2 < Autotests_ColonyMaker.overRect.maxZ; k2++)
				{
					for (int l = Autotests_ColonyMaker.overRect.minX; l < Autotests_ColonyMaker.overRect.maxX; l += 7)
					{
						designator_Build.DesignateSingleCell(new IntVec3(l, 0, k2));
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.PowerPlants))
			{
				List<ThingDef> list = new List<ThingDef>
				{
					ThingDefOf.SolarGenerator,
					ThingDefOf.WindTurbine
				};
				for (int m = 0; m < 8; m++)
				{
					if (Autotests_ColonyMaker.TryMakeBuilding(list[m % list.Count]) == null)
					{
						Log.Message("Could not make solar generator.");
						break;
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.Batteries))
			{
				for (int n = 0; n < 6; n++)
				{
					Thing thing2 = Autotests_ColonyMaker.TryMakeBuilding(ThingDefOf.Battery);
					if (thing2 == null)
					{
						Log.Message("Could not make battery.");
						break;
					}
					((Building_Battery)thing2).GetComp<CompPowerBattery>().AddEnergy(999999f);
				}
			}
			if (flags.Contains(ColonyMakerFlag.WorkTables))
			{
				IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
				where typeof(Building_WorkTable).IsAssignableFrom(def.thingClass)
				select def;
				foreach (ThingDef current3 in enumerable)
				{
					Thing thing3 = Autotests_ColonyMaker.TryMakeBuilding(current3);
					if (thing3 == null)
					{
						Log.Message("Could not make worktable: " + current3.defName);
						break;
					}
					Building_WorkTable building_WorkTable = thing3 as Building_WorkTable;
					if (building_WorkTable != null)
					{
						foreach (RecipeDef current4 in building_WorkTable.def.AllRecipes)
						{
							building_WorkTable.billStack.AddBill(current4.MakeNewBill());
						}
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.AllBuildings))
			{
				IEnumerable<ThingDef> enumerable2 = from def in DefDatabase<ThingDef>.AllDefs
				where def.category == ThingCategory.Building && def.designationCategory != null
				select def;
				foreach (ThingDef current5 in enumerable2)
				{
					if (current5 != ThingDefOf.PowerConduit)
					{
						if (Autotests_ColonyMaker.TryMakeBuilding(current5) == null)
						{
							Log.Message("Could not make building: " + current5.defName);
							break;
						}
					}
				}
			}
			CellRect rect;
			if (!Autotests_ColonyMaker.TryGetFreeRect(33, 33, out rect))
			{
				Log.Error("Could not get wallable rect");
			}
			rect = rect.ContractedBy(1);
			if (flags.Contains(ColonyMakerFlag.AllItems))
			{
				List<ThingDef> itemDefs = (from def in DefDatabase<ThingDef>.AllDefs
				where DebugThingPlaceHelper.IsDebugSpawnable(def) && def.category == ThingCategory.Item
				select def).ToList<ThingDef>();
				Autotests_ColonyMaker.FillWithItems(rect, itemDefs);
			}
			else if (flags.Contains(ColonyMakerFlag.ItemsRawFood))
			{
				List<ThingDef> list2 = new List<ThingDef>();
				list2.Add(ThingDefOf.RawPotatoes);
				Autotests_ColonyMaker.FillWithItems(rect, list2);
			}
			if (flags.Contains(ColonyMakerFlag.Filth))
			{
				foreach (IntVec3 current6 in rect)
				{
					GenSpawn.Spawn(ThingDefOf.FilthDirt, current6, Autotests_ColonyMaker.Map);
				}
			}
			if (flags.Contains(ColonyMakerFlag.ItemsWall))
			{
				CellRect cellRect2 = rect.ExpandedBy(1);
				Designator_Build designator_Build2 = new Designator_Build(ThingDefOf.Wall);
				designator_Build2.SetStuffDef(ThingDefOf.WoodLog);
				foreach (IntVec3 current7 in cellRect2.EdgeCells)
				{
					designator_Build2.DesignateSingleCell(current7);
				}
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsMany))
			{
				Autotests_ColonyMaker.MakeColonists(15, Autotests_ColonyMaker.overRect.CenterCell);
			}
			else if (flags.Contains(ColonyMakerFlag.ColonistOne))
			{
				Autotests_ColonyMaker.MakeColonists(1, Autotests_ColonyMaker.overRect.CenterCell);
			}
			if (flags.Contains(ColonyMakerFlag.Fire))
			{
				CellRect cellRect3;
				if (!Autotests_ColonyMaker.TryGetFreeRect(30, 30, out cellRect3))
				{
					Log.Error("Could not get free rect for fire.");
				}
				ThingDef plantTreeOak = ThingDefOf.PlantTreeOak;
				foreach (IntVec3 current8 in cellRect3)
				{
					GenSpawn.Spawn(plantTreeOak, current8, Autotests_ColonyMaker.Map);
				}
				foreach (IntVec3 current9 in cellRect3)
				{
					if (current9.x % 7 == 0 && current9.z % 7 == 0)
					{
						GenExplosion.DoExplosion(current9, Find.VisibleMap, 3.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsHungry))
			{
				Autotests_ColonyMaker.DoToColonists(0.4f, delegate(Pawn col)
				{
					col.needs.food.CurLevel = Mathf.Max(0f, Rand.Range(-0.05f, 0.05f));
				});
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsTired))
			{
				Autotests_ColonyMaker.DoToColonists(0.4f, delegate(Pawn col)
				{
					col.needs.rest.CurLevel = Mathf.Max(0f, Rand.Range(-0.05f, 0.05f));
				});
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsInjured))
			{
				Autotests_ColonyMaker.DoToColonists(0.4f, delegate(Pawn col)
				{
					DamageDef def = (from d in DefDatabase<DamageDef>.AllDefs
					where d.externalViolence
					select d).RandomElement<DamageDef>();
					col.TakeDamage(new DamageInfo(def, 10, -1f, null, null, null));
				});
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsDiseased))
			{
				foreach (HediffDef current10 in from d in DefDatabase<HediffDef>.AllDefs
				where d.hediffClass != typeof(Hediff_AddedPart) && (d.HasComp(typeof(HediffComp_Immunizable)) || d.HasComp(typeof(HediffComp_GrowthMode)))
				select d)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
					CellRect cellRect4;
					Autotests_ColonyMaker.TryGetFreeRect(1, 1, out cellRect4);
					GenSpawn.Spawn(pawn, cellRect4.CenterCell, Autotests_ColonyMaker.Map);
					pawn.health.AddHediff(current10, null, null);
				}
			}
			if (flags.Contains(ColonyMakerFlag.Beds))
			{
				IEnumerable<ThingDef> source = from def in DefDatabase<ThingDef>.AllDefs
				where def.thingClass == typeof(Building_Bed)
				select def;
				int freeColonistsCount = Autotests_ColonyMaker.Map.mapPawns.FreeColonistsCount;
				for (int num = 0; num < freeColonistsCount; num++)
				{
					if (Autotests_ColonyMaker.TryMakeBuilding(source.RandomElement<ThingDef>()) == null)
					{
						Log.Message("Could not make beds.");
						break;
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.Stockpiles))
			{
				Designator_ZoneAddStockpile_Resources designator_ZoneAddStockpile_Resources = new Designator_ZoneAddStockpile_Resources();
				using (IEnumerator enumerator11 = Enum.GetValues(typeof(StoragePriority)).GetEnumerator())
				{
					while (enumerator11.MoveNext())
					{
						StoragePriority priority = (StoragePriority)((byte)enumerator11.Current);
						CellRect cellRect5;
						Autotests_ColonyMaker.TryGetFreeRect(7, 7, out cellRect5);
						cellRect5 = cellRect5.ContractedBy(1);
						designator_ZoneAddStockpile_Resources.DesignateMultiCell(cellRect5.Cells);
						Zone_Stockpile zone_Stockpile = (Zone_Stockpile)Autotests_ColonyMaker.Map.zoneManager.ZoneAt(cellRect5.CenterCell);
						zone_Stockpile.settings.Priority = priority;
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.GrowingZones))
			{
				Zone_Growing dummyZone = new Zone_Growing(Autotests_ColonyMaker.Map.zoneManager);
				foreach (ThingDef current11 in from d in DefDatabase<ThingDef>.AllDefs
				where d.plant != null && GenPlant.CanSowOnGrower(d, dummyZone)
				select d)
				{
					CellRect cellRect6;
					if (!Autotests_ColonyMaker.TryGetFreeRect(6, 6, out cellRect6))
					{
						Log.Error("Could not get growing zone rect.");
					}
					cellRect6 = cellRect6.ContractedBy(1);
					foreach (IntVec3 current12 in cellRect6)
					{
						Autotests_ColonyMaker.Map.terrainGrid.SetTerrain(current12, TerrainDefOf.Soil);
					}
					Designator_ZoneAdd_Growing designator_ZoneAdd_Growing = new Designator_ZoneAdd_Growing();
					designator_ZoneAdd_Growing.DesignateMultiCell(cellRect6.Cells);
					Zone_Growing zone_Growing = (Zone_Growing)Autotests_ColonyMaker.Map.zoneManager.ZoneAt(cellRect6.CenterCell);
					zone_Growing.SetPlantDefToGrow(current11);
				}
				dummyZone.Delete();
			}
			Autotests_ColonyMaker.ClearAllHomeArea();
			Autotests_ColonyMaker.FillWithHomeArea(Autotests_ColonyMaker.overRect);
			DebugSettings.godMode = godMode;
			Thing.allowDestroyNonDestroyable = false;
		}

		private static void FillWithItems(CellRect rect, List<ThingDef> itemDefs)
		{
			int num = 0;
			foreach (IntVec3 current in rect)
			{
				if (current.x % 6 != 0 && current.z % 6 != 0)
				{
					ThingDef def = itemDefs[num];
					DebugThingPlaceHelper.DebugSpawn(def, current, -1, true);
					num++;
					if (num >= itemDefs.Count)
					{
						num = 0;
					}
				}
			}
		}

		private static Thing TryMakeBuilding(ThingDef def)
		{
			CellRect cellRect;
			if (!Autotests_ColonyMaker.TryGetFreeRect(def.size.x + 2, def.size.z + 2, out cellRect))
			{
				return null;
			}
			foreach (IntVec3 current in cellRect)
			{
				Autotests_ColonyMaker.Map.terrainGrid.SetTerrain(current, TerrainDefOf.Concrete);
			}
			Designator_Build designator_Build = new Designator_Build(def);
			designator_Build.DesignateSingleCell(cellRect.CenterCell);
			return cellRect.CenterCell.GetEdifice(Find.VisibleMap);
		}

		private static bool TryGetFreeRect(int width, int height, out CellRect result)
		{
			for (int i = Autotests_ColonyMaker.overRect.minZ; i <= Autotests_ColonyMaker.overRect.maxZ - height; i++)
			{
				for (int j = Autotests_ColonyMaker.overRect.minX; j <= Autotests_ColonyMaker.overRect.maxX - width; j++)
				{
					CellRect cellRect = new CellRect(j, i, width, height);
					bool flag = true;
					for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
					{
						for (int l = cellRect.minX; l <= cellRect.maxX; l++)
						{
							if (Autotests_ColonyMaker.usedCells[l, k])
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					if (flag)
					{
						result = cellRect;
						for (int m = cellRect.minZ; m <= cellRect.maxZ; m++)
						{
							for (int n = cellRect.minX; n <= cellRect.maxX; n++)
							{
								IntVec3 c = new IntVec3(n, 0, m);
								Autotests_ColonyMaker.usedCells.Set(c, true);
								if (c.GetTerrain(Find.VisibleMap).passability == Traversability.Impassable)
								{
									Autotests_ColonyMaker.Map.terrainGrid.SetTerrain(c, TerrainDefOf.Concrete);
								}
							}
						}
						return true;
					}
				}
			}
			result = new CellRect(0, 0, width, height);
			return false;
		}

		private static void DoToColonists(float fraction, Action<Pawn> funcToDo)
		{
			int num = Rand.RangeInclusive(1, Mathf.RoundToInt((float)Autotests_ColonyMaker.Map.mapPawns.FreeColonistsCount * fraction));
			int num2 = 0;
			foreach (Pawn current in Autotests_ColonyMaker.Map.mapPawns.FreeColonists.InRandomOrder(null))
			{
				funcToDo(current);
				num2++;
				if (num2 >= num)
				{
					break;
				}
			}
		}

		private static void MakeColonists(int count, IntVec3 center)
		{
			for (int i = 0; i < count; i++)
			{
				CellRect cellRect;
				Autotests_ColonyMaker.TryGetFreeRect(1, 1, out cellRect);
				Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
				{
					if (!pawn.story.WorkTypeIsDisabled(current))
					{
						pawn.workSettings.SetPriority(current, 3);
					}
				}
				GenSpawn.Spawn(pawn, cellRect.CenterCell, Autotests_ColonyMaker.Map);
			}
		}

		private static void DeleteAllSpawnedPawns()
		{
			foreach (Pawn current in Autotests_ColonyMaker.Map.mapPawns.AllPawnsSpawned.ToList<Pawn>())
			{
				current.Destroy(DestroyMode.Vanish);
				current.relations.ClearAllRelations();
			}
			Find.GameEnder.gameEnding = false;
		}

		private static void ClearAllHomeArea()
		{
			foreach (IntVec3 current in Autotests_ColonyMaker.Map.AllCells)
			{
				Autotests_ColonyMaker.Map.areaManager.Home[current] = false;
			}
		}

		private static void FillWithHomeArea(CellRect r)
		{
			Designator_AreaHomeExpand designator_AreaHomeExpand = new Designator_AreaHomeExpand();
			designator_AreaHomeExpand.DesignateMultiCell(r.Cells);
		}
	}
}
