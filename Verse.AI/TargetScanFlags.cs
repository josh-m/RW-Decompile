using System;

namespace Verse.AI
{
	[Flags]
	public enum TargetScanFlags : byte
	{
		None = 0,
		NeedLOSToPawns = 1,
		NeedLOSToNonPawns = 2,
		NeedLOSToAll = 3,
		NeedReachable = 4,
		NeedReachableIfCantHitFromMyPos = 8,
		NeedNonBurning = 16,
		NeedThreat = 32,
		LOSBlockableByGas = 64
	}
}
