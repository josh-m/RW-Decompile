using System;
using Verse;

namespace RimWorld
{
	public class WorkGiver_Refuel_Turret : WorkGiver_Refuel
	{
		public override JobDef JobStandard
		{
			get
			{
				return JobDefOf.RearmTurret;
			}
		}

		public override JobDef JobAtomic
		{
			get
			{
				return JobDefOf.RearmTurretAtomic;
			}
		}

		public override bool CanRefuelThing(Thing t)
		{
			return t is Building_Turret;
		}
	}
}
