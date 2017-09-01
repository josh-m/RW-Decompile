using System;

namespace Verse
{
	[Flags]
	public enum RegionType
	{
		None = 0,
		ImpassableFreeAirExchange = 1,
		Normal = 2,
		Portal = 4,
		Set_Passable = 6,
		Set_Impassable = 1,
		Set_All = 7
	}
}
