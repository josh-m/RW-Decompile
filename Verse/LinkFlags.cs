using System;

namespace Verse
{
	[Flags]
	public enum LinkFlags
	{
		None = 0,
		MapEdge = 1,
		Rock = 2,
		Wall = 4,
		Sandbags = 8,
		PowerConduit = 16,
		Custom1 = 131072,
		Custom2 = 262144,
		Custom3 = 524288,
		Custom4 = 1048576,
		Custom5 = 2097152,
		Custom6 = 4194304,
		Custom7 = 8388608,
		Custom8 = 16777216,
		Custom9 = 33554432,
		Custom10 = 67108864
	}
}
