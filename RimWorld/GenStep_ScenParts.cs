using System;
using Verse;

namespace RimWorld
{
	public class GenStep_ScenParts : GenStep
	{
		public override void Generate()
		{
			Find.Scenario.GenerateIntoMap();
		}
	}
}
