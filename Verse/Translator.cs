using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public static class Translator
	{
		public static bool CanTranslate(this string key)
		{
			return LanguageDatabase.activeLanguage.HaveTextForKey(key);
		}

		public static bool TryTranslate(this string key, out string result)
		{
			if (key.NullOrEmpty())
			{
				result = key;
				return false;
			}
			if (LanguageDatabase.activeLanguage == null)
			{
				Log.Error("No active language! Cannot translate from key " + key + ".");
				result = key;
				return true;
			}
			if (LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out result))
			{
				return true;
			}
			result = key;
			return false;
		}

		public static string Translate(this string key)
		{
			if (key.NullOrEmpty())
			{
				return key;
			}
			if (LanguageDatabase.activeLanguage == null)
			{
				Log.Error("No active language! Cannot translate from key " + key + ".");
				return key;
			}
			string text;
			if (LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out text))
			{
				return text;
			}
			LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out text);
			if (Prefs.DevMode)
			{
				text = Translator.PseudoTranslated(text);
			}
			return text;
		}

		public static string Translate(this string key, params object[] args)
		{
			if (key == null || key == string.Empty)
			{
				return key;
			}
			if (LanguageDatabase.activeLanguage == null)
			{
				Log.Error("No active language! Cannot translate from key " + key + ".");
				return key;
			}
			string text;
			if (!LanguageDatabase.activeLanguage.TryGetTextFromKey(key, out text))
			{
				LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out text);
				if (Prefs.DevMode)
				{
					text = Translator.PseudoTranslated(text);
				}
			}
			string result = text;
			try
			{
				result = string.Format(text, args);
			}
			catch (Exception ex)
			{
				Log.Error("Exception translating '" + text + "': " + ex.ToString());
			}
			return result;
		}

		public static bool TryGetTranslatedStringsForFile(string fileName, out List<string> stringList)
		{
			if (!LanguageDatabase.activeLanguage.TryGetStringsFromFile(fileName, out stringList) && !LanguageDatabase.defaultLanguage.TryGetStringsFromFile(fileName, out stringList))
			{
				Log.Error("No string files for " + fileName + ".");
				return false;
			}
			return true;
		}

		private static string PseudoTranslated(string original)
		{
			if (!Prefs.DevMode)
			{
				return original;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < original.Length; i++)
			{
				char c = original[i];
				string value;
				switch (c)
				{
				case 'a':
					value = "à";
					break;
				case 'b':
					value = "þ";
					break;
				case 'c':
					value = "ç";
					break;
				case 'd':
					value = "ð";
					break;
				case 'e':
					value = "è";
					break;
				case 'f':
					value = "Ƒ";
					break;
				case 'g':
					value = "ğ";
					break;
				case 'h':
					value = "ĥ";
					break;
				case 'i':
					value = "ì";
					break;
				case 'j':
					value = "ĵ";
					break;
				case 'k':
					value = "к";
					break;
				case 'l':
					value = "ſ";
					break;
				case 'm':
					value = "ṁ";
					break;
				case 'n':
					value = "ƞ";
					break;
				case 'o':
					value = "ò";
					break;
				case 'p':
					value = "ṗ";
					break;
				case 'q':
					value = "q";
					break;
				case 'r':
					value = "ṟ";
					break;
				case 's':
					value = "ș";
					break;
				case 't':
					value = "ṭ";
					break;
				case 'u':
					value = "ù";
					break;
				case 'v':
					value = "ṽ";
					break;
				case 'w':
					value = "ẅ";
					break;
				case 'x':
					value = "ẋ";
					break;
				case 'y':
					value = "ý";
					break;
				case 'z':
					value = "ž";
					break;
				default:
					value = string.Empty + c;
					break;
				}
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}
	}
}
