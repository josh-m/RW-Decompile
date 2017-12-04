using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld.Planet
{
	public class AbandonComp : WorldObjectComp
	{
		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			MapParent mapParent = this.parent as MapParent;
			if (mapParent.HasMap && mapParent.Faction == Faction.OfPlayer)
			{
				yield return SettlementAbandonUtility.AbandonCommand(mapParent);
			}
		}
	}
}
