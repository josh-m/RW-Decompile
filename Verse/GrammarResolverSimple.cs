using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class GrammarResolverSimple
	{
		private static bool working;

		private static StringBuilder tmpResultBuffer = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer_objectLabel = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer_subSymbol = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer_args = new StringBuilder();

		private static List<string> tmpArgsLabels = new List<string>();

		private static List<object> tmpArgsObjects = new List<object>();

		private static StringBuilder tmpArg = new StringBuilder();

		public static string Formatted(string str, List<string> argsLabelsArg, List<object> argsObjectsArg)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			bool flag;
			StringBuilder stringBuilder;
			StringBuilder stringBuilder2;
			StringBuilder stringBuilder3;
			StringBuilder stringBuilder4;
			StringBuilder stringBuilder5;
			List<string> list;
			List<object> list2;
			if (GrammarResolverSimple.working)
			{
				flag = false;
				stringBuilder = new StringBuilder();
				stringBuilder2 = new StringBuilder();
				stringBuilder3 = new StringBuilder();
				stringBuilder4 = new StringBuilder();
				stringBuilder5 = new StringBuilder();
				list = argsLabelsArg.ToList<string>();
				list2 = argsObjectsArg.ToList<object>();
			}
			else
			{
				flag = true;
				stringBuilder = GrammarResolverSimple.tmpResultBuffer;
				stringBuilder2 = GrammarResolverSimple.tmpSymbolBuffer;
				stringBuilder3 = GrammarResolverSimple.tmpSymbolBuffer_objectLabel;
				stringBuilder4 = GrammarResolverSimple.tmpSymbolBuffer_subSymbol;
				stringBuilder5 = GrammarResolverSimple.tmpSymbolBuffer_args;
				list = GrammarResolverSimple.tmpArgsLabels;
				list.Clear();
				list.AddRange(argsLabelsArg);
				list2 = GrammarResolverSimple.tmpArgsObjects;
				list2.Clear();
				list2.AddRange(argsObjectsArg);
			}
			if (flag)
			{
				GrammarResolverSimple.working = true;
			}
			string result;
			try
			{
				stringBuilder.Length = 0;
				for (int i = 0; i < str.Length; i++)
				{
					char c = str[i];
					if (c == '{')
					{
						stringBuilder2.Length = 0;
						stringBuilder3.Length = 0;
						stringBuilder4.Length = 0;
						stringBuilder5.Length = 0;
						bool flag2 = false;
						bool flag3 = false;
						bool flag4 = false;
						i++;
						bool flag5 = i < str.Length && str[i] == '{';
						while (i < str.Length)
						{
							char c2 = str[i];
							if (c2 == '}')
							{
								flag2 = true;
								break;
							}
							stringBuilder2.Append(c2);
							if (c2 == '_' && !flag3)
							{
								flag3 = true;
							}
							else if (c2 == '?' && !flag4)
							{
								flag4 = true;
							}
							else if (flag4)
							{
								stringBuilder5.Append(c2);
							}
							else if (flag3)
							{
								stringBuilder4.Append(c2);
							}
							else
							{
								stringBuilder3.Append(c2);
							}
							i++;
						}
						if (!flag2)
						{
							Log.ErrorOnce("Could not find matching '}' in \"" + str + "\".", str.GetHashCode() ^ 194857261, false);
						}
						else if (flag5)
						{
							stringBuilder.Append(stringBuilder2);
						}
						else
						{
							if (flag4)
							{
								while (stringBuilder4.Length != 0 && stringBuilder4[stringBuilder4.Length - 1] == ' ')
								{
									stringBuilder4.Length--;
								}
							}
							string text = stringBuilder3.ToString();
							bool flag6 = false;
							int num = -1;
							if (int.TryParse(text, out num))
							{
								string value;
								if (num >= 0 && num < list2.Count && GrammarResolverSimple.TryResolveSymbol(list2[num], stringBuilder4.ToString(), stringBuilder5.ToString(), out value, str))
								{
									flag6 = true;
									stringBuilder.Append(value);
								}
							}
							else
							{
								for (int j = 0; j < list.Count; j++)
								{
									if (list[j] == text)
									{
										string value2;
										if (GrammarResolverSimple.TryResolveSymbol(list2[j], stringBuilder4.ToString(), stringBuilder5.ToString(), out value2, str))
										{
											flag6 = true;
											stringBuilder.Append(value2);
										}
										break;
									}
								}
							}
							if (!flag6)
							{
								Log.ErrorOnce(string.Concat(new object[]
								{
									"Could not resolve symbol \"",
									stringBuilder2,
									"\" for string \"",
									str,
									"\"."
								}), str.GetHashCode() ^ stringBuilder2.ToString().GetHashCode() ^ 879654654, false);
							}
						}
					}
					else
					{
						stringBuilder.Append(c);
					}
				}
				string text2 = GenText.CapitalizeSentences(stringBuilder.ToString(), str[0] == '{');
				text2 = Find.ActiveLanguageWorker.PostProcessedKeyedTranslation(text2);
				result = text2;
			}
			finally
			{
				if (flag)
				{
					GrammarResolverSimple.working = false;
				}
			}
			return result;
		}

		private static bool TryResolveSymbol(object obj, string subSymbol, string symbolArgs, out string resolvedStr, string fullStringForReference)
		{
			Pawn pawn = obj as Pawn;
			if (pawn != null)
			{
				switch (subSymbol)
				{

					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "nameFull":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringFull, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "nameFullDef":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelDefinite() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringFull, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "label":
					resolvedStr = pawn.Label;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelShort":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabel : pawn.Name.ToStringShort);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "definite":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelDefinite() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "nameDef":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelDefinite() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "indefinite":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "nameIndef":
					resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, false, true));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pronoun":
					resolvedStr = pawn.gender.GetPronoun();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "possessive":
					resolvedStr = pawn.gender.GetPossessive();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "objective":
					resolvedStr = pawn.gender.GetObjective();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionName":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : pawn.Faction.Name);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionPawnSingular":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : pawn.Faction.def.pawnSingular);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionPawnSingularDef":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Faction.def.pawnSingular, false, false));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionPawnSingularIndef":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Faction.def.pawnSingular, false, false));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionPawnsPlural":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : pawn.Faction.def.pawnsPlural);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionPawnsPluralDef":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(pawn.Faction.def.pawnsPlural, pawn.Faction.def.pawnSingular), true, false));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionPawnsPluralIndef":
					resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(pawn.Faction.def.pawnsPlural, pawn.Faction.def.pawnSingular), true, false));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kind":
					resolvedStr = pawn.KindLabel;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindDef":
					resolvedStr = pawn.KindLabelDefinite();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindIndef":
					resolvedStr = pawn.KindLabelIndefinite();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindPlural":
					resolvedStr = pawn.GetKindLabelPlural(-1);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindPluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.GetKindLabelPlural(-1), pawn.gender, true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindPluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.GetKindLabelPlural(-1), pawn.gender, true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindBase":
					resolvedStr = pawn.kindDef.label;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindBaseDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.kindDef.label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindBaseIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.kindDef.label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindBasePlural":
					resolvedStr = pawn.kindDef.GetLabelPlural(-1);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindBasePluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.kindDef.GetLabelPlural(-1), LanguageDatabase.activeLanguage.ResolveGender(pawn.kindDef.GetLabelPlural(-1), pawn.kindDef.label), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "kindBasePluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.kindDef.GetLabelPlural(-1), LanguageDatabase.activeLanguage.ResolveGender(pawn.kindDef.GetLabelPlural(-1), pawn.kindDef.label), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "lifeStage":
					resolvedStr = pawn.ageTracker.CurLifeStage.label;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "lifeStageDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.ageTracker.CurLifeStage.label, pawn.gender, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "lifeStageIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.ageTracker.CurLifeStage.label, pawn.gender, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "lifeStageAdjective":
					resolvedStr = pawn.ageTracker.CurLifeStage.Adjective;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "title":
					resolvedStr = ((pawn.story == null) ? string.Empty : pawn.story.Title);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "titleDef":
					resolvedStr = ((pawn.story == null) ? string.Empty : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.story.Title, false, false));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "titleIndef":
					resolvedStr = ((pawn.story == null) ? string.Empty : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.story.Title, false, false));
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "gender":
					resolvedStr = GrammarResolverSimple.ResolveGenderSymbol(pawn.gender, pawn.RaceProps.Animal, symbolArgs, fullStringForReference);
					return true;
				case "humanlike":
					resolvedStr = GrammarResolverSimple.ResolveHumanlikeSymbol(pawn.RaceProps.Humanlike, symbolArgs, fullStringForReference);
					return true;
				}
				resolvedStr = string.Empty;
				return false;
			}
			Thing thing = obj as Thing;
			if (thing != null)
			{
				switch (subSymbol)
				{

					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.Label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "label":
					resolvedStr = thing.Label;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPlural":
					resolvedStr = Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount, -1);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount, -1), thing.LabelNoCount), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount, -1), thing.LabelNoCount), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelShort":
					resolvedStr = thing.LabelShort;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "definite":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(thing.Label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "indefinite":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.Label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pronoun":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount, null).GetPronoun();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "possessive":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount, null).GetPossessive();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "objective":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount, null).GetObjective();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionName":
					resolvedStr = ((thing.Faction == null) ? string.Empty : thing.Faction.Name);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "gender":
					resolvedStr = GrammarResolverSimple.ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount, null), false, symbolArgs, fullStringForReference);
					return true;
				}
				resolvedStr = string.Empty;
				return false;
			}
			WorldObject worldObject = obj as WorldObject;
			if (worldObject != null)
			{
				switch (subSymbol)
				{
				{
					LanguageWorker arg_E65_0 = Find.ActiveLanguageWorker;
					string label = worldObject.Label;
					bool hasName = worldObject.HasName;
					resolvedStr = arg_E65_0.WithIndefiniteArticle(label, false, hasName);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				}
				case "label":
					resolvedStr = worldObject.Label;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPlural":
					resolvedStr = Find.ActiveLanguageWorker.Pluralize(worldObject.Label, -1);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(worldObject.Label, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(worldObject.Label, -1), worldObject.Label), true, worldObject.HasName);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(worldObject.Label, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(worldObject.Label, -1), worldObject.Label), true, worldObject.HasName);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "definite":
				{
					LanguageWorker arg_F6E_0 = Find.ActiveLanguageWorker;
					string label = worldObject.Label;
					bool hasName = worldObject.HasName;
					resolvedStr = arg_F6E_0.WithDefiniteArticle(label, false, hasName);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				}
				case "indefinite":
				{
					LanguageWorker arg_F9C_0 = Find.ActiveLanguageWorker;
					string label = worldObject.Label;
					bool hasName = worldObject.HasName;
					resolvedStr = arg_F9C_0.WithIndefiniteArticle(label, false, hasName);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				}
				case "pronoun":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label, null).GetPronoun();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "possessive":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label, null).GetPossessive();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "objective":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label, null).GetObjective();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "factionName":
					resolvedStr = ((worldObject.Faction == null) ? string.Empty : worldObject.Faction.Name);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "gender":
					resolvedStr = GrammarResolverSimple.ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label, null), false, symbolArgs, fullStringForReference);
					return true;
				}
				resolvedStr = string.Empty;
				return false;
			}
			Faction faction = obj as Faction;
			if (faction != null)
			{
				switch (subSymbol)
				{

					resolvedStr = faction.Name;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "name":
					resolvedStr = faction.Name;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pawnSingular":
					resolvedStr = faction.def.pawnSingular;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pawnSingularDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnSingular, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pawnSingularIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnSingular, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pawnsPlural":
					resolvedStr = faction.def.pawnsPlural;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pawnsPluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pawnsPluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				}
				resolvedStr = string.Empty;
				return false;
			}
			Def def = obj as Def;
			if (def != null)
			{
				PawnKindDef pawnKindDef = def as PawnKindDef;
				if (pawnKindDef != null && subSymbol != null)
				{
					if (subSymbol == "labelPlural")
					{
						resolvedStr = pawnKindDef.GetLabelPlural(-1);
						GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					if (subSymbol == "labelPluralDef")
					{
						resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawnKindDef.GetLabelPlural(-1), LanguageDatabase.activeLanguage.ResolveGender(pawnKindDef.GetLabelPlural(-1), pawnKindDef.label), true, false);
						GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					if (subSymbol == "labelPluralIndef")
					{
						resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawnKindDef.GetLabelPlural(-1), LanguageDatabase.activeLanguage.ResolveGender(pawnKindDef.GetLabelPlural(-1), pawnKindDef.label), true, false);
						GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
				}
				switch (subSymbol)
				{

					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "label":
					resolvedStr = def.label;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPlural":
					resolvedStr = Find.ActiveLanguageWorker.Pluralize(def.label, -1);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(def.label, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(def.label, -1), def.label), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(def.label, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(def.label, -1), def.label), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "definite":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(def.label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "indefinite":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pronoun":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label, null).GetPronoun();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "possessive":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label, null).GetPossessive();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "objective":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label, null).GetObjective();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "gender":
					resolvedStr = GrammarResolverSimple.ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(def.label, null), false, symbolArgs, fullStringForReference);
					return true;
				}
				resolvedStr = string.Empty;
				return false;
			}
			string text = obj as string;
			if (text != null)
			{
				switch (subSymbol)
				{

					resolvedStr = text;
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "plural":
					resolvedStr = Find.ActiveLanguageWorker.Pluralize(text, -1);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(text, -1), text), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text, -1), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(text, -1), text), true, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "definite":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(text, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "indefinite":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(text, false, false);
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "pronoun":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text, null).GetPronoun();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "possessive":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text, null).GetPossessive();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "objective":
					resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text, null).GetObjective();
					GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "gender":
					resolvedStr = GrammarResolverSimple.ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(text, null), false, symbolArgs, fullStringForReference);
					return true;
				}
				resolvedStr = string.Empty;
				return false;
			}
			if (obj is int || obj is long)
			{
				int number = (!(obj is int)) ? ((int)((long)obj)) : ((int)obj);
				if (subSymbol != null)
				{
					if (subSymbol == string.Empty)
					{
						resolvedStr = number.ToString();
						GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					if (subSymbol == "ordinal")
					{
						resolvedStr = Find.ActiveLanguageWorker.OrdinalNumber(number, Gender.None).ToString();
						GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			if (subSymbol.NullOrEmpty())
			{
				GrammarResolverSimple.EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				if (obj == null)
				{
					resolvedStr = string.Empty;
				}
				else
				{
					resolvedStr = obj.ToString();
				}
				return true;
			}
			resolvedStr = string.Empty;
			return false;
		}

		private static void EnsureNoArgs(string subSymbol, string symbolArgs, string fullStringForReference)
		{
			if (!symbolArgs.NullOrEmpty())
			{
				Log.ErrorOnce(string.Concat(new string[]
				{
					"Symbol \"",
					subSymbol,
					"\" doesn't expect any args but \"",
					symbolArgs,
					"\" args were provided. Full string: \"",
					fullStringForReference,
					"\"."
				}), subSymbol.GetHashCode() ^ symbolArgs.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 958090126, false);
			}
		}

		private static string ResolveGenderSymbol(Gender gender, bool animal, string args, string fullStringForReference)
		{
			if (args.NullOrEmpty())
			{
				return gender.GetLabel(animal);
			}
			int argsCount = GrammarResolverSimple.GetArgsCount(args);
			if (argsCount == 2)
			{
				switch (gender)
				{
				case Gender.None:
					return GrammarResolverSimple.GetArg(args, 0);
				case Gender.Male:
					return GrammarResolverSimple.GetArg(args, 0);
				case Gender.Female:
					return GrammarResolverSimple.GetArg(args, 1);
				default:
					return string.Empty;
				}
			}
			else
			{
				if (argsCount != 3)
				{
					Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"gender\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 787618371, false);
					return string.Empty;
				}
				switch (gender)
				{
				case Gender.None:
					return GrammarResolverSimple.GetArg(args, 2);
				case Gender.Male:
					return GrammarResolverSimple.GetArg(args, 0);
				case Gender.Female:
					return GrammarResolverSimple.GetArg(args, 1);
				default:
					return string.Empty;
				}
			}
		}

		private static string ResolveHumanlikeSymbol(bool humanlike, string args, string fullStringForReference)
		{
			int argsCount = GrammarResolverSimple.GetArgsCount(args);
			if (argsCount != 2)
			{
				Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"humanlike\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 895109845, false);
				return string.Empty;
			}
			if (humanlike)
			{
				return GrammarResolverSimple.GetArg(args, 0);
			}
			return GrammarResolverSimple.GetArg(args, 1);
		}

		private static int GetArgsCount(string args)
		{
			int num = 1;
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == ':')
				{
					num++;
				}
			}
			return num;
		}

		private static string GetArg(string args, int argIndex)
		{
			GrammarResolverSimple.tmpArg.Length = 0;
			int num = 0;
			for (int i = 0; i < args.Length; i++)
			{
				char c = args[i];
				if (c == ':')
				{
					num++;
				}
				else if (num == argIndex)
				{
					GrammarResolverSimple.tmpArg.Append(c);
				}
				else if (num > argIndex)
				{
					break;
				}
			}
			while (GrammarResolverSimple.tmpArg.Length != 0 && GrammarResolverSimple.tmpArg[0] == ' ')
			{
				GrammarResolverSimple.tmpArg.Remove(0, 1);
			}
			while (GrammarResolverSimple.tmpArg.Length != 0 && GrammarResolverSimple.tmpArg[GrammarResolverSimple.tmpArg.Length - 1] == ' ')
			{
				GrammarResolverSimple.tmpArg.Length--;
			}
			return GrammarResolverSimple.tmpArg.ToString();
		}
	}
}
