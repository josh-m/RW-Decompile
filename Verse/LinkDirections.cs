using System;

namespace Verse
{
	[Flags]
	public enum LinkDirections : byte
	{
		Up = 1,
		Right = 2,
		Down = 4,
		Left = 8
	}
}
