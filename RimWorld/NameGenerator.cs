using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public static class NameGenerator
	{
		public static string GenerateName(RulePackDef rootPack, IEnumerable<string> extantNames, bool appendNumberIfNameUsed = false, string rootKeyword = null)
		{
			return NameGenerator.GenerateName(rootPack, (string x) => !extantNames.Contains(x), appendNumberIfNameUsed, rootKeyword, null);
		}

		public static string GenerateName(RulePackDef rootPack, Predicate<string> validator = null, bool appendNumberIfNameUsed = false, string rootKeyword = null, string testPawnNameSymbol = null)
		{
			GrammarRequest grammarRequest = default(GrammarRequest);
			grammarRequest.Includes.Add(rootPack);
			if (testPawnNameSymbol != null)
			{
				grammarRequest.Rules.Add(new Rule_String("ANYPAWN_nameDef", testPawnNameSymbol));
				grammarRequest.Rules.Add(new Rule_String("ANYPAWN_nameIndef", testPawnNameSymbol));
			}
			string text = (rootKeyword == null) ? rootPack.FirstRuleKeyword : rootKeyword;
			string text2 = (rootKeyword == null) ? rootPack.FirstUntranslatedRuleKeyword : rootKeyword;
			if (appendNumberIfNameUsed)
			{
				string text3;
				GrammarRequest request;
				string text4;
				for (int i = 0; i < 100; i++)
				{
					for (int j = 0; j < 5; j++)
					{
						text3 = text;
						request = grammarRequest;
						text4 = text2;
						string text5 = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(text3, request, null, false, text4));
						if (i != 0)
						{
							text5 = text5 + " " + (i + 1);
						}
						if (validator == null || validator(text5))
						{
							return text5;
						}
					}
				}
				text4 = text;
				request = grammarRequest;
				text3 = text2;
				return GenText.ToTitleCaseSmart(GrammarResolver.Resolve(text4, request, null, false, text3));
			}
			for (int k = 0; k < 150; k++)
			{
				string text3 = text;
				GrammarRequest request = grammarRequest;
				string text4 = text2;
				string text6 = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(text3, request, null, false, text4));
				if (validator == null || validator(text6))
				{
					return text6;
				}
			}
			Log.Error("Could not get new name (rule pack: " + rootPack + ")", false);
			return "Errorname";
		}
	}
}
