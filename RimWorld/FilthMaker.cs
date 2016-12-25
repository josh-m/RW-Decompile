using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class FilthMaker
	{
		public static void MakeFilth(IntVec3 c, Map map, ThingDef filthDef, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				FilthMaker.MakeFilth(c, map, filthDef, null, true);
			}
		}

		public static bool MakeFilth(IntVec3 c, Map map, ThingDef filthDef, string source, int count = 1)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				flag |= FilthMaker.MakeFilth(c, map, filthDef, Gen.YieldSingle<string>(source), true);
			}
			return flag;
		}

		public static void MakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources)
		{
			FilthMaker.MakeFilth(c, map, filthDef, sources, true);
		}

		private static bool MakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, bool shouldPropagate)
		{
			Filth filth = (Filth)(from t in c.GetThingList(map)
			where t.def == filthDef
			select t).FirstOrDefault<Thing>();
			if (!c.Walkable(map) || (filth != null && !filth.CanBeThickened))
			{
				if (shouldPropagate)
				{
					List<IntVec3> list = GenAdj.AdjacentCells8WayRandomized();
					for (int i = 0; i < 8; i++)
					{
						IntVec3 c2 = c + list[i];
						if (c2.InBounds(map))
						{
							if (FilthMaker.MakeFilth(c2, map, filthDef, sources, false))
							{
								return true;
							}
						}
					}
				}
				if (filth != null)
				{
					filth.AddSources(sources);
				}
				return false;
			}
			if (filth != null)
			{
				filth.ThickenFilth();
				filth.AddSources(sources);
			}
			else
			{
				Filth filth2 = (Filth)ThingMaker.MakeThing(filthDef, null);
				filth2.AddSources(sources);
				GenSpawn.Spawn(filth2, c, map);
			}
			return true;
		}
	}
}
