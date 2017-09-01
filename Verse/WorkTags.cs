using System;

namespace Verse
{
	[Flags]
	public enum WorkTags
	{
		None = 0,
		ManualDumb = 2,
		ManualSkilled = 4,
		Violent = 8,
		Caring = 16,
		Social = 32,
		Intellectual = 64,
		Animals = 128,
		Artistic = 256,
		Crafting = 512,
		Cooking = 1024,
		Firefighting = 2048,
		Cleaning = 4096,
		Hauling = 8192,
		PlantWork = 16384,
		Mining = 32768
	}
}
