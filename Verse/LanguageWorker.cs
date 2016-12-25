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
				throw new ArgumentException();
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

		public virtual string WithDefiniteArticle(string str)
		{
			if (str.NullOrEmpty())
			{
				throw new ArgumentException();
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

		public virtual string OrdinalNumber(int number)
		{
			return number.ToString();
		}

		public virtual string PostProcessed(string str)
		{
			str = str.Replace("  ", " ");
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

		public virtual string Pluralize(string str)
		{
			return str;
		}
	}
}
