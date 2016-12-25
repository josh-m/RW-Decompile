using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_MarryAdjacentPawn : JobDriver
	{
		private const TargetIndex OtherFianceInd = TargetIndex.A;

		private const int Duration = 2500;

		private int ticksLeftToMarry = 2500;

		private Pawn OtherFiance
		{
			get
			{
				return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
			}
		}

		public int TicksLeftToMarry
		{
			get
			{
				return this.ticksLeftToMarry;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => this.<>f__this.OtherFiance.Drafted || !this.<>f__this.pawn.Position.IsAdjacentTo8WayOrInside(this.<>f__this.OtherFiance.Position, this.<>f__this.OtherFiance.Rotation, this.<>f__this.OtherFiance.def.size));
			Toil marry = new Toil();
			marry.initAction = delegate
			{
				this.<>f__this.ticksLeftToMarry = 2500;
			};
			marry.tickAction = delegate
			{
				this.<>f__this.ticksLeftToMarry--;
				if (this.<>f__this.ticksLeftToMarry <= 0)
				{
					this.<>f__this.ticksLeftToMarry = 0;
					this.<>f__this.ReadyForNextToil();
				}
			};
			marry.defaultCompleteMode = ToilCompleteMode.Never;
			marry.FailOn(() => !this.<>f__this.pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, this.<>f__this.OtherFiance));
			yield return marry;
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Instant,
				initAction = delegate
				{
					if (this.<>f__this.pawn.thingIDNumber < this.<>f__this.OtherFiance.thingIDNumber)
					{
						MarriageCeremonyUtility.Married(this.<>f__this.pawn, this.<>f__this.OtherFiance);
					}
				}
			};
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksLeftToMarry, "ticksLeftToMarry", 0, false);
		}
	}
}
