using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Verse
{
	public static class DefInjectionUtility
	{
		public delegate void PossibleDefInjectionTraverser(string suggestedPath, string normalizedPath, bool isCollection, string currentValue, IEnumerable<string> currentValueCollection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def);

		public static void ForEachPossibleDefInjection(Type defType, DefInjectionUtility.PossibleDefInjectionTraverser action)
		{
			IEnumerable<Def> allDefsInDatabaseForDef = GenDefDatabase.GetAllDefsInDatabaseForDef(defType);
			foreach (Def current in allDefsInDatabaseForDef)
			{
				DefInjectionUtility.ForEachPossibleDefInjectionInDef(current, action);
			}
		}

		private static void ForEachPossibleDefInjectionInDef(Def def, DefInjectionUtility.PossibleDefInjectionTraverser action)
		{
			HashSet<object> visited = new HashSet<object>();
			DefInjectionUtility.ForEachPossibleDefInjectionInDefRecursive(def, def.defName, def.defName, visited, true, def, action);
		}

		private static void ForEachPossibleDefInjectionInDefRecursive(object obj, string curNormalizedPath, string curSuggestedPath, HashSet<object> visited, bool translationAllowed, Def def, DefInjectionUtility.PossibleDefInjectionTraverser action)
		{
			if (obj == null)
			{
				return;
			}
			if (visited.Contains(obj))
			{
				return;
			}
			visited.Add(obj);
			foreach (FieldInfo current in DefInjectionUtility.FieldsInDeterministicOrder(obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))
			{
				object value = current.GetValue(obj);
				bool flag = translationAllowed && !current.HasAttribute<NoTranslateAttribute>() && !current.HasAttribute<UnsavedAttribute>();
				if (!(value is Def))
				{
					if (typeof(string).IsAssignableFrom(current.FieldType))
					{
						string currentValue = (string)value;
						string normalizedPath = curNormalizedPath + "." + current.Name;
						string suggestedPath = curSuggestedPath + "." + current.Name;
						action(suggestedPath, normalizedPath, false, currentValue, null, flag, false, current, def);
					}
					else if (value is IEnumerable<string>)
					{
						IEnumerable<string> currentValueCollection = (IEnumerable<string>)value;
						bool flag2 = current.HasAttribute<TranslationCanChangeCountAttribute>();
						string normalizedPath2 = curNormalizedPath + "." + current.Name;
						string suggestedPath2 = curSuggestedPath + "." + current.Name;
						action(suggestedPath2, normalizedPath2, true, null, currentValueCollection, flag, flag && flag2, current, def);
					}
					else if (value is IEnumerable)
					{
						IEnumerable enumerable = (IEnumerable)value;
						int num = 0;
						foreach (object current2 in enumerable)
						{
							if (current2 != null && !(current2 is Def) && GenTypes.IsCustomType(current2.GetType()))
							{
								string text = TranslationHandleUtility.GetBestHandleWithIndexForListElement(enumerable, current2);
								if (text.NullOrEmpty())
								{
									text = num.ToString();
								}
								string curNormalizedPath2 = string.Concat(new object[]
								{
									curNormalizedPath,
									".",
									current.Name,
									".",
									num
								});
								string curSuggestedPath2 = string.Concat(new string[]
								{
									curSuggestedPath,
									".",
									current.Name,
									".",
									text
								});
								DefInjectionUtility.ForEachPossibleDefInjectionInDefRecursive(current2, curNormalizedPath2, curSuggestedPath2, visited, flag, def, action);
							}
							num++;
						}
					}
					else if (value != null && GenTypes.IsCustomType(value.GetType()))
					{
						string curNormalizedPath3 = curNormalizedPath + "." + current.Name;
						string curSuggestedPath3 = curSuggestedPath + "." + current.Name;
						DefInjectionUtility.ForEachPossibleDefInjectionInDefRecursive(value, curNormalizedPath3, curSuggestedPath3, visited, flag, def, action);
					}
				}
			}
		}

		public static bool ShouldCheckMissingInjection(string str, FieldInfo fi, Def def)
		{
			return !def.generated && !str.NullOrEmpty() && !fi.HasAttribute<NoTranslateAttribute>() && !fi.HasAttribute<UnsavedAttribute>() && !fi.HasAttribute<MayTranslateAttribute>() && (fi.HasAttribute<MustTranslateAttribute>() || str.Contains(' '));
		}

		private static IEnumerable<FieldInfo> FieldsInDeterministicOrder(IEnumerable<FieldInfo> fields)
		{
			return from x in fields
			orderby x.HasAttribute<UnsavedAttribute>() || x.HasAttribute<NoTranslateAttribute>(), x.Name == "label" descending, x.Name == "description" descending, x.Name
			select x;
		}
	}
}
