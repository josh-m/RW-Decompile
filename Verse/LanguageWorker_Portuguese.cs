using System;

namespace Verse
{
	public class LanguageWorker_Portuguese : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return ((gender != Gender.Female) ? "uns " : "umas ") + str;
			}
			return ((gender != Gender.Female) ? "um " : "uma ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return ((gender != Gender.Female) ? "os " : "as ") + str;
			}
			return ((gender != Gender.Female) ? "o " : "a ") + str;
		}
	}
}
