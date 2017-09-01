using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RimWorld
{
	public class WorldObjectCompProperties
	{
		public Type compClass = typeof(WorldObjectComp);

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors(WorldObjectDef parentDef)
		{
			if (this.compClass == null)
			{
				yield return parentDef.defName + " has WorldObjectCompProperties with null compClass.";
			}
		}

		public virtual void ResolveReferences(WorldObjectDef parentDef)
		{
		}
	}
}
