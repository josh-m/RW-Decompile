using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Components : WorldGenStep
	{
		public override void GenerateFresh(string seed)
		{
			Find.World.ConstructComponents();
		}

		public override void GenerateFromScribe(string seed)
		{
			Find.World.ConstructComponents();
			Find.World.ExposeComponents();
		}
	}
}
