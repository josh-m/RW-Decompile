using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_EnterCryptosleepCasket : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil prepare = Toils_General.Wait(500);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return prepare;
			Toil enter = new Toil();
			enter.initAction = delegate
			{
				Pawn actor = enter.actor;
				Building_CryptosleepCasket pod = (Building_CryptosleepCasket)actor.CurJob.targetA.Thing;
				Action action = delegate
				{
					actor.DeSpawn();
					pod.TryAcceptThing(actor, true);
				};
				if (!pod.def.building.isPlayerEjectable)
				{
					int freeColonistsSpawnedOrInPlayerEjectablePodsCount = this.$this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
					if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate().AdjustedFor(actor), action, false, null));
					}
					else
					{
						action();
					}
				}
				else
				{
					action();
				}
			};
			enter.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return enter;
		}
	}
}
