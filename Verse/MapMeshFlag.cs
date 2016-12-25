using System;

namespace Verse
{
	[Flags]
	public enum MapMeshFlag
	{
		None = 0,
		Things = 1,
		FogOfWar = 2,
		Buildings = 4,
		GroundGlow = 8,
		Terrain = 16,
		Roofs = 32,
		Snow = 64,
		Zone = 128,
		PowerGrid = 256,
		BuildingsDamage = 512
	}
}
