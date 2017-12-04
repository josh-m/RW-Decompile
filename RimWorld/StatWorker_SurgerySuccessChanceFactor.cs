using System;
using Verse;

namespace RimWorld
{
	public class StatWorker_SurgerySuccessChanceFactor : StatWorker
	{
		public override bool ShouldShowFor(BuildableDef eDef)
		{
			if (!base.ShouldShowFor(eDef))
			{
				return false;
			}
			if (!(eDef is ThingDef))
			{
				return false;
			}
			ThingDef thingDef = eDef as ThingDef;
			return typeof(Building_Bed).IsAssignableFrom(thingDef.thingClass);
		}
	}
}
