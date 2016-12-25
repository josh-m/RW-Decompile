using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_MakeMapCondition : IncidentWorker
	{
		protected override bool CanFireNowSub()
		{
			if (Find.MapConditionManager.ConditionIsActive(this.def.mapCondition))
			{
				return false;
			}
			List<MapCondition> activeConditions = Find.MapConditionManager.ActiveConditions;
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
			int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
			MapCondition cond = MapConditionMaker.MakeCondition(this.def.mapCondition, duration, 0);
			Find.MapConditionManager.RegisterCondition(cond);
			base.SendStandardLetter();
			return true;
		}
	}
}
