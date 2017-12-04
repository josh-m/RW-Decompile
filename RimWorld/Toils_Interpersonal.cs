using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Interpersonal
	{
		public static Toil GotoInteractablePosition(TargetIndex target)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)((Thing)actor.CurJob.GetTarget(target));
				if (InteractionUtility.IsGoodPositionForInteraction(actor, pawn))
				{
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else
				{
					actor.pather.StartPath(pawn, PathEndMode.Touch);
				}
			};
			toil.tickAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)((Thing)actor.CurJob.GetTarget(target));
				Map map = actor.Map;
				if (InteractionUtility.IsGoodPositionForInteraction(actor, pawn) && actor.Position.InHorDistOf(pawn.Position, (float)Mathf.CeilToInt(3f)) && (!actor.pather.Moving || actor.pather.nextCell.GetDoor(map) == null))
				{
					actor.pather.StopDead();
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else if (!actor.pather.Moving)
				{
					IntVec3 intVec = IntVec3.Invalid;
					for (int i = 0; i < 8; i++)
					{
						IntVec3 intVec2 = pawn.Position + GenAdj.AdjacentCells[i];
						if (intVec2.InBounds(map) && intVec2.Standable(map) && intVec2 != actor.Position && InteractionUtility.IsGoodPositionForInteraction(intVec2, pawn.Position, map) && actor.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn) && (!intVec.IsValid || actor.Position.DistanceToSquared(intVec2) < actor.Position.DistanceToSquared(intVec)))
						{
							intVec = intVec2;
						}
					}
					if (intVec.IsValid)
					{
						actor.pather.StartPath(intVec, PathEndMode.OnCell);
					}
					else
					{
						actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					}
				}
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			return toil;
		}

		public static Toil GotoPrisoner(Pawn pawn, Pawn talkee, PrisonerInteractionModeDef mode)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				pawn.pather.StartPath(talkee, PathEndMode.Touch);
			};
			toil.AddFailCondition(() => talkee.DestroyedOrNull() || (mode != PrisonerInteractionModeDefOf.Execution && !talkee.Awake()) || !talkee.IsPrisonerOfColony || (talkee.guest == null || talkee.guest.interactionMode != mode));
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil WaitToBeAbleToInteract(Pawn pawn)
		{
			return new Toil
			{
				initAction = delegate
				{
					if (!pawn.interactions.InteractedTooRecentlyToInteract())
					{
						pawn.jobs.curDriver.ReadyForNextToil();
					}
				},
				tickAction = delegate
				{
					if (!pawn.interactions.InteractedTooRecentlyToInteract())
					{
						pawn.jobs.curDriver.ReadyForNextToil();
					}
				},
				socialMode = RandomSocialMode.Off,
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}

		public static Toil ConvinceRecruitee(Pawn pawn, Pawn talkee)
		{
			return new Toil
			{
				initAction = delegate
				{
					if (!pawn.interactions.TryInteractWith(talkee, InteractionDefOf.BuildRapport))
					{
						pawn.jobs.curDriver.ReadyForNextToil();
					}
					else
					{
						pawn.records.Increment(RecordDefOf.PrisonersChatted);
					}
				},
				socialMode = RandomSocialMode.Off,
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 350
			};
		}

		public static Toil SetLastInteractTime(TargetIndex targetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn pawn = (Pawn)toil.actor.jobs.curJob.GetTarget(targetInd).Thing;
				pawn.mindState.lastAssignedInteractTime = Find.TickManager.TicksGame;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		public static Toil TryRecruit(TargetIndex recruiteeInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(recruiteeInd).Thing;
				if (!pawn.Spawned || !pawn.Awake())
				{
					return;
				}
				InteractionDef intDef = (!pawn.AnimalOrWildMan()) ? InteractionDefOf.RecruitAttempt : InteractionDefOf.TameAttempt;
				actor.interactions.TryInteractWith(pawn, intDef);
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 350;
			return toil;
		}

		public static Toil TryTrain(TargetIndex traineeInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(traineeInd).Thing;
				if (pawn.Spawned && pawn.Awake() && actor.interactions.TryInteractWith(pawn, InteractionDefOf.TrainAttempt))
				{
					float num = actor.GetStatValue(StatDefOf.TrainAnimalChance, true);
					num *= Mathf.Max(0.05f, GenMath.LerpDouble(0f, 1f, 2f, 0f, pawn.RaceProps.wildness));
					if (actor.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn))
					{
						num *= 1.5f;
					}
					num = Mathf.Clamp01(num);
					TrainableDef trainableDef = pawn.training.NextTrainableToTrain();
					string text;
					if (Rand.Value < num)
					{
						pawn.training.Train(trainableDef, actor);
						if (pawn.caller != null)
						{
							pawn.caller.DoCall();
						}
						text = "TextMote_TrainSuccess".Translate(new object[]
						{
							trainableDef.LabelCap,
							num.ToStringPercent()
						});
						RelationsUtility.TryDevelopBondRelation(actor, pawn, 0.007f);
						TaleRecorder.RecordTale(TaleDefOf.TrainedAnimal, new object[]
						{
							actor,
							pawn,
							trainableDef
						});
					}
					else
					{
						text = "TextMote_TrainFail".Translate(new object[]
						{
							trainableDef.LabelCap,
							num.ToStringPercent()
						});
					}
					string text2 = text;
					text = string.Concat(new object[]
					{
						text2,
						"\n",
						pawn.training.GetSteps(trainableDef),
						" / ",
						trainableDef.steps
					});
					MoteMaker.ThrowText((actor.DrawPos + pawn.DrawPos) / 2f, actor.Map, text, 5f);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 100;
			return toil;
		}

		public static Toil Interact(TargetIndex otherPawnInd, InteractionDef interaction)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(otherPawnInd).Thing;
				if (!pawn.Spawned)
				{
					return;
				}
				actor.interactions.TryInteractWith(pawn, interaction);
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 60;
			return toil;
		}
	}
}
