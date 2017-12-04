using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class GenStep_DownedRefugee : GenStep_Scatterer
	{
		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			return base.CanScatterAt(c, map) && c.Standable(map);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, int count = 1)
		{
			DownedRefugeeComp component = map.info.parent.GetComponent<DownedRefugeeComp>();
			Pawn newThing;
			if (component != null && component.pawn.Any)
			{
				newThing = component.pawn.Take(component.pawn[0]);
			}
			else
			{
				newThing = DownedRefugeeQuestUtility.GenerateRefugee(map.Tile);
			}
			GenSpawn.Spawn(newThing, loc, map);
			MapGenerator.rootsToUnfog.Add(loc);
		}
	}
}
