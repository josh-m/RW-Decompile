using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_GrowerSow : WorkGiver_Grower
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		protected override bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
		{
			if (!settable.CanAcceptSowNow())
			{
				return false;
			}
			Zone_Growing zone_Growing = settable as Zone_Growing;
			IntVec3 c;
			if (zone_Growing != null)
			{
				if (!zone_Growing.allowSow)
				{
					return false;
				}
				c = zone_Growing.Cells[0];
			}
			else
			{
				c = ((Thing)settable).Position;
			}
			WorkGiver_Grower.wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
			return WorkGiver_Grower.wantedPlantDef != null;
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c)
		{
			if (c.IsForbidden(pawn))
			{
				return null;
			}
			if (!GenPlant.GrowthSeasonNow(c, pawn.Map))
			{
				return null;
			}
			if (WorkGiver_Grower.wantedPlantDef == null)
			{
				WorkGiver_Grower.wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
				if (WorkGiver_Grower.wantedPlantDef == null)
				{
					return null;
				}
			}
			List<Thing> thingList = c.GetThingList(pawn.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def == WorkGiver_Grower.wantedPlantDef)
				{
					return null;
				}
				if ((thing is Blueprint || thing is Frame) && thing.Faction == pawn.Faction)
				{
					return null;
				}
			}
			Plant plant = c.GetPlant(pawn.Map);
			if (plant != null && plant.def.plant.blockAdjacentSow)
			{
				if (!pawn.CanReserve(plant, 1) || plant.IsForbidden(pawn))
				{
					return null;
				}
				return new Job(JobDefOf.CutPlant, plant);
			}
			else
			{
				Thing thing2 = GenPlant.AdjacentSowBlocker(WorkGiver_Grower.wantedPlantDef, c, pawn.Map);
				if (thing2 != null)
				{
					Plant plant2 = thing2 as Plant;
					if (plant2 != null && pawn.CanReserve(plant2, 1) && !plant2.IsForbidden(pawn))
					{
						Zone_Growing zone_Growing = pawn.Map.zoneManager.ZoneAt(plant2.Position) as Zone_Growing;
						if (zone_Growing == null || zone_Growing.GetPlantDefToGrow() != plant2.def)
						{
							return new Job(JobDefOf.CutPlant, plant2);
						}
					}
					return null;
				}
				if (WorkGiver_Grower.wantedPlantDef.plant.sowMinSkill > 0 && pawn.skills != null && pawn.skills.GetSkill(SkillDefOf.Growing).Level < WorkGiver_Grower.wantedPlantDef.plant.sowMinSkill)
				{
					return null;
				}
				int j = 0;
				while (j < thingList.Count)
				{
					Thing thing3 = thingList[j];
					if (thing3.def.BlockPlanting)
					{
						if (!pawn.CanReserve(thing3, 1))
						{
							return null;
						}
						if (thing3.def.category == ThingCategory.Plant)
						{
							if (!thing3.IsForbidden(pawn))
							{
								return new Job(JobDefOf.CutPlant, thing3);
							}
							return null;
						}
						else
						{
							if (thing3.def.EverHaulable)
							{
								return HaulAIUtility.HaulAsideJobFor(pawn, thing3);
							}
							return null;
						}
					}
					else
					{
						j++;
					}
				}
				if (!WorkGiver_Grower.wantedPlantDef.CanEverPlantAt(c, pawn.Map) || !GenPlant.GrowthSeasonNow(c, pawn.Map) || !pawn.CanReserve(c, 1))
				{
					return null;
				}
				return new Job(JobDefOf.Sow, c)
				{
					plantDefToSow = WorkGiver_Grower.wantedPlantDef
				};
			}
		}
	}
}
