using System;
using Verse;

namespace RimWorld
{
	public static class ManhunterPackGenStepUtility
	{
		public static bool TryGetAnimalsKind(float points, int tile, out PawnKindDef animalKind)
		{
			return ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(points, tile, out animalKind) || ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(points, -1, out animalKind);
		}
	}
}
