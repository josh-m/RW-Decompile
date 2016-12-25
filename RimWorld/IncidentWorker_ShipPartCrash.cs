using System;
using Verse;

namespace RimWorld
{
	internal abstract class IncidentWorker_ShipPartCrash : IncidentWorker
	{
		private const float ShipPointsFactor = 0.9f;

		private const int IncidentMinimumPoints = 300;

		protected virtual int CountToSpawn
		{
			get
			{
				return 1;
			}
		}

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return map.listerThings.ThingsOfDef(this.def.shipPart).Count <= 0;
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int num = 0;
			int countToSpawn = this.CountToSpawn;
			IntVec3 cell = IntVec3.Invalid;
			for (int i = 0; i < countToSpawn; i++)
			{
				Predicate<IntVec3> validator = delegate(IntVec3 c)
				{
					if (c.Fogged(map))
					{
						return false;
					}
					foreach (IntVec3 current in GenAdj.CellsOccupiedBy(c, Rot4.North, this.def.shipPart.size))
					{
						if (!current.Standable(map))
						{
							bool result = false;
							return result;
						}
						if (map.roofGrid.Roofed(current))
						{
							bool result = false;
							return result;
						}
					}
					return map.reachability.CanReachColony(c);
				};
				IntVec3 intVec;
				if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(14, validator, map, out intVec))
				{
					break;
				}
				GenExplosion.DoExplosion(intVec, map, 3f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
				Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart)GenSpawn.Spawn(this.def.shipPart, intVec, map);
				building_CrashedShipPart.SetFaction(Faction.OfMechanoids, null);
				building_CrashedShipPart.pointsLeft = parms.points * 0.9f;
				if (building_CrashedShipPart.pointsLeft < 300f)
				{
					building_CrashedShipPart.pointsLeft = 300f;
				}
				num++;
				cell = intVec;
			}
			if (num > 0)
			{
				if (map == Find.VisibleMap)
				{
					Find.CameraDriver.shaker.DoShake(1f);
				}
				Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, new TargetInfo(cell, map, false), null);
			}
			return num > 0;
		}
	}
}
