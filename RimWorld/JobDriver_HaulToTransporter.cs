using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_HaulToTransporter : JobDriver_HaulToContainer
	{
		public int initialCount;

		public CompTransporter Transporter
		{
			get
			{
				return (base.Container == null) ? null : base.Container.TryGetComp<CompTransporter>();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.initialCount, "initialCount", 0, false);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			ThingCount thingCount = LoadTransportersJobUtility.FindThingToLoad(this.pawn, base.Container.TryGetComp<CompTransporter>());
			this.job.targetA = thingCount.Thing;
			this.job.count = thingCount.Count;
			this.initialCount = thingCount.Count;
			this.pawn.Reserve(thingCount.Thing, this.job, 1, -1, null, true);
		}
	}
}
