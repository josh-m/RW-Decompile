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

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil to = new Toil();
			to.initAction = delegate
			{
				this.<>f__this.ticksLeft = Rand.Range(300, 900);
				int num = 0;
				IntVec3 c;
				while (true)
				{
					c = this.<>f__this.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
					num++;
					if (num > 12)
					{
						break;
					}
					if (c.InBounds(this.<>f__this.pawn.Map) && c.Standable(this.<>f__this.pawn.Map))
					{
						goto IL_A1;
					}
				}
				c = this.<>f__this.pawn.Position;
				IL_A1:
				this.<>f__this.pawn.CurJob.targetA = c;
				this.<>f__this.pawn.Drawer.rotator.FaceCell(c);
				this.<>f__this.pawn.pather.StopDead();
			};
			to.tickAction = delegate
			{
				if (this.<>f__this.ticksLeft % 150 == 149)
				{
					FilthMaker.MakeFilth(this.<>f__this.pawn.CurJob.targetA.Cell, this.<>f__this.Map, ThingDefOf.FilthVomit, this.<>f__this.pawn.LabelIndefinite(), 1);
					if (this.<>f__this.pawn.needs.food.CurLevelPercentage > 0.1f)
					{
						this.<>f__this.pawn.needs.food.CurLevel -= this.<>f__this.pawn.needs.food.MaxLevel * 0.04f;
					}
				}
				this.<>f__this.ticksLeft--;
				if (this.<>f__this.ticksLeft <= 0)
				{
					this.<>f__this.ReadyForNextToil();
					TaleRecorder.RecordTale(TaleDefOf.Vomited, new object[]
					{
						this.<>f__this.pawn
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
