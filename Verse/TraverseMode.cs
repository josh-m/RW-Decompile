using System;

namespace Verse
{
	public enum TraverseMode : byte
	{
		ByPawn,
		PassDoors,
		NoPassClosedDoors,
		PassAllDestroyableThings
	}
}
