using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IConstructible
	{
		List<ThingCount> MaterialsNeeded();

		ThingDef UIStuff();
	}
}
