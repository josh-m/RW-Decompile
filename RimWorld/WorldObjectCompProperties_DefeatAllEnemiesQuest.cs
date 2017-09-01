using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RimWorld
{
	public class WorldObjectCompProperties_DefeatAllEnemiesQuest : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_DefeatAllEnemiesQuest()
		{
			this.compClass = typeof(DefeatAllEnemiesQuestComp);
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
				yield return parentDef.defName + " has WorldObjectCompProperties_DefeatAllEnemiesQuest but it's not MapParent.";
			}
		}
	}
}
