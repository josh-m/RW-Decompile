using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class GenStep_ScatterShrines : GenStep_ScatterRuinsSimple
	{
		private enum PodContentsType
		{
			Undefined,
			SpacerFriendly,
			SpacerIncapped,
			SpacerHalfEaten,
			Slave,
			SpacerHostile
		}

		private const float SkipShrineChance = 0.25f;

		private const int MaxNumCaskets = 6;

		private const int MarginCells = 1;

		private const float MechanoidsChance = 0.5f;

		private const float ArtifactsChance = 0.9f;

		private const float LuciferiumChance = 0.9f;

		private const float HivesChance = 0.45f;

		private static int nextGroupID = 0;

		private static readonly IntRange PodGridSizeXRange = new IntRange(1, 4);

		private static readonly IntRange PodGridSizeZRange = new IntRange(1, 4);

		private static readonly IntRange ShrineExtraHeightRange = new IntRange(0, 8);

		private static readonly IntRange MechanoidCountRange = new IntRange(1, 5);

		private static readonly IntRange ArtifactsCountRange = new IntRange(1, 3);

		private static readonly IntRange HivesCountRange = new IntRange(1, 2);

		protected override bool CanScatterAt(IntVec3 loc)
		{
			if (!base.CanScatterAt(loc))
			{
				return false;
			}
			Building edifice = loc.GetEdifice();
			return edifice != null && edifice.def.building.isNaturalRock;
		}

		protected override void ScatterAt(IntVec3 loc, int stackCount = 1)
		{
			int randomInRange = GenStep_ScatterShrines.PodGridSizeXRange.RandomInRange;
			int randomInRange2 = GenStep_ScatterShrines.PodGridSizeZRange.RandomInRange;
			int num = randomInRange * 4 + (randomInRange - 1);
			int num2 = randomInRange2 * 3 + (randomInRange2 - 1);
			int randomInRange3 = GenStep_ScatterShrines.ShrineExtraHeightRange.RandomInRange;
			int num3 = num + 2;
			int num4 = num2 + 2 + randomInRange3;
			IntVec3 intVec = loc;
			CellRect mapRect = new CellRect(intVec.x, intVec.z, num3, num4);
			mapRect.ClipInsideMap();
			if (mapRect.Width != num3 || mapRect.Height != num4)
			{
				return;
			}
			foreach (IntVec3 current in mapRect.Cells)
			{
				List<Thing> list = Find.ThingGrid.ThingsListAt(current);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].def == ThingDefOf.AncientCryptosleepCasket)
					{
						return;
					}
				}
			}
			base.MakeShed(mapRect, base.RandomWallStuff(), false);
			RectTrigger rectTrigger = (RectTrigger)ThingMaker.MakeThing(ThingDefOf.RectTrigger, null);
			rectTrigger.Rect = mapRect.ExpandedBy(1);
			rectTrigger.letter = new Letter("LetterLabelAncientShrineWarning".Translate(), "AncientShrineWarning".Translate(), LetterType.BadNonUrgent, mapRect.CenterCell);
			rectTrigger.destroyIfUnfogged = true;
			GenSpawn.Spawn(rectTrigger, mapRect.CenterCell);
			IntVec3 intVec2 = loc + new IntVec3(1, 0, 1);
			List<Building_AncientCryptosleepCasket> list2 = new List<Building_AncientCryptosleepCasket>();
			for (int j = 0; j < randomInRange2; j++)
			{
				for (int k = 0; k < randomInRange; k++)
				{
					if (Rand.Value >= 0.25f)
					{
						if (list2.Count >= 6)
						{
							break;
						}
						Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = this.MakeCasketShrine(new CellRect
						{
							minX = intVec2.x + k * 5,
							minZ = intVec2.z + j * 4 + randomInRange3,
							Width = 4,
							Height = 3
						}, GenStep_ScatterShrines.nextGroupID);
						if (building_AncientCryptosleepCasket != null)
						{
							list2.Add(building_AncientCryptosleepCasket);
						}
					}
				}
			}
			float value = Rand.Value;
			if (value < 0.5f)
			{
				foreach (Building_AncientCryptosleepCasket current2 in list2)
				{
					GenStep_ScatterShrines.GeneratePodContents(current2, Gen.RandomEnumValue<GenStep_ScatterShrines.PodContentsType>(true));
				}
			}
			else if (value < 0.7f)
			{
				foreach (Building_AncientCryptosleepCasket current3 in list2)
				{
					GenStep_ScatterShrines.GeneratePodContents(current3, GenStep_ScatterShrines.PodContentsType.Slave);
				}
			}
			else
			{
				foreach (Building_AncientCryptosleepCasket current4 in list2)
				{
					GenStep_ScatterShrines.GeneratePodContents(current4, GenStep_ScatterShrines.PodContentsType.SpacerHostile);
				}
			}
			foreach (Building_AncientCryptosleepCasket current5 in list2)
			{
				GenSpawn.Spawn(current5, current5.Position, Rot4.East);
			}
			GenStep_ScatterShrines.nextGroupID++;
			if (Rand.Value < 0.5f)
			{
				int randomInRange4 = GenStep_ScatterShrines.MechanoidCountRange.RandomInRange;
				List<Pawn> list3 = new List<Pawn>();
				for (int l = 0; l < randomInRange4; l++)
				{
					IntVec3 loc2 = (from mc in mapRect.Cells
					where !mc.Impassable()
					select mc).RandomElement<IntVec3>();
					PawnKindDef kindDef = (from kind in DefDatabase<PawnKindDef>.AllDefs
					where kind.RaceProps.IsMechanoid
					select kind).RandomElementByWeight((PawnKindDef kind) => 1f / kind.combatPower);
					Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfMechanoids);
					pawn.mindState.Active = false;
					GenSpawn.Spawn(pawn, loc2);
					list3.Add(pawn);
				}
				LordJob lordJob;
				if (Rand.Value < 0.5f)
				{
					lordJob = new LordJob_DefendPoint(list3[0].Position);
				}
				else
				{
					lordJob = new LordJob_AssaultColony(Faction.OfMechanoids, false, false, false, false, true);
				}
				LordMaker.MakeNewLord(Faction.OfMechanoids, lordJob, list3);
			}
			else if (Rand.Value < 0.45f)
			{
				int randomInRange5 = GenStep_ScatterShrines.HivesCountRange.RandomInRange;
				Hive hive = (Hive)ThingMaker.MakeThing(ThingDefOf.Hive, null);
				hive.active = false;
				hive.SetFaction(Faction.OfInsects, null);
				IntVec3 loc3 = (from mc in mapRect.Cells
				where mc.Standable()
				select mc).RandomElement<IntVec3>();
				hive = (Hive)GenSpawn.Spawn(hive, loc3);
				for (int m = 0; m < randomInRange5 - 1; m++)
				{
					Hive hive2;
					if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(true, out hive2))
					{
						hive = hive2;
					}
				}
			}
			if (Rand.Value < 0.9f)
			{
				int randomInRange6 = GenStep_ScatterShrines.ArtifactsCountRange.RandomInRange;
				for (int n = 0; n < randomInRange6; n++)
				{
					IntVec3 loc4;
					if (GenStep_ScatterShrines.TryFindClearCellForItem(mapRect, out loc4))
					{
						ThingDef def = (from x in DefDatabase<ThingDef>.AllDefs
						where x.comps.Find((CompProperties y) => y.compClass == typeof(CompUseEffect_Artifact)) != null
						select x).RandomElement<ThingDef>();
						Thing newThing = ThingMaker.MakeThing(def, null);
						GenSpawn.Spawn(newThing, loc4);
					}
				}
			}
			IntVec3 loc5;
			if (Rand.Value < 0.9f && GenStep_ScatterShrines.TryFindClearCellForItem(mapRect, out loc5))
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Luciferium, null);
				thing.stackCount = Rand.Range(10, 40);
				GenSpawn.Spawn(thing, loc5);
			}
		}

		private static bool TryFindClearCellForItem(CellRect mapRect, out IntVec3 result)
		{
			return mapRect.Cells.Where(delegate(IntVec3 c)
			{
				if (!c.Standable())
				{
					return false;
				}
				List<Thing> thingList = c.GetThingList();
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].def.category == ThingCategory.Item || thingList[i].def.category == ThingCategory.Plant || thingList[i].def.category == ThingCategory.Pawn)
					{
						return false;
					}
				}
				return true;
			}).TryRandomElement(out result);
		}

		private Building_AncientCryptosleepCasket MakeCasketShrine(CellRect mapRect, int groupID)
		{
			mapRect.ClipInsideMap();
			CellRect cellRect = new CellRect(mapRect.BottomLeft.x + 1, mapRect.BottomLeft.z + 1, 2, 1);
			cellRect.ClipInsideMap();
			foreach (IntVec3 current in cellRect)
			{
				List<Thing> thingList = current.GetThingList();
				for (int i = 0; i < thingList.Count; i++)
				{
					if (!thingList[i].def.destroyable)
					{
						return null;
					}
				}
			}
			foreach (IntVec3 current2 in mapRect)
			{
				if ((current2.x == mapRect.minX && current2.z == mapRect.minZ) || (current2.x == mapRect.maxX && current2.z == mapRect.minZ) || (current2.x == mapRect.minX && current2.z == mapRect.maxZ) || (current2.x == mapRect.maxX && current2.z == mapRect.maxZ))
				{
					Find.TerrainGrid.SetTerrain(current2, TerrainDefOf.MetalTile);
				}
				else
				{
					Find.TerrainGrid.SetTerrain(current2, TerrainDefOf.Concrete);
				}
			}
			Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket, null);
			building_AncientCryptosleepCasket.groupID = groupID;
			building_AncientCryptosleepCasket.SetPositionDirect(cellRect.BottomLeft);
			return building_AncientCryptosleepCasket;
		}

		private static void GeneratePodContents(Building_CryptosleepCasket casket, GenStep_ScatterShrines.PodContentsType contentsType)
		{
			switch (contentsType)
			{
			case GenStep_ScatterShrines.PodContentsType.SpacerFriendly:
				GenStep_ScatterShrines.GenerateFriendlySpacer(casket);
				break;
			case GenStep_ScatterShrines.PodContentsType.SpacerIncapped:
				GenStep_ScatterShrines.GenerateIncappedSpacer(casket);
				break;
			case GenStep_ScatterShrines.PodContentsType.SpacerHalfEaten:
				GenStep_ScatterShrines.GenerateHalfEatenSpacer(casket);
				break;
			case GenStep_ScatterShrines.PodContentsType.Slave:
				GenStep_ScatterShrines.GenerateSlave(casket);
				break;
			case GenStep_ScatterShrines.PodContentsType.SpacerHostile:
				GenStep_ScatterShrines.GenerateAngrySpacer(casket);
				break;
			}
		}

		private static void GenerateFriendlySpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			GenStep_ScatterShrines.GiveRandomLootInventoryForTombPawn(pawn);
		}

		private static void GenerateIncappedSpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			HealthUtility.GiveInjuriesToForceDowned(pawn);
			GenStep_ScatterShrines.GiveRandomLootInventoryForTombPawn(pawn);
		}

		private static void GenerateSlave(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Slave, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			HealthUtility.GiveInjuriesToForceDowned(pawn);
			GenStep_ScatterShrines.GiveRandomLootInventoryForTombPawn(pawn);
			if (Rand.Value < 0.5f)
			{
				HealthUtility.GiveInjuriesToKill(pawn);
			}
		}

		private static void GenerateAngrySpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			GenStep_ScatterShrines.GiveRandomLootInventoryForTombPawn(pawn);
		}

		private static void GenerateHalfEatenSpacer(Building_CryptosleepCasket pod)
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.SpaceSoldier, faction);
			if (!pod.TryAcceptThing(pawn, false))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				return;
			}
			int num = Rand.Range(6, 10);
			for (int i = 0; i < num; i++)
			{
				pawn.TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(3, 8), pawn, null, null));
			}
			GenStep_ScatterShrines.GiveRandomLootInventoryForTombPawn(pawn);
			int num2 = Rand.Range(3, 6);
			for (int j = 0; j < num2; j++)
			{
				Pawn pawn2 = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, null);
				if (!pod.TryAcceptThing(pawn2, false))
				{
					Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
					return;
				}
				pawn2.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false);
			}
		}

		private static void GiveRandomLootInventoryForTombPawn(Pawn p)
		{
			if ((double)Rand.Value < 0.65)
			{
				GenStep_ScatterShrines.MakeIntoContainer(p.inventory.container, ThingDefOf.Gold, Rand.Range(10, 50));
			}
			else
			{
				GenStep_ScatterShrines.MakeIntoContainer(p.inventory.container, ThingDefOf.Plasteel, Rand.Range(10, 50));
			}
			GenStep_ScatterShrines.MakeIntoContainer(p.inventory.container, ThingDefOf.Component, Rand.Range(-2, 4));
		}

		private static void MakeIntoContainer(ThingContainer container, ThingDef def, int count)
		{
			if (count <= 0)
			{
				return;
			}
			Thing thing = ThingMaker.MakeThing(def, null);
			thing.stackCount = count;
			container.TryAdd(thing);
		}
	}
}
