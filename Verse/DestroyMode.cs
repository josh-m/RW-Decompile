using System;

namespace Verse
{
	public enum DestroyMode : byte
	{
		Vanish,
		KillFinalize,
		Deconstruct,
		FailConstruction,
		Cancel,
		Refund
	}
}
