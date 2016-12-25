using System;

namespace Verse
{
	[Flags]
	public enum MeshParts : byte
	{
		None = 0,
		Verts = 1,
		Tris = 2,
		Colors = 4,
		UVs = 8,
		All = 127
	}
}
