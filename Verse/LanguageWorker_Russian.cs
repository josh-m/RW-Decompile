using System;

namespace Verse
{
	public class LanguageWorker_Russian : LanguageWorker
	{
		public override string OrdinalNumber(int number)
		{
			return number + "-й";
		}

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			str = this.ReplaceAtStartSpaceOrNewline(str, "1 лет", "1 год");
			str = this.ReplaceAtStartSpaceOrNewline(str, "1 сезонов", "1 сезон");
			str = this.ReplaceAtStartSpaceOrNewline(str, "1 дней", "1 день");
			str = this.ReplaceAtStartSpaceOrNewline(str, "1 часов", "1 час");
			str = this.ReplaceAtStartSpaceOrNewline(str, "2 лет", "2 года");
			str = this.ReplaceAtStartSpaceOrNewline(str, "2 сезонов", "2 сезона");
			str = this.ReplaceAtStartSpaceOrNewline(str, "2 дней", "2 дня");
			str = this.ReplaceAtStartSpaceOrNewline(str, "2 часов", "2 часа");
			str = this.ReplaceAtStartSpaceOrNewline(str, "3 лет", "3 года");
			str = this.ReplaceAtStartSpaceOrNewline(str, "3 сезонов", "3 сезона");
			str = this.ReplaceAtStartSpaceOrNewline(str, "3 дней", "3 дня");
			str = this.ReplaceAtStartSpaceOrNewline(str, "3 часов", "3 часа");
			str = this.ReplaceAtStartSpaceOrNewline(str, "4 лет", "4 года");
			str = this.ReplaceAtStartSpaceOrNewline(str, "4 сезонов", "4 сезона");
			str = this.ReplaceAtStartSpaceOrNewline(str, "4 дней", "4 дня");
			str = this.ReplaceAtStartSpaceOrNewline(str, "4 часов", "4 часа");
			return str;
		}

		private string ReplaceAtStartSpaceOrNewline(string str, string toReplace, string replaceWith)
		{
			if (!str.Contains(toReplace))
			{
				return str;
			}
			str = str.Replace(' ' + toReplace, ' ' + replaceWith);
			str = str.Replace('\n' + toReplace, '\n' + replaceWith);
			if (str.StartsWith(toReplace))
			{
				str = replaceWith + str.Substring(toReplace.Length);
			}
			return str;
		}
	}
}
