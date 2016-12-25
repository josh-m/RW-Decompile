using System;

namespace Verse
{
	public static class ModToolUtility
	{
		public static bool IsValueEditable(this Type type)
		{
			return type.IsValueType || type == typeof(string);
		}
	}
}
