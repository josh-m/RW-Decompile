using System;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Features : WorldGenStep
	{
		public override void GenerateFresh(string seed)
		{
			Rand.Seed = GenText.StableStringHash(seed);
			Find.World.features = new WorldFeatures();
			IOrderedEnumerable<FeatureDef> orderedEnumerable = from x in DefDatabase<FeatureDef>.AllDefsListForReading
			orderby x.order, x.index
			select x;
			foreach (FeatureDef current in orderedEnumerable)
			{
				try
				{
					current.Worker.GenerateWhereAppropriate();
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Could not generate world features of def ",
						current,
						": ",
						ex
					}));
				}
			}
			Rand.RandomizeStateFromTime();
		}
	}
}
