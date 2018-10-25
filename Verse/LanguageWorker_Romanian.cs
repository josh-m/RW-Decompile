using System;

namespace Verse
{
	public class LanguageWorker_Romanian : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return (gender != Gender.Male) ? (str + 'e') : (str + 'i');
			}
			return ((gender != Gender.Female) ? "un " : "a ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (name)
			{
				return str;
			}
			char ch = str[str.Length - 1];
			if (plural)
			{
				return (gender != Gender.Male) ? (str + 'e') : (str + 'i');
			}
			if (!this.IsVowel(ch))
			{
				return str + "ul";
			}
			if (gender == Gender.Male)
			{
				return str + "le";
			}
			return str + 'a';
		}

		public bool IsVowel(char ch)
		{
			return "aeiouâîAEIOUÂÎ".IndexOf(ch) >= 0;
		}
	}
}
