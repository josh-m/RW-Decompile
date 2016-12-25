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

		protected override bool CanFireNowSub()
		{
			return Find.ListerThings.ThingsOfDef(this.def.shipPart).Count <= 0;
		}

		public override bool TryExecute(IncidentParms parms)
		{
			int num = 0;
			int countToSpawn = this.CountToSpawn;
			IntVec3 vec = IntVec3.Invalid;
			for (int i = 0; i < countToSpawn; i++)
			{
				Predicate<IntVec3> validator = delegate(IntVec3 c)
				{
					if (c.Fogged())
					{
						return false;
					}
					foreach (IntVec3 current in GenAdj.CellsOccupiedBy(c, Rot4.North, this.def.shipPart.size))
					{
						if (!current.Standable())
						{
							bool result = false;
							return result;
						}
						if (Find.RoofGrid.Roofed(current))
						{
							bool result = false;
							return result;
						}
					}
					return c.CanReachColony();
				};
				IntVec3 intVec;
				if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(14, validator, out intVec))
				{
					break;
				}
				GenExplosion.DoExplosion(intVec, 3f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
				Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart)GenSpawn.Spawn(this.def.shipPart, intVec);
				building_CrashedShipPart.SetFaction(Faction.OfMechanoids, null);
				building_CrashedShipPart.pointsLeft = parms.points * 0.9f;
				if (building_CrashedShipPart.pointsLeft < 300f)
				{
					building_CrashedShipPart.pointsLeft = 300f;
				}
				num++;
				vec = intVec;
			}
			if (num > 0)
			{
				Find.CameraDriver.shaker.DoShake(1f);
				Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, vec, null);
			}
			return num > 0;
		}
	}
}
