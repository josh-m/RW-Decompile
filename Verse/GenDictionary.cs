using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public static class GenDictionary
	{
		public static string ToStringFullContents<K, V>(this Dictionary<K, V> dict)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<K, V> current in dict)
			{
				StringBuilder arg_50_0 = stringBuilder;
				K key = current.Key;
				string arg_4B_0 = key.ToString();
				string arg_4B_1 = ": ";
				V value = current.Value;
				arg_50_0.AppendLine(arg_4B_0 + arg_4B_1 + value.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
