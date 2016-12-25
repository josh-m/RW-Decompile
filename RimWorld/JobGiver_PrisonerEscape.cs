using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PrisonerEscape : ThinkNode_JobGiver
	{
		private const int MaxRegionsToCheckWhenEscapingThroughOpenDoors = 25;

		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 vec;
			if (this.ShouldStartEscaping(pawn) && RCellFinder.TryFindBestExitSpot(pawn, out vec, TraverseMode.ByPawn))
			{
				if (!pawn.guest.released)
				{
					Messages.Message("MessagePrisonerIsEscaping".Translate(new object[]
					{
						pawn.NameStringShort
					}), pawn, MessageSound.SeriousAlert);
				}
				return new Job(JobDefOf.Goto, vec)
				{
					exitMapOnArrival = true
				};
			}
			return null;
		}

		private bool ShouldStartEscaping(Pawn pawn)
		{
			if (!pawn.guest.IsPrisoner || pawn.guest.HostFaction != Faction.OfPlayer || !pawn.guest.PrisonerIsSecure)
			{
				return false;
			}
			Room room = RoomQuery.RoomAt(pawn.Position);
			if (room.TouchesMapEdge)
			{
				return true;
			}
			bool found = false;
			RegionTraverser.BreadthFirstTraverse(room.Regions[0], (Region from, Region reg) => reg.portal == null || reg.portal.FreePassage, delegate(Region reg)
			{
				if (reg.Room.TouchesMapEdge)
				{
					found = true;
					return true;
				}
				return false;
			}, 25);
			return found;
		}
	}
}
