using System;
using System.Collections.Generic;

namespace Verse
{
	public class ThingDefComparer : IEqualityComparer<ThingDef>
	{
		public static readonly ThingDefComparer Instance = new ThingDefComparer();

		public bool Equals(ThingDef x, ThingDef y)
		{
			return (x == null && y == null) || (x != null && y != null && x.shortHash == y.shortHash);
		}

		public int GetHashCode(ThingDef obj)
		{
			return obj.GetHashCode();
		}
	}
}
