using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class RoofCollapserImmediate
	{
		private static readonly IntRange ThinRoofCrushDamageRange = new IntRange(15, 30);

		public static void DropRoofInCells(IntVec3 c)
		{
			RoofCollapserImmediate.DropRoofInCellPhaseOne(c);
			RoofCollapserImmediate.DropRoofInCellPhaseTwo(c);
			SoundDefOf.RoofCollapse.PlayOneShot(c);
		}

		public static void DropRoofInCells(IEnumerable<IntVec3> cells)
		{
			IntVec3 sourceLoc = IntVec3.Invalid;
			foreach (IntVec3 current in cells)
			{
				RoofCollapserImmediate.DropRoofInCellPhaseOne(current);
			}
			foreach (IntVec3 current2 in cells)
			{
				RoofCollapserImmediate.DropRoofInCellPhaseTwo(current2);
				sourceLoc = current2;
			}
			if (sourceLoc.IsValid)
			{
				SoundDefOf.RoofCollapse.PlayOneShot(sourceLoc);
			}
		}

		public static void DropRoofInCells(List<IntVec3> cells)
		{
			if (cells.NullOrEmpty<IntVec3>())
			{
				return;
			}
			for (int i = 0; i < cells.Count; i++)
			{
				RoofCollapserImmediate.DropRoofInCellPhaseOne(cells[i]);
			}
			for (int j = 0; j < cells.Count; j++)
			{
				RoofCollapserImmediate.DropRoofInCellPhaseTwo(cells[j]);
			}
			SoundDefOf.RoofCollapse.PlayOneShot(cells[0]);
		}

		private static void DropRoofInCellPhaseOne(IntVec3 c)
		{
			RoofDef roofDef = Find.RoofGrid.RoofAt(c);
			if (roofDef == null)
			{
				return;
			}
			if (roofDef.collapseLeavingThingDef != null && roofDef.collapseLeavingThingDef.passability == Traversability.Impassable)
			{
				for (int i = 0; i < 2; i++)
				{
					List<Thing> thingList = c.GetThingList();
					for (int j = thingList.Count - 1; j >= 0; j--)
					{
						Thing thing = thingList[j];
						RoofCollapseBuffer.Notify_Crushed(thing);
						Pawn pawn = thing as Pawn;
						BodyPartDamageInfo value;
						if (pawn != null)
						{
							value = new BodyPartDamageInfo(pawn.health.hediffSet.GetBrain(), false, null);
						}
						else
						{
							value = new BodyPartDamageInfo(new BodyPartHeight?(BodyPartHeight.Top), new BodyPartDepth?(BodyPartDepth.Outside));
						}
						DamageInfo dinfo = new DamageInfo(DamageDefOf.Crush, 99999, null, new BodyPartDamageInfo?(value), null);
						thing.TakeDamage(dinfo);
						if (!thing.Destroyed && thing.def.destroyable)
						{
							thing.Destroy(DestroyMode.Vanish);
						}
					}
				}
			}
			else
			{
				List<Thing> thingList2 = c.GetThingList();
				for (int k = thingList2.Count - 1; k >= 0; k--)
				{
					Thing thing2 = thingList2[k];
					if (thing2.def.category == ThingCategory.Item || thing2.def.category == ThingCategory.Plant || thing2.def.category == ThingCategory.Building || thing2.def.category == ThingCategory.Pawn)
					{
						RoofCollapseBuffer.Notify_Crushed(thing2);
						float num = (float)RoofCollapserImmediate.ThinRoofCrushDamageRange.RandomInRange;
						if (thing2.def.building != null)
						{
							num *= thing2.def.building.roofCollapseDamageMultiplier;
						}
						BodyPartDamageInfo value2 = new BodyPartDamageInfo(new BodyPartHeight?(BodyPartHeight.Top), new BodyPartDepth?(BodyPartDepth.Outside));
						DamageInfo dinfo2 = new DamageInfo(DamageDefOf.Crush, GenMath.RoundRandom(num), null, new BodyPartDamageInfo?(value2), null);
						thing2.TakeDamage(dinfo2);
					}
				}
			}
			if (roofDef.collapseLeavingThingDef != null)
			{
				Thing thing3 = GenSpawn.Spawn(roofDef.collapseLeavingThingDef, c);
				if (thing3.def.rotatable)
				{
					thing3.Rotation = Rot4.Random;
				}
			}
			for (int l = 0; l < 1; l++)
			{
				Vector3 vector = c.ToVector3Shifted();
				vector += Gen.RandomHorizontalVector(0.6f);
				MoteMaker.ThrowDustPuff(vector, 2f);
			}
		}

		private static void DropRoofInCellPhaseTwo(IntVec3 c)
		{
			RoofDef roofDef = Find.RoofGrid.RoofAt(c);
			if (roofDef == null)
			{
				return;
			}
			if (roofDef.filthLeaving != null)
			{
				FilthMaker.MakeFilth(c, roofDef.filthLeaving, 1);
			}
			if (!roofDef.isThickRoof)
			{
				Find.RoofGrid.SetRoof(c, null);
			}
		}
	}
}
