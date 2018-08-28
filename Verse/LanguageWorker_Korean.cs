using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Verse
{
	public class LanguageWorker_Korean : LanguageWorker
	{
		private struct JosaPair
		{
			public readonly string josa1;

			public readonly string josa2;

			public JosaPair(string josa1, string josa2)
			{
				this.josa1 = josa1;
				this.josa2 = josa2;
			}
		}

		private static StringBuilder tmpStringBuilder = new StringBuilder();

		private static readonly Regex JosaPattern = new Regex("\\(이\\)가|\\(와\\)과|\\(을\\)를|\\(은\\)는|\\(아\\)야|\\(이\\)여|\\(으\\)로|\\(이\\)라");

		private static readonly Dictionary<string, LanguageWorker_Korean.JosaPair> JosaPatternPaired = new Dictionary<string, LanguageWorker_Korean.JosaPair>
		{
			{
				"(이)가",
				new LanguageWorker_Korean.JosaPair("이", "가")
			},
			{
				"(와)과",
				new LanguageWorker_Korean.JosaPair("과", "와")
			},
			{
				"(을)를",
				new LanguageWorker_Korean.JosaPair("을", "를")
			},
			{
				"(은)는",
				new LanguageWorker_Korean.JosaPair("은", "는")
			},
			{
				"(아)야",
				new LanguageWorker_Korean.JosaPair("아", "야")
			},
			{
				"(이)여",
				new LanguageWorker_Korean.JosaPair("이여", "여")
			},
			{
				"(으)로",
				new LanguageWorker_Korean.JosaPair("으로", "로")
			},
			{
				"(이)라",
				new LanguageWorker_Korean.JosaPair("이라", "라")
			}
		};

		private static readonly List<char> AlphabetEndPattern = new List<char>
		{
			'b',
			'c',
			'k',
			'l',
			'm',
			'n',
			'p',
			'q',
			't'
		};

		public override string PostProcessed(string str)
		{
			str = base.PostProcessed(str);
			str = this.ReplaceJosa(str);
			return str;
		}

		public override string PostProcessedBackstoryDescription(string desc)
		{
			desc = base.PostProcessedBackstoryDescription(desc);
			desc = this.ReplaceJosa(desc);
			return desc;
		}

		public override string PostProcessedKeyedTranslation(string translation, string key, params object[] args)
		{
			translation = base.PostProcessedKeyedTranslation(translation, key, args);
			translation = this.ReplaceJosa(translation);
			return translation;
		}

		public string ReplaceJosa(string src)
		{
			LanguageWorker_Korean.tmpStringBuilder.Length = 0;
			MatchCollection matchCollection = LanguageWorker_Korean.JosaPattern.Matches(src);
			int num = 0;
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match match = matchCollection[i];
				LanguageWorker_Korean.JosaPair josaPair = LanguageWorker_Korean.JosaPatternPaired[match.Value];
				LanguageWorker_Korean.tmpStringBuilder.Append(src, num, match.Index - num);
				if (match.Index > 0)
				{
					char inChar = src[match.Index - 1];
					if ((match.Value != "(으)로" && this.HasJong(inChar)) || (match.Value == "(으)로" && this.HasJongExceptRieul(inChar)))
					{
						LanguageWorker_Korean.tmpStringBuilder.Append(josaPair.josa1);
					}
					else
					{
						LanguageWorker_Korean.tmpStringBuilder.Append(josaPair.josa2);
					}
				}
				else
				{
					LanguageWorker_Korean.tmpStringBuilder.Append(josaPair.josa1);
				}
				num = match.Index + match.Length;
			}
			LanguageWorker_Korean.tmpStringBuilder.Append(src, num, src.Length - num);
			return LanguageWorker_Korean.tmpStringBuilder.ToString();
		}

		private bool HasJong(char inChar)
		{
			if (!this.IsKorean(inChar))
			{
				return LanguageWorker_Korean.AlphabetEndPattern.Contains(inChar);
			}
			int num = (int)(inChar - '가');
			int num2 = num % 28;
			return num2 > 0;
		}

		private bool HasJongExceptRieul(char inChar)
		{
			if (!this.IsKorean(inChar))
			{
				return false;
			}
			int num = (int)(inChar - '가');
			int num2 = num % 28;
			return num2 != 8 && num2 != 0;
		}

		private bool IsKorean(char inChar)
		{
			return inChar >= '가' && inChar <= '힣';
		}
	}
}
