using System;

namespace RimWorld
{
	[Flags]
	public enum OverlayTypes
	{
		NeedsPower = 1,
		PowerOff = 2,
		BurningWick = 4,
		Forbidden = 8,
		ForbiddenBig = 16,
		QuestionMark = 32,
		BrokenDown = 64,
		OutOfFuel = 128
	}
}
