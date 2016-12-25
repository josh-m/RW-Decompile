using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public static class TaleTextGenerator
	{
		private const float TalelessChanceWithTales = 0.2f;

		public static string GenerateTextFromTale(TextGenerationPurpose purpose, Tale tale, int seed, List<Rule> extraRules)
		{
			Rand.PushSeed();
			Rand.Seed = seed;
			string rootKeyword = null;
			List<Rule> list = new List<Rule>();
			list.AddRange(extraRules);
			if (purpose == TextGenerationPurpose.ArtDescription)
			{
				rootKeyword = "art_description_root";
				if (tale != null && Rand.Value > 0.2f)
				{
					list.AddRange(RulePackDefOf.ArtDescriptionRoot_HasTale.Rules);
					list.AddRange(tale.GetTextGenerationRules());
				}
				else
				{
					list.AddRange(RulePackDefOf.ArtDescriptionRoot_Taleless.Rules);
					list.AddRange(RulePackDefOf.TalelessImages.Rules);
				}
				list.AddRange(RulePackDefOf.ArtDescriptionUtility_Global.Rules);
			}
			else if (purpose == TextGenerationPurpose.ArtName)
			{
				rootKeyword = "art_name";
				if (tale != null)
				{
					list.AddRange(tale.GetTextGenerationRules());
				}
			}
			string result = GrammarResolver.Resolve(rootKeyword, list, (tale == null) ? "null_tale" : tale.def.defName);
			Rand.PopSeed();
			return result;
		}
	}
}
