using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class LoadTransportersJobUtility
	{
		private static HashSet<Thing> neededThings = new HashSet<Thing>();

		public static bool HasJobOnTransporter(Pawn pawn, CompTransporter transporter)
		{
			return !transporter.parent.IsForbidden(pawn) && transporter.AnythingLeftToLoad && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.CanReserveAndReach(transporter.parent, PathEndMode.Touch, pawn.NormalMaxDanger(), 1) && LoadTransportersJobUtility.FindThingToLoad(pawn, transporter) != null;
		}

		public static Job JobOnTransporter(Pawn p, CompTransporter transporter)
		{
			Thing thing = LoadTransportersJobUtility.FindThingToLoad(p, transporter);
			return new Job(JobDefOf.HaulToContainer, thing, transporter.parent)
			{
				count = Mathf.Min(TransferableUtility.TransferableMatching<TransferableOneWay>(thing, transporter.leftToLoad).countToTransfer, thing.stackCount),
				ignoreForbidden = true
			};
		}

		private static Thing FindThingToLoad(Pawn p, CompTransporter transporter)
		{
			LoadTransportersJobUtility.neededThings.Clear();
			List<TransferableOneWay> leftToLoad = transporter.leftToLoad;
			if (leftToLoad != null)
			{
				for (int i = 0; i < leftToLoad.Count; i++)
				{
					TransferableOneWay transferableOneWay = leftToLoad[i];
					if (transferableOneWay.countToTransfer > 0)
					{
						for (int j = 0; j < transferableOneWay.things.Count; j++)
						{
							LoadTransportersJobUtility.neededThings.Add(transferableOneWay.things[j]);
						}
					}
				}
			}
			if (!LoadTransportersJobUtility.neededThings.Any<Thing>())
			{
				return null;
			}
			Predicate<Thing> validator = (Thing x) => LoadTransportersJobUtility.neededThings.Contains(x) && p.CanReserve(x, 1);
			Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
			if (thing == null)
			{
				foreach (Thing current in LoadTransportersJobUtility.neededThings)
				{
					Pawn pawn = current as Pawn;
					if (pawn != null && (!pawn.IsColonist || pawn.Downed) && p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly, 1))
					{
						return pawn;
					}
				}
			}
			LoadTransportersJobUtility.neededThings.Clear();
			return thing;
		}
	}
}
