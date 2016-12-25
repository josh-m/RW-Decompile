using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_MakeMapCondition : IncidentWorker
	{
		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			if (map.mapConditionManager.ConditionIsActive(this.def.mapCondition))
			{
				return false;
			}
			List<MapCondition> activeConditions = map.mapConditionManager.ActiveConditions;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				if (!this.def.mapCondition.CanCoexistWith(activeConditions[i].def))
				{
					return false;
				}
			}
			return true;
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
			MapCondition cond = MapConditionMaker.MakeCondition(this.def.mapCondition, duration, 0);
			map.mapConditionManager.RegisterCondition(cond);
			base.SendStandardLetter();
			return true;
		}
	}
}
