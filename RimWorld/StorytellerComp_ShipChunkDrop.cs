using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ShipChunkDrop : StorytellerComp
	{
		private const float BaseShipChunkDropMTBDays = 20f;

		private float ShipChunkDropMTBDays
		{
			get
			{
				float num = (float)Find.TickManager.TicksGame / 3600000f;
				if (num > 10f)
				{
					num = 2.75f;
				}
				return 20f * Mathf.Pow(2f, num);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(this.ShipChunkDropMTBDays, 60000f, 1000f))
			{
				IncidentDef def = IncidentDefOf.ShipChunkDrop;
				if (def.TargetAllowed(target))
				{
					yield return new FiringIncident(def, this, this.GenerateParms(def.category, target));
				}
			}
		}
	}
}
