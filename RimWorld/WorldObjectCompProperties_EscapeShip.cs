using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RimWorld
{
	public class WorldObjectCompProperties_EscapeShip : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_EscapeShip()
		{
			this.compClass = typeof(EscapeShipComp);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
		{
			foreach (string e in base.ConfigErrors(parentDef))
			{
				yield return e;
			}
			if (!typeof(MapParent).IsAssignableFrom(parentDef.worldObjectClass))
			{
				yield return parentDef.defName + " has WorldObjectCompProperties_EscapeShip but it's not MapParent.";
			}
		}
	}
}
