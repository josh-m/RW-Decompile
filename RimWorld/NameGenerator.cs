using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public static class NameGenerator
	{
		public static string GenerateName(RulePackDef rootPack, IEnumerable<string> extantNames, bool appendNumberIfNameUsed = false)
		{
			return NameGenerator.GenerateName(rootPack, (string x) => !extantNames.Contains(x), appendNumberIfNameUsed);
		}

		public static string GenerateName(RulePackDef rootPack, Predicate<string> validator = null, bool appendNumberIfNameUsed = false)
		{
			string text = null;
			if (appendNumberIfNameUsed)
			{
				for (int i = 0; i < 100; i++)
				{
					for (int j = 0; j < 5; j++)
					{
						text = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(rootPack.Rules[0].keyword, rootPack.Rules, null));
						if (i != 0)
						{
							text = text + " " + (i + 1);
						}
						if (validator == null || validator(text))
						{
							return text;
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < 150; k++)
				{
					text = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(rootPack.Rules[0].keyword, rootPack.Rules, null));
					if (validator == null || validator(text))
					{
						return text;
					}
				}
			}
			Log.Error("Could not get new name.");
			return text;
		}
	}
}
