using System;

namespace Verse
{
	public class LanguageWorker_Default : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str)
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

		public override string WithDefiniteArticle(string str)
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

		public override string OrdinalNumber(int number)
		{
			return number.ToString();
		}
	}
}
