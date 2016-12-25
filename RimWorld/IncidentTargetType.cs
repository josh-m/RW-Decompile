using System;

namespace RimWorld
{
	[Flags]
	public enum IncidentTargetType : byte
	{
		None = 0,
		BaseMap = 1,
		TempMap = 2,
		Caravan = 4
	}
}
