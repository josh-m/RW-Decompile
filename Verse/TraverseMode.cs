using System;

namespace Verse
{
	public enum TraverseMode : byte
	{
		PassAnything,
		ByPawn,
		PassDoors,
		NoPassClosedDoors
	}
}
