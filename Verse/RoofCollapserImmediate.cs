using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class RoofCollapserImmediate
	{
		private static readonly IntRange ThinRoofCrushDamageRange = new IntRange(15, 30);

		public static void DropRoofInCells(IntVec3 c, Map map)
		{
			if (!c.Roofed(map))
			{
				return;
			}
			RoofCollapserImmediate.DropRoofInCellPhaseOne(c, map);
			RoofCollapserImmediate.DropRoofInCellPhaseTwo(c, map);
			SoundDefOf.RoofCollapse.PlayOneShot(new TargetInfo(c, map, false));
		}

		public static void DropRoofInCells(IEnumerable<IntVec3> cells, Map map)
		{
			IntVec3 cell = IntVec3.Invalid;
			foreach (IntVec3 current in cells)
			{
				if (current.Roofed(map))
				{
					RoofCollapserImmediate.DropRoofInCellPhaseOne(current, map);
				}
			}
			foreach (IntVec3 current2 in cells)
			{
				if (current2.Roofed(map))
				{
					RoofCollapserImmediate.DropRoofInCellPhaseTwo(current2, map);
					cell = current2;
				}
			}
			if (cell.IsValid)
			{
				SoundDefOf.RoofCollapse.PlayOneShot(new TargetInfo(cell, map, false));
			}
		}

		public static void DropRoofInCells(List<IntVec3> cells, Map map)
		{
			if (cells.NullOrEmpty<IntVec3>())
			{
				return;
			}
			IntVec3 cell = IntVec3.Invalid;
			for (int i = 0; i < cells.Count; i++)
			{
				if (cells[i].Roofed(map))
				{
					RoofCollapserImmediate.DropRoofInCellPhaseOne(cells[i], map);
				}
			}
			for (int j = 0; j < cells.Count; j++)
			{
				if (cells[j].Roofed(map))
				{
					RoofCollapserImmediate.DropRoofInCellPhaseTwo(cells[j], map);
					cell = cells[j];
				}
			}
			if (cell.IsValid)
			{
				SoundDefOf.RoofCollapse.PlayOneShot(new TargetInfo(cell, map, false));
			}
		}

		private static void DropRoofInCellPhaseOne(IntVec3 c, Map map)
		{
			RoofDef roofDef = map.roofGrid.RoofAt(c);
			if (roofDef == null)
			{
				return;
			}
			if (roofDef.collapseLeavingThingDef != null && roofDef.collapseLeavingThingDef.passability == Traversability.Impassable)
			{
				for (int i = 0; i < 2; i++)
				{
					List<Thing> thingList = c.GetThingList(map);
					for (int j = thingList.Count - 1; j >= 0; j--)
					{
						Thing thing = thingList[j];
						map.roofCollapseBuffer.Notify_Crushed(thing);
						Pawn pawn = thing as Pawn;
						DamageInfo dinfo;
						if (pawn != null)
						{
							DamageDef crush = DamageDefOf.Crush;
							int amount = 99999;
							BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
							dinfo = new DamageInfo(crush, amount, -1f, null, brain, null, DamageInfo.SourceCategory.Collapse);
						}
						else
						{
							dinfo = new DamageInfo(DamageDefOf.Crush, 99999, -1f, null, null, null, DamageInfo.SourceCategory.Collapse);
							dinfo.SetBodyRegion(BodyPartHeight.Top, BodyPartDepth.Outside);
						}
						BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
						if (i == 0 && pawn != null)
						{
							battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Ceiling, null);
							Find.BattleLog.Add(battleLogEntry_DamageTaken);
						}
						thing.TakeDamage(dinfo).InsertIntoLog(battleLogEntry_DamageTaken);
						if (!thing.Destroyed && thing.def.destroyable)
						{
							thing.Destroy(DestroyMode.Vanish);
						}
					}
				}
			}
			else
			{
				List<Thing> thingList2 = c.GetThingList(map);
				for (int k = thingList2.Count - 1; k >= 0; k--)
				{
					Thing thing2 = thingList2[k];
					if (thing2.def.category == ThingCategory.Item || thing2.def.category == ThingCategory.Plant || thing2.def.category == ThingCategory.Building || thing2.def.category == ThingCategory.Pawn)
					{
						map.roofCollapseBuffer.Notify_Crushed(thing2);
						float num = (float)RoofCollapserImmediate.ThinRoofCrushDamageRange.RandomInRange;
						if (thing2.def.building != null)
						{
							num *= thing2.def.building.roofCollapseDamageMultiplier;
						}
						BattleLogEntry_DamageTaken battleLogEntry_DamageTaken2 = null;
						if (thing2 is Pawn)
						{
							battleLogEntry_DamageTaken2 = new BattleLogEntry_DamageTaken(thing2 as Pawn, RulePackDefOf.DamageEvent_Ceiling, null);
							Find.BattleLog.Add(battleLogEntry_DamageTaken2);
						}
						DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Crush, GenMath.RoundRandom(num), -1f, null, null, null, DamageInfo.SourceCategory.Collapse);
						dinfo2.SetBodyRegion(BodyPartHeight.Top, BodyPartDepth.Outside);
						thing2.TakeDamage(dinfo2).InsertIntoLog(battleLogEntry_DamageTaken2);
					}
				}
			}
			if (roofDef.collapseLeavingThingDef != null)
			{
				Thing thing3 = GenSpawn.Spawn(roofDef.collapseLeavingThingDef, c, map);
				if (thing3.def.rotatable)
				{
					thing3.Rotation = Rot4.Random;
				}
			}
			for (int l = 0; l < 1; l++)
			{
				Vector3 vector = c.ToVector3Shifted();
				vector += Gen.RandomHorizontalVector(0.6f);
				MoteMaker.ThrowDustPuff(vector, map, 2f);
			}
		}

		private static void DropRoofInCellPhaseTwo(IntVec3 c, Map map)
		{
			RoofDef roofDef = map.roofGrid.RoofAt(c);
			if (roofDef == null)
			{
				return;
			}
			if (roofDef.filthLeaving != null)
			{
				FilthMaker.MakeFilth(c, map, roofDef.filthLeaving, 1);
			}
			if (roofDef.VanishOnCollapse)
			{
				map.roofGrid.SetRoof(c, null);
			}
			CellRect bound = CellRect.CenteredOn(c, 2);
			foreach (Pawn current in from pawn in map.mapPawns.AllPawnsSpawned
			where bound.Contains(pawn.Position)
			select pawn)
			{
				TaleRecorder.RecordTale(TaleDefOf.CollapseDodged, new object[]
				{
					current
				});
			}
		}
	}
}
