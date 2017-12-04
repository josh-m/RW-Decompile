using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Resurrect : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex ItemInd = TargetIndex.B;

		private const int DurationTicks = 600;

		private Corpse Corpse
		{
			get
			{
				return (Corpse)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Thing Item
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.Corpse, this.job, 1, -1, null) && this.pawn.Reserve(this.Item, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil prepare = Toils_General.Wait(600);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedOrNull(TargetIndex.A);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return prepare;
			yield return Toils_General.Do(new Action(this.Resurrect));
		}

		private void Resurrect()
		{
			Pawn innerPawn = this.Corpse.InnerPawn;
			ResurrectionUtility.ResurrectWithSideEffects(innerPawn);
			Messages.Message("MessagePawnResurrected".Translate(new object[]
			{
				innerPawn.LabelIndefinite()
			}).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent);
			this.Item.SplitOff(1).Destroy(DestroyMode.Vanish);
		}
	}
}
