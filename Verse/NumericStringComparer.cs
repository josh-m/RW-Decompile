using System;
using System.Collections.Generic;

namespace Verse
{
	public class NumericStringComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			if (x.Contains("~"))
			{
				string[] array = x.Split(new char[]
				{
					'~'
				});
				if (array.Length == 2)
				{
					x = array[0];
				}
			}
			if (y.Contains("~"))
			{
				string[] array2 = y.Split(new char[]
				{
					'~'
				});
				if (array2.Length == 2)
				{
					y = array2[0];
				}
			}
			if ((x.EndsWith("%") && y.EndsWith("%")) || (x.EndsWith("C") && y.EndsWith("C")))
			{
				x = x.Substring(0, x.Length - 1);
				y = y.Substring(0, y.Length - 1);
			}
			float num;
			float value;
			if (float.TryParse(x, out num) && float.TryParse(y, out value))
			{
				return num.CompareTo(value);
			}
			return x.CompareTo(y);
		}
	}
}
