using System;

namespace Verse
{
	public class LanguageWorker_English : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str)
		{
			if (str.NullOrEmpty())
			{
				throw new ArgumentException();
			}
			return "a " + str;
		}

		public override string WithDefiniteArticle(string str)
		{
			if (str.NullOrEmpty())
			{
				throw new ArgumentException();
			}
			return "the " + str;
		}

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			if (str.StartsWith("a ", StringComparison.OrdinalIgnoreCase) && (str[2] == 'a' || str[2] == 'e' || str[2] == 'i' || str[2] == 'o' || str[2] == 'u'))
			{
				str = str.Insert(1, "n");
			}
			str = str.Replace(" a a", " an a");
			str = str.Replace(" a e", " an e");
			str = str.Replace(" a i", " an i");
			str = str.Replace(" a o", " an o");
			str = str.Replace(" a u", " an u");
			str = str.Replace(" a hour", " an hour");
			str = str.Replace(" A a", " An a");
			str = str.Replace(" A e", " An e");
			str = str.Replace(" A i", " An i");
			str = str.Replace(" A o", " An o");
			str = str.Replace(" A u", " An u");
			str = str.Replace(" A hour", " An hour");
			return str;
		}

		public override string OrdinalNumber(int number)
		{
			int num = number % 10;
			int num2 = number / 10 % 10;
			if (num2 != 1)
			{
				if (num == 1)
				{
					return number + "st";
				}
				if (num == 2)
				{
					return number + "nd";
				}
				if (num == 3)
				{
					return number + "rd";
				}
			}
			return number + "th";
		}
	}
}
