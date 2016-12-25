using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_ManTurret : JobDriver
	{
		private const float ShellSearchRadius = 40f;

		private static bool GunNeedsLoading(Building b)
		{
			Building_TurretGun building_TurretGun = b as Building_TurretGun;
			return building_TurretGun != null && building_TurretGun.def.building.turretShellDef != null && !building_TurretGun.loaded;
		}

		public static Thing FindAmmoForTurret(Pawn pawn, Thing gun)
		{
			Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 1);
			return GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForDef(gun.def.building.turretShellDef), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 40f, validator, null, -1, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			Toil gotoTurret = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<loadIfNeeded>__1.actor;
					Building building = (Building)actor.CurJob.targetA.Thing;
					Building_TurretGun building_TurretGun = building as Building_TurretGun;
					if (!JobDriver_ManTurret.GunNeedsLoading(building))
					{
						this.<>f__this.JumpToToil(this.<gotoTurret>__0);
						return;
					}
					Thing thing = JobDriver_ManTurret.FindAmmoForTurret(this.<>f__this.pawn, building_TurretGun);
					if (thing == null)
					{
						if (actor.Faction == Faction.OfPlayer)
						{
							Messages.Message("MessageOutOfNearbyShellsFor".Translate(new object[]
							{
								actor.LabelShort,
								building_TurretGun.Label
							}).CapitalizeFirst(), building_TurretGun, MessageSound.Negative);
						}
						actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					}
					actor.CurJob.targetB = thing;
					actor.CurJob.count = 1;
				}
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B, 25);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate
				{
					Pawn actor = this.<loadIfNeeded>__1.actor;
					Building building = (Building)actor.CurJob.targetA.Thing;
					Building_TurretGun building_TurretGun = building as Building_TurretGun;
					SoundDefOf.ArtilleryShellLoaded.PlayOneShot(new TargetInfo(building_TurretGun.Position, building_TurretGun.Map, false));
					building_TurretGun.loaded = true;
					actor.carryTracker.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
				}
			};
			yield return gotoTurret;
			yield return new Toil
			{
				tickAction = delegate
				{
					Pawn actor = this.<man>__3.actor;
					Building building = (Building)actor.CurJob.targetA.Thing;
					if (JobDriver_ManTurret.GunNeedsLoading(building))
					{
						this.<>f__this.JumpToToil(this.<loadIfNeeded>__1);
						return;
					}
					building.GetComp<CompMannable>().ManForATick(actor);
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}
	}
}
