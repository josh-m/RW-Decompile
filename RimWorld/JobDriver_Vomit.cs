using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Vomit : JobDriver
	{
		private int ticksLeft;

		private PawnPosture lastPosture;

		public override PawnPosture Posture
		{
			get
			{
				return this.lastPosture;
			}
		}

		public override void Notify_LastPosture(PawnPosture posture, LayingDownState layingDown)
		{
			this.lastPosture = posture;
			this.layingDown = layingDown;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
			Scribe_Values.Look<PawnPosture>(ref this.lastPosture, "lastPosture", PawnPosture.Standing, false);
		}

		public override bool TryMakePreToilReservations()
		{
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil to = new Toil();
			to.initAction = delegate
			{
				this.$this.ticksLeft = Rand.Range(300, 900);
				int num = 0;
				IntVec3 c;
				while (true)
				{
					c = this.$this.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
					num++;
					if (num > 12)
					{
						break;
					}
					if (c.InBounds(this.$this.pawn.Map) && c.Standable(this.$this.pawn.Map))
					{
						goto IL_A1;
					}
				}
				c = this.$this.pawn.Position;
				IL_A1:
				this.$this.job.targetA = c;
				this.$this.pawn.pather.StopDead();
			};
			to.tickAction = delegate
			{
				if (this.$this.ticksLeft % 150 == 149)
				{
					FilthMaker.MakeFilth(this.$this.job.targetA.Cell, this.$this.Map, ThingDefOf.FilthVomit, this.$this.pawn.LabelIndefinite(), 1);
					if (this.$this.pawn.needs.food.CurLevelPercentage > 0.1f)
					{
						this.$this.pawn.needs.food.CurLevel -= this.$this.pawn.needs.food.MaxLevel * 0.04f;
					}
				}
				this.$this.ticksLeft--;
				if (this.$this.ticksLeft <= 0)
				{
					this.$this.ReadyForNextToil();
					TaleRecorder.RecordTale(TaleDefOf.Vomited, new object[]
					{
						this.$this.pawn
					});
				}
			};
			to.defaultCompleteMode = ToilCompleteMode.Never;
			to.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
			to.PlaySustainerOrSound(() => SoundDef.Named("Vomit"));
			yield return to;
		}
	}
}
