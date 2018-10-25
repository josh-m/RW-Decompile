using System;

namespace Verse
{
	public class LanguageWorker_French : LanguageWorker
	{
		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (name)
			{
				return str;
			}
			if (plural)
			{
				return "des " + str;
			}
			return ((gender != Gender.Female) ? "un " : "une ") + str;
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
			if (plural)
			{
				return "les " + str;
			}
			char ch = str[0];
			if (this.IsVowel(ch))
			{
				return "l'" + str;
			}
			return ((gender != Gender.Female) ? "le " : "la ") + str;
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			return (number != 1) ? (number + "e") : (number + "er");
		}

		public override string Pluralize(string str, Gender gender, int count = -1)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			if (str[str.Length - 1] == 's' || str[str.Length - 1] == 'x')
			{
				return str;
			}
			return str + "s";
		}

		public override string PostProcessed(string str)
		{
			return this.PostProcessedInt(base.PostProcessed(str));
		}

		public override string PostProcessedKeyedTranslation(string translation)
		{
			return this.PostProcessedInt(base.PostProcessedKeyedTranslation(translation));
		}

		public bool IsVowel(char ch)
		{
			return "hiueøoɛœəɔaãɛ̃œ̃ɔ̃IHUEØOƐŒƏƆAÃƐ̃Œ̃Ɔ̃".IndexOf(ch) >= 0;
		}

		private string PostProcessedInt(string str)
		{
			return str.Replace(" de le ", " du ").Replace(" de les ", " des ").Replace(" de des ", " des ").Replace(" si il ", " s'il ").Replace(" Si il ", " S'il ");
		}
	}
}
