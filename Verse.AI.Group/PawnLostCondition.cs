using System;

namespace Verse.AI.Group
{
	public enum PawnLostCondition : byte
	{
		Undefined,
		Vanished,
		IncappedOrKilled,
		MadePrisoner,
		ChangedFaction,
		ExitedMap,
		LeftVoluntarily,
		Drafted
	}
}
