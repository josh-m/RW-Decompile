using System;

namespace Verse
{
	public static class TrainableIntelligenceExtension
	{
		public static string GetLabel(this TrainableIntelligence ti)
		{
			return ("TrainableIntelligence_" + ti.ToString()).Translate();
		}
	}
}
