using System;

namespace Verse
{
	[Flags]
	public enum WorkTags
	{
		None = 0,
		Intellectual = 2,
		ManualDumb = 4,
		ManualSkilled = 8,
		Violent = 16,
		Caring = 32,
		Social = 64,
		Scary = 128,
		Animals = 256,
		Artistic = 512,
		Crafting = 1024,
		Cooking = 2048,
		Firefighting = 4096,
		Cleaning = 8192,
		Hauling = 16384,
		PlantWork = 32768,
		Mining = 65536
	}
}
