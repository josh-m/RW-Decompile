using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ViewArt : JobDriver_VisitJoyThing
	{
		private Thing ArtThing
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected override Action GetWaitTickAction()
		{
			return delegate
			{
				float num = this.ArtThing.GetStatValue(StatDefOf.EntertainmentStrengthFactor, true);
				float num2 = this.ArtThing.GetStatValue(StatDefOf.Beauty, true) / this.ArtThing.def.GetStatValueAbstract(StatDefOf.Beauty, null);
				num *= ((num2 <= 0f) ? 0f : num2);
				this.pawn.GainComfortFromCellIfPossible();
				Pawn pawn = this.pawn;
				float extraJoyGainFactor = num;
				JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor);
			};
		}
	}
}
