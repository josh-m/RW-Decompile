using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InPrivateRoom : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.ownership == null)
			{
				return null;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return null;
			}
			IntVec3 vec;
			if (!(from c in ownedRoom.Cells
			where c.Standable() && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1)
			select c).TryRandomElement(out vec))
			{
				return null;
			}
			return new Job(this.def.jobDef, vec);
		}
	}
}
