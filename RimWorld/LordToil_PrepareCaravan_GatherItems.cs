using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_GatherItems : LordToil
	{
		private IntVec3 meetingPoint;

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_PrepareCaravan_GatherItems(IntVec3 meetingPoint)
		{
			this.meetingPoint = meetingPoint;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				if (pawn.IsColonist)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_GatherItems);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, this.meetingPoint, -1f);
				}
			}
		}

		public override void LordToilTick()
		{
			base.LordToilTick();
			if (Find.TickManager.TicksGame % 120 == 0)
			{
				bool flag = true;
				for (int i = 0; i < this.lord.ownedPawns.Count; i++)
				{
					Pawn pawn = this.lord.ownedPawns[i];
					if (pawn.IsColonist && pawn.mindState.lastJobTag != JobTag.AllCaravanItemsGathered)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					this.lord.ReceiveMemo("AllItemsGathered");
				}
			}
		}
	}
}
