using System;
using System.Globalization;

namespace Verse
{
	public abstract class LanguageWorker
	{
		public virtual string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return string.Empty;
			}
			if (name)
			{
				return str;
			}
			if ("IndefiniteForm".CanTranslate())
			{
				return "IndefiniteForm".Translate(str);
			}
			return "IndefiniteArticle".Translate() + " " + str;
		}

		public string WithIndefiniteArticle(string str, bool plural = false, bool name = false)
		{
			return this.WithIndefiniteArticle(str, LanguageDatabase.activeLanguage.ResolveGender(str, null), plural, name);
		}

		public string WithIndefiniteArticlePostProcessed(string str, Gender gender, bool plural = false, bool name = false)
		{
			return this.PostProcessed(this.WithIndefiniteArticle(str, gender, plural, name));
		}

		public string WithIndefiniteArticlePostProcessed(string str, bool plural = false, bool name = false)
		{
			return this.PostProcessed(this.WithIndefiniteArticle(str, plural, name));
		}

		public virtual string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
			{
				return string.Empty;
			}
			if (name)
			{
				return str;
			}
			if ("DefiniteForm".CanTranslate())
			{
				return "DefiniteForm".Translate(str);
			}
			return "DefiniteArticle".Translate() + " " + str;
		}

		public string WithDefiniteArticle(string str, bool plural = false, bool name = false)
		{
			return this.WithDefiniteArticle(str, LanguageDatabase.activeLanguage.ResolveGender(str, null), plural, name);
		}

		public string WithDefiniteArticlePostProcessed(string str, Gender gender, bool plural = false, bool name = false)
		{
			return this.PostProcessed(this.WithDefiniteArticle(str, gender, plural, name));
		}

		public string WithDefiniteArticlePostProcessed(string str, bool plural = false, bool name = false)
		{
			return this.PostProcessed(this.WithDefiniteArticle(str, plural, name));
		}

		public virtual string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			return number.ToString();
		}

		public virtual string PostProcessed(string str)
		{
			str = str.MergeMultipleSpaces(true);
			return str;
		}

		public virtual string ToTitleCase(string str)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
		}

		public virtual string Pluralize(string str, Gender gender, int count = -1)
		{
			return str;
		}

		public string Pluralize(string str, int count = -1)
		{
			return this.Pluralize(str, LanguageDatabase.activeLanguage.ResolveGender(str, null), count);
		}

		public virtual string PostProcessedKeyedTranslation(string translation)
		{
			return translation;
		}
	}
}
