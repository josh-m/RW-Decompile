using System;
using Verse;

namespace RimWorld.Planet
{
	public class SiteCoreOrPartParams : IExposable
	{
		public int randomValue;

		public float threatPoints;

		public ThingDef preciousLumpResources;

		public PawnKindDef animalKind;

		public int turretsCount;

		public int mortarsCount;

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.randomValue, "randomValue", 0, false);
			Scribe_Values.Look<float>(ref this.threatPoints, "threatPoints", 0f, false);
			Scribe_Defs.Look<ThingDef>(ref this.preciousLumpResources, "preciousLumpResources");
			Scribe_Defs.Look<PawnKindDef>(ref this.animalKind, "animalKind");
			Scribe_Values.Look<int>(ref this.turretsCount, "turretsCount", 0, false);
			Scribe_Values.Look<int>(ref this.mortarsCount, "mortarsCount", 0, false);
		}
	}
}
