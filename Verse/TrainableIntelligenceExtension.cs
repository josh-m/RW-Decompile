using System;

namespace Verse
{
	public static class TrainableIntelligenceExtension
	{
		public static string GetLabel(this TrainableIntelligenceDef ti)
		{
			return ("TrainableIntelligence_" + ti.defName).Translate();
		}
	}
}
