using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class FactionBase_TraderTracker : Settlement_TraderTracker
	{
		public FactionBase FactionBase
		{
			get
			{
				return (FactionBase)this.settlement;
			}
		}

		public override TraderKindDef TraderKind
		{
			get
			{
				FactionBase factionBase = this.FactionBase;
				List<TraderKindDef> baseTraderKinds = factionBase.Faction.def.baseTraderKinds;
				if (baseTraderKinds.NullOrEmpty<TraderKindDef>())
				{
					return null;
				}
				int index = Mathf.Abs(factionBase.HashOffset()) % baseTraderKinds.Count;
				return baseTraderKinds[index];
			}
		}

		public FactionBase_TraderTracker(Settlement factionBase) : base(factionBase)
		{
		}
	}
}
