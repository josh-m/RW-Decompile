using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Grower : WorkGiver_Scanner
	{
		protected static ThingDef wantedPlantDef;

		protected virtual bool ExtraRequirements(IPlantToGrowSettable settable)
		{
			return true;
		}

		[DebuggerHidden]
		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			Danger maxDanger = pawn.NormalMaxDanger();
			List<Building> bList = Find.ListerBuildings.allBuildingsColonist;
			for (int i = 0; i < bList.Count; i++)
			{
				Building_PlantGrower b = bList[i] as Building_PlantGrower;
				if (b != null)
				{
					if (this.ExtraRequirements(b))
					{
						if (!b.IsForbidden(pawn))
						{
							if (pawn.CanReach(b, PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn))
							{
								if (!b.IsBurning())
								{
									CellRect.CellRectIterator cri = b.OccupiedRect().GetIterator();
									while (!cri.Done())
									{
										yield return cri.Current;
										cri.MoveNext();
									}
									WorkGiver_Grower.wantedPlantDef = null;
								}
							}
						}
					}
				}
			}
			WorkGiver_Grower.wantedPlantDef = null;
			List<Zone> zonesList = Find.ZoneManager.AllZones;
			for (int j = 0; j < zonesList.Count; j++)
			{
				Zone_Growing growZone = zonesList[j] as Zone_Growing;
				if (growZone != null)
				{
					if (growZone.cells.Count == 0)
					{
						Log.ErrorOnce("Grow zone has 0 cells: " + growZone, -563487);
					}
					else if (this.ExtraRequirements(growZone))
					{
						if (!growZone.ContainsStaticFire)
						{
							if (pawn.CanReach(growZone.Cells[0], PathEndMode.OnCell, maxDanger, false, TraverseMode.ByPawn))
							{
								for (int k = 0; k < growZone.cells.Count; k++)
								{
									yield return growZone.cells[k];
								}
								WorkGiver_Grower.wantedPlantDef = null;
							}
						}
					}
				}
			}
			WorkGiver_Grower.wantedPlantDef = null;
		}

		protected void DetermineWantedPlantDef(IntVec3 c)
		{
			IPlantToGrowSettable plantToGrowSettable = c.GetEdifice() as IPlantToGrowSettable;
			if (plantToGrowSettable == null)
			{
				plantToGrowSettable = (Find.ZoneManager.ZoneAt(c) as IPlantToGrowSettable);
			}
			if (plantToGrowSettable == null)
			{
				WorkGiver_Grower.wantedPlantDef = null;
			}
			else
			{
				WorkGiver_Grower.wantedPlantDef = plantToGrowSettable.GetPlantDefToGrow();
			}
		}
	}
}
