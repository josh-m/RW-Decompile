using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_MarryAdjacentPawn : JobDriver
	{
		private int ticksLeftToMarry = 2500;

		private const TargetIndex OtherFianceInd = TargetIndex.A;

		private const int Duration = 2500;

		private Pawn OtherFiance
		{
			get
			{
				return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		public int TicksLeftToMarry
		{
			get
			{
				return this.ticksLeftToMarry;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => this.$this.OtherFiance.Drafted || !this.$this.pawn.Position.AdjacentTo8WayOrInside(this.$this.OtherFiance));
			Toil marry = new Toil();
			marry.initAction = delegate
			{
				this.$this.ticksLeftToMarry = 2500;
			};
			marry.tickAction = delegate
			{
				this.$this.ticksLeftToMarry--;
				if (this.$this.ticksLeftToMarry <= 0)
				{
					this.$this.ticksLeftToMarry = 0;
					this.$this.ReadyForNextToil();
				}
			};
			marry.defaultCompleteMode = ToilCompleteMode.Never;
			marry.FailOn(() => !this.$this.pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, this.$this.OtherFiance));
			yield return marry;
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Instant,
				initAction = delegate
				{
					if (this.$this.pawn.thingIDNumber < this.$this.OtherFiance.thingIDNumber)
					{
						MarriageCeremonyUtility.Married(this.$this.pawn, this.$this.OtherFiance);
					}
				}
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeftToMarry, "ticksLeftToMarry", 0, false);
		}
	}
}
