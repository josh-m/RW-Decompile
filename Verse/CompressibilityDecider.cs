using System;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace Verse
{
	public static class CompressibilityDecider
	{
		private const int NewlineInterval = 140;

		private static HashSet<Thing> referencedThings = new HashSet<Thing>();

		public static void DetermineReferences()
		{
			CompressibilityDecider.referencedThings.Clear();
			foreach (Thing current in from des in Find.DesignationManager.allDesignations
			select des.target.Thing)
			{
				CompressibilityDecider.referencedThings.Add(current);
			}
			foreach (Thing current2 in Find.Reservations.AllReservedThings())
			{
				CompressibilityDecider.referencedThings.Add(current2);
			}
			foreach (Pawn current3 in Find.MapPawns.AllPawnsSpawned)
			{
				Job curJob = current3.jobs.curJob;
				if (curJob != null)
				{
					if (curJob.targetA.HasThing)
					{
						CompressibilityDecider.referencedThings.Add(curJob.targetA.Thing);
					}
					if (curJob.targetB.HasThing)
					{
						CompressibilityDecider.referencedThings.Add(curJob.targetB.Thing);
					}
					if (curJob.targetC.HasThing)
					{
						CompressibilityDecider.referencedThings.Add(curJob.targetC.Thing);
					}
				}
			}
		}

		public static bool IsSaveCompressible(this Thing t)
		{
			return !Scribe.writingForDebug && t.def.saveCompressible && (!t.def.useHitPoints || t.HitPoints == t.MaxHitPoints) && t.holder == null && !CompressibilityDecider.referencedThings.Contains(t);
		}
	}
}
