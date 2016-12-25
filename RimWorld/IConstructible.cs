using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public interface IConstructible
	{
		List<ThingCountClass> MaterialsNeeded();

		ThingDef UIStuff();
	}
}
