using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_ViewArt : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn, null);
			IEnumerable<Thing> source = from thing in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Art)
			where thing.Faction == Faction.OfPlayer && !thing.IsForbidden(pawn) && (allowedOutside || thing.Position.Roofed(thing.Map)) && pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None, 1)
			select thing;
			Thing t;
			if (!source.TryRandomElementByWeight(delegate(Thing target)
			{
				CompArt compArt = target.TryGetComp<CompArt>();
				if (compArt == null)
				{
					Log.Error("No CompArt on thing being considered for viewing: " + target);
					return 0f;
				}
				if (!compArt.CanShowArt)
				{
					return 0f;
				}
				if (!compArt.Props.canBeEnjoyedAsArt)
				{
					return 0f;
				}
				float statValue = target.GetStatValue(StatDefOf.Beauty, true);
				return Mathf.Max(statValue, 0.5f);
			}, out t))
			{
				return null;
			}
			return new Job(this.def.jobDef, t);
		}
	}
}
