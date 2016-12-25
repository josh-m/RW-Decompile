using System;
using Verse;

namespace RimWorld
{
	public class GenStep_ScenParts : GenStep
	{
		public override void Generate(Map map)
		{
			Find.Scenario.GenerateIntoMap(map);
		}
	}
}
