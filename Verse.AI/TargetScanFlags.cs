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
		NeedNonBurning = 8,
		NeedThreat = 16
	}
}
