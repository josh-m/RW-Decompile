using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Settlement_TraderTracker : SettlementBase_TraderTracker
	{
		public Settlement Settlement
		{
			get
			{
				return (Settlement)this.settlement;
			}
		}

		public override TraderKindDef TraderKind
		{
			get
			{
				Settlement settlement = this.Settlement;
				List<TraderKindDef> baseTraderKinds = settlement.Faction.def.baseTraderKinds;
				if (baseTraderKinds.NullOrEmpty<TraderKindDef>())
				{
					return null;
				}
				int index = Mathf.Abs(settlement.HashOffset()) % baseTraderKinds.Count;
				return baseTraderKinds[index];
			}
		}

		public Settlement_TraderTracker(SettlementBase settlement) : base(settlement)
		{
		}
	}
}
