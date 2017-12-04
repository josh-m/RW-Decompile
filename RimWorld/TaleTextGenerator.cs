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
			Rand.PushState();
			Rand.Seed = seed;
			string rootKeyword = null;
			GrammarRequest request = default(GrammarRequest);
			request.Rules.AddRange(extraRules);
			if (purpose == TextGenerationPurpose.ArtDescription)
			{
				rootKeyword = "art_description_root";
				if (tale != null && Rand.Value > 0.2f)
				{
					request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_HasTale);
					request.Rules.AddRange(tale.GetTextGenerationRules());
				}
				else
				{
					request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_Taleless);
					request.Includes.Add(RulePackDefOf.TalelessImages);
				}
				request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);
			}
			else if (purpose == TextGenerationPurpose.ArtName)
			{
				rootKeyword = "art_name";
				if (tale != null)
				{
					request.Rules.AddRange(tale.GetTextGenerationRules());
				}
			}
			string result = GrammarResolver.Resolve(rootKeyword, request, (tale == null) ? "null_tale" : tale.def.defName, false);
			Rand.PopState();
			return result;
		}
	}
}
