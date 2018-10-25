using System;

namespace Verse
{
	public class LanguageWorker_Catalan : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return this.WithElLaArticle(str, gender, true);
			}
			if (plural)
			{
				return ((gender != Gender.Female) ? "uns " : "unes ") + str;
			}
			return ((gender != Gender.Female) ? "un " : "una ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return this.WithElLaArticle(str, gender, true);
			}
			if (plural)
			{
				return ((gender != Gender.Female) ? "els " : "les ") + str;
			}
			return this.WithElLaArticle(str, gender, false);
		}

		private string WithElLaArticle(string str, Gender gender, bool name)
		{
			if (str.Length == 0 || (!this.IsVowel(str[0]) && str[0] != 'h' && str[0] != 'H'))
			{
				return ((gender != Gender.Female) ? "el " : "la ") + str;
			}
			if (name)
			{
				return ((gender != Gender.Female) ? "n'" : "l'") + str;
			}
			return "l'" + str;
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			if (gender == Gender.Female)
			{
				return number + "a";
			}
			if (number == 1 || number == 3)
			{
				return number + "r";
			}
			if (number == 2)
			{
				return number + "n";
			}
			if (number == 4)
			{
				return number + "t";
			}
			return number + "è";
		}

		public bool IsVowel(char ch)
		{
			return "ieɛaoɔuəuàêèéòóüúIEƐAOƆUƏUÀÊÈÉÒÓÜÚ".IndexOf(ch) >= 0;
		}
	}
}
