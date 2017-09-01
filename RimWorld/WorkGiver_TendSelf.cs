using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_TendSelf : WorkGiver_Tend
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Undefined);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			yield return pawn;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			bool flag = base.HasJobOnThing(pawn, t, forced) && pawn == t && pawn.playerSettings != null;
			if (flag && !pawn.playerSettings.selfTend)
			{
				JobFailReason.Is("SelfTendDisabled".Translate());
			}
			return flag && pawn.playerSettings.selfTend;
		}
	}
}
