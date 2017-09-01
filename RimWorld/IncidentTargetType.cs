using System;

namespace RimWorld
{
	[Flags]
	public enum IncidentTargetType : byte
	{
		None = 0,
		MapPlayerHome = 1,
		MapTempIncident = 2,
		MapMisc = 4,
		Caravan = 8,
		World = 16,
		All = 31
	}
}
