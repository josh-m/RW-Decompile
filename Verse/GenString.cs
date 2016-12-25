using System;

namespace Verse
{
	public static class GenString
	{
		private static string[] numberStrings;

		static GenString()
		{
			GenString.numberStrings = new string[10000];
			for (int i = 0; i < 10000; i++)
			{
				GenString.numberStrings[i] = (i - 5000).ToString();
			}
		}

		public static string ToStringCached(this int num)
		{
			if (num < -4999)
			{
				return num.ToString();
			}
			if (num > 4999)
			{
				return num.ToString();
			}
			return GenString.numberStrings[num + 5000];
		}

		public static string TrimmedToLength(this string str, int length)
		{
			if (str == null || str.Length <= length)
			{
				return str;
			}
			return str.Substring(0, length);
		}
	}
}
