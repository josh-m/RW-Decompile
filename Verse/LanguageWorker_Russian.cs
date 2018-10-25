using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Verse
{
	public class LanguageWorker_Russian : LanguageWorker
	{
		private static readonly Regex _languageWorkerTagRegex = new Regex("\\$(.*?)\\$", RegexOptions.Compiled);

		private static readonly Regex _numYearsRegex = new Regex("([0-9]+) лет", RegexOptions.Compiled);

		private static readonly Regex _numQuadrumsRegex = new Regex("([0-9]+) кварталов", RegexOptions.Compiled);

		private static readonly Regex _numDaysRegex = new Regex("([0-9]+) дней", RegexOptions.Compiled);

		private static readonly Regex _numTimesRegex = new Regex("([0-9]+) раз", RegexOptions.Compiled);

		private static readonly Regex _passedDaysRegex = new Regex("Прошло ([0-9]+)", RegexOptions.Compiled);

		private static List<string> tags = new List<string>();

		private static readonly char[] Comma = new char[]
		{
			','
		};

		public override string PostProcessedKeyedTranslation(string translation)
		{
			translation = base.PostProcessedKeyedTranslation(translation);
			LanguageWorker_Russian.tags.Clear();
			translation = LanguageWorker_Russian._languageWorkerTagRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateTags(match, LanguageWorker_Russian.tags));
			foreach (string current in LanguageWorker_Russian.tags)
			{
				if (current != null)
				{
					if (current == "date")
					{
						translation = translation.Replace("Мартомай", "Мартомая").Replace("Июгуст", "Июгуста").Replace("Сентоноябрь", "Сентоноября").Replace("Декавраль", "Декавраля");
						continue;
					}
					if (current == "XItems1")
					{
						translation = LanguageWorker_Russian._numYearsRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "лет", "год", "года"));
						translation = LanguageWorker_Russian._numQuadrumsRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "кварталов", "квартал", "квартала"));
						translation = LanguageWorker_Russian._numDaysRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "дней", "день", "дня"));
						translation = LanguageWorker_Russian._numTimesRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "раз", "раз", "раза"));
						continue;
					}
					if (current == "XItems2")
					{
						translation = LanguageWorker_Russian._numYearsRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "лет", "года", "лет"));
						translation = LanguageWorker_Russian._numQuadrumsRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "кварталов", "квартала", "кварталов"));
						translation = LanguageWorker_Russian._numDaysRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "дней", "дня", "дней"));
						translation = LanguageWorker_Russian._numTimesRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "раз", "раза", "раз"));
						continue;
					}
					if (current == "passed")
					{
						translation = LanguageWorker_Russian._passedDaysRegex.Replace(translation, (Match match) => LanguageWorker_Russian.EvaluateCasedItem(match, "Прошло", "Прошёл", "Прошло"));
						continue;
					}
				}
				Log.Warning(string.Format("Unexpected LanguageWorker_Russian tag: {0}", current), false);
			}
			return translation;
		}

		public override string Pluralize(string str, Gender gender, int count = -1)
		{
			if (str.NullOrEmpty())
			{
				return str;
			}
			char c = str[str.Length - 1];
			char c2 = (str.Length >= 2) ? str[str.Length - 2] : '\0';
			if (gender != Gender.Male)
			{
				if (gender != Gender.Female)
				{
					if (gender == Gender.None)
					{
						if (c == 'o')
						{
							return str.Substring(0, str.Length - 1) + 'a';
						}
						if (c == 'O')
						{
							return str.Substring(0, str.Length - 1) + 'A';
						}
						if (c == 'e' || c == 'E')
						{
							char value = char.ToUpper(c2);
							if ("ГКХЖЧШЩЦ".IndexOf(value) >= 0)
							{
								if (c == 'e')
								{
									return str.Substring(0, str.Length - 1) + 'a';
								}
								if (c == 'E')
								{
									return str.Substring(0, str.Length - 1) + 'A';
								}
							}
							else
							{
								if (c == 'e')
								{
									return str.Substring(0, str.Length - 1) + 'я';
								}
								if (c == 'E')
								{
									return str.Substring(0, str.Length - 1) + 'Я';
								}
							}
						}
					}
				}
				else
				{
					if (c == 'я')
					{
						return str.Substring(0, str.Length - 1) + 'и';
					}
					if (c == 'ь')
					{
						return str.Substring(0, str.Length - 1) + 'и';
					}
					if (c == 'Я')
					{
						return str.Substring(0, str.Length - 1) + 'И';
					}
					if (c == 'Ь')
					{
						return str.Substring(0, str.Length - 1) + 'И';
					}
					if (c == 'a' || c == 'A')
					{
						char value2 = char.ToUpper(c2);
						if ("ГКХЖЧШЩ".IndexOf(value2) >= 0)
						{
							if (c == 'a')
							{
								return str.Substring(0, str.Length - 1) + 'и';
							}
							return str.Substring(0, str.Length - 1) + 'И';
						}
						else
						{
							if (c == 'a')
							{
								return str.Substring(0, str.Length - 1) + 'ы';
							}
							return str.Substring(0, str.Length - 1) + 'Ы';
						}
					}
				}
			}
			else
			{
				if (LanguageWorker_Russian.IsConsonant(c))
				{
					return str + 'ы';
				}
				if (c == 'й')
				{
					return str.Substring(0, str.Length - 1) + 'и';
				}
				if (c == 'ь')
				{
					return str.Substring(0, str.Length - 1) + 'и';
				}
				if (c == 'Й')
				{
					return str.Substring(0, str.Length - 1) + 'И';
				}
				if (c == 'Ь')
				{
					return str.Substring(0, str.Length - 1) + 'И';
				}
			}
			return str;
		}

		private static string EvaluateTags(Match match, List<string> tags)
		{
			if (match.Groups.Count <= 1)
			{
				return string.Empty;
			}
			string value = match.Groups[1].Value;
			string[] array = value.Split(LanguageWorker_Russian.Comma, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string element = array[i];
				tags.AddDistinct(element);
			}
			return string.Empty;
		}

		private static string EvaluateCasedItem(Match match, string caseDefault, string case1, string case2)
		{
			int number;
			if (!LanguageWorker_Russian.TryParseNumber(match, out number))
			{
				Log.Warning(string.Format("{0} doesn't have a number", match.Value), false);
				return match.Value;
			}
			return match.Value.Replace(caseDefault, LanguageWorker_Russian.GetCasedItem(number, caseDefault, case1, case2));
		}

		private static bool TryParseNumber(Match match, out int number)
		{
			number = -2147483648;
			if (match.Groups.Count <= 1)
			{
				return false;
			}
			string value = match.Groups[1].Value;
			return int.TryParse(value, out number);
		}

		private static string GetCasedItem(int number, string caseDefault, string case1, string case2)
		{
			int numberCase = LanguageWorker_Russian.GetNumberCase(number);
			if (numberCase == 1)
			{
				return case1;
			}
			if (numberCase != 2)
			{
				return caseDefault;
			}
			return case2;
		}

		private static int GetNumberCase(int number)
		{
			int num = number % 10;
			int num2 = number / 10 % 10;
			if (num2 == 1)
			{
				return 0;
			}
			switch (num)
			{
			case 1:
				return 1;
			case 2:
			case 3:
			case 4:
				return 2;
			default:
				return 0;
			}
		}

		private static string ProcessDate(string str)
		{
			return str.Replace("Мартомай", "Мартомая").Replace("Июгуст", "Июгуста").Replace("Сентоноябрь", "Сентоноября").Replace("Декавраль", "Декавраля");
		}

		private static bool IsConsonant(char ch)
		{
			return "бвгджзклмнпрстфхцчшщБВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(ch) >= 0;
		}
	}
}
