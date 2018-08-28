using System;
using Verse;

namespace RimWorld
{
	public class GenStep_ScenParts : GenStep
	{
		public override int SeedPart
		{
			get
			{
				return 1561683158;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			Find.Scenario.GenerateIntoMap(map);
		}
	}
}
