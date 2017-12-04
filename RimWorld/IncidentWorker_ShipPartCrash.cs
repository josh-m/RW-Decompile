using System;
using UnityEngine;
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

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int num = 0;
			int countToSpawn = this.CountToSpawn;
			IntVec3 cell = IntVec3.Invalid;
			float shrapnelDirection = Rand.Range(0f, 360f);
			for (int i = 0; i < countToSpawn; i++)
			{
				IntVec3 intVec;
				if (!CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.CrashedShipPartIncoming, map, out intVec, 14, default(IntVec3), -1, false, true, true, true, null))
				{
					break;
				}
				Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart)ThingMaker.MakeThing(this.def.shipPart, null);
				building_CrashedShipPart.SetFaction(Faction.OfMechanoids, null);
				building_CrashedShipPart.GetComp<CompSpawnerMechanoidsOnDamaged>().pointsLeft = Mathf.Max(parms.points * 0.9f, 300f);
				Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, building_CrashedShipPart);
				skyfaller.shrapnelDirection = shrapnelDirection;
				GenSpawn.Spawn(skyfaller, intVec, map);
				num++;
				cell = intVec;
			}
			if (num > 0)
			{
				base.SendStandardLetter(new TargetInfo(cell, map, false), new string[0]);
			}
			return num > 0;
		}
	}
}
