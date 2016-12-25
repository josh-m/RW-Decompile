using System;
using System.Collections.Generic;

namespace RimWorld
{
	public class FastIntComparer : IEqualityComparer<int>
	{
		public static readonly FastIntComparer Instance = new FastIntComparer();

		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj;
		}
	}
}
