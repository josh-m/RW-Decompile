using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Tornado : IncidentWorker
	{
		private const int MinDistanceFromMapEdge = 30;

		private const float MinWind = 1f;

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			return map.weatherManager.CurWindSpeedFactor >= 1f;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			CellRect cellRect = CellRect.WholeMap(map).ContractedBy(30);
			if (cellRect.IsEmpty)
			{
				cellRect = CellRect.WholeMap(map);
			}
			IntVec3 loc;
			if (!CellFinder.TryFindRandomCellInsideWith(cellRect, (IntVec3 x) => this.CanSpawnTornadoAt(x, map), out loc))
			{
				return false;
			}
			Tornado t = (Tornado)GenSpawn.Spawn(ThingDefOf.Tornado, loc, map);
			base.SendStandardLetter(t, new string[0]);
			return true;
		}

		private bool CanSpawnTornadoAt(IntVec3 c, Map map)
		{
			if (c.Fogged(map))
			{
				return false;
			}
			int num = GenRadial.NumCellsInRadius(7f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 c2 = c + GenRadial.RadialPattern[i];
				if (c2.InBounds(map))
				{
					if (this.AnyPawnOfPlayerFactionAt(c2, map))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool AnyPawnOfPlayerFactionAt(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null && pawn.Faction == Faction.OfPlayer)
				{
					return true;
				}
			}
			return false;
		}
	}
}
