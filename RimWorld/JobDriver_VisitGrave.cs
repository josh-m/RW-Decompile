using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_VisitGrave : JobDriver_VisitJoyThing
	{
		private Building_Grave Grave
		{
			get
			{
				return (Building_Grave)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected override Action GetWaitTickAction()
		{
			return delegate
			{
				float num = this.Grave.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				Room room = this.pawn.GetRoom();
				if (room != null)
				{
					num *= room.GetStat(RoomStatDefOf.GraveVisitingJoyGainFactor);
				}
				this.pawn.GainComfortFromCellIfPossible();
				float extraJoyGainFactor = num;
				JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
		}
	}
}
