using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Researcher : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				if (Find.ResearchManager.currentProj == null)
				{
					return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
				}
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
			}
		}

		public override bool Prioritized
		{
			get
			{
				return true;
			}
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return Find.ResearchManager.currentProj == null;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
			{
				return false;
			}
			Building_ResearchBench building_ResearchBench = t as Building_ResearchBench;
			return building_ResearchBench != null && currentProj.CanBeResearchedAt(building_ResearchBench, false) && pawn.CanReserve(t, 1);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(JobDefOf.Research, t);
		}

		public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			return t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
		}
	}
}
