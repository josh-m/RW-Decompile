using System;
using System.Globalization;

namespace Verse
{
	public abstract class LanguageWorker
	{
		public virtual string WithIndefiniteArticle(string str)
		{
			if (str.NullOrEmpty())
			{
				return string.Empty;
			}
			if ("IndefiniteForm".CanTranslate())
			{
				return "IndefiniteForm".Translate(new object[]
				{
					str
				});
			}
			return "IndefiniteArticle".Translate() + " " + str;
		}

		public string WithIndefiniteArticlePostProcessed(string str)
		{
			return this.PostProcessed(this.WithIndefiniteArticle(str));
		}

		public virtual string WithDefiniteArticle(string str)
		{
			if (str.NullOrEmpty())
			{
				return string.Empty;
			}
			if ("DefiniteForm".CanTranslate())
			{
				return "DefiniteForm".Translate(new object[]
				{
					str
				});
			}
			return "DefiniteArticle".Translate() + " " + str;
		}

		public string WithDefiniteArticlePostProcessed(string str)
		{
			return this.PostProcessed(this.WithDefiniteArticle(str));
		}

		public virtual string OrdinalNumber(int number)
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

		public virtual string Pluralize(string str, int count = -1)
		{
			return str;
		}

		public virtual string PostProcessedBackstoryDescription(string desc)
		{
			return desc;
		}

		public virtual string PostProcessedKeyedTranslation(string translation, string key, params object[] args)
		{
			return translation;
		}
	}
}
