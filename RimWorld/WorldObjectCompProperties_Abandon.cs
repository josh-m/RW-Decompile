using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RimWorld
{
	public class WorldObjectCompProperties_Abandon : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_Abandon()
		{
			this.compClass = typeof(AbandonComp);
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
				yield return parentDef.defName + " has WorldObjectCompProperties_Abandon but it's not MapParent.";
			}
		}
	}
}
