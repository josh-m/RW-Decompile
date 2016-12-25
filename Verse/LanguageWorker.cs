using System;

namespace Verse
{
	public abstract class LanguageWorker
	{
		public abstract string WithIndefiniteArticle(string str);

		public abstract string WithDefiniteArticle(string str);

		public abstract string OrdinalNumber(int number);

		public virtual string PostProcessed(string str)
		{
			str = str.Replace("  ", " ");
			return str;
		}
	}
}
