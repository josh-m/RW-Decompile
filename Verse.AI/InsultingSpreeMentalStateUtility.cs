using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public static class InsultingSpreeMentalStateUtility
	{
		private const int MaxRegionsToSearch = 40;

		public const int MaxDistance = 40;

		public const int MinTicksBetweenInsults = 1200;

		public static bool CanChaseAndInsult(Pawn bully, Pawn insulted, bool skipReachabilityCheck = false, bool allowPrisoners = true)
		{
			return insulted.RaceProps.Humanlike && (insulted.Faction == bully.Faction || (allowPrisoners && insulted.HostFaction == bully.Faction)) && insulted != bully && !insulted.Dead && !insulted.Downed && insulted.Spawned && insulted.Awake() && insulted.Position.InHorDistOf(bully.Position, 40f) && InteractionUtility.CanReceiveInteraction(insulted) && !insulted.HostileTo(bully) && Find.TickManager.TicksGame - insulted.mindState.lastHarmTick >= 833 && (skipReachabilityCheck || bully.CanReach(insulted, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn));
		}

		public static void GetInsultCandidatesFor(Pawn bully, List<Pawn> outCandidates, bool allowPrisoners = true)
		{
			outCandidates.Clear();
			Region region = bully.GetRegion(RegionType.Set_Passable);
			if (region == null)
			{
				return;
			}
			TraverseParms traverseParams = TraverseParms.For(bully, Danger.Deadly, TraverseMode.ByPawn, false);
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region to) => to.Allows(traverseParams, false), delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = (Pawn)list[i];
					if (InsultingSpreeMentalStateUtility.CanChaseAndInsult(bully, pawn, true, allowPrisoners))
					{
						outCandidates.Add(pawn);
					}
				}
				return false;
			}, 40, RegionType.Set_Passable);
		}
	}
}
