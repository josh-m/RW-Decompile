using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class PawnUtility
	{
		private const float RecruitDifficultyMin = 0.33f;

		private const float RecruitDifficultyMax = 0.99f;

		private const float RecruitDifficultyRandomOffset = 0.2f;

		private const float RecruitDifficultyOffsetPerTechDiff = 0.15f;

		public static IEnumerable<Pawn> AllPawnsMapOrWorldAliveOrDead
		{
			get
			{
				foreach (Pawn alive in PawnUtility.AllPawnsMapOrWorldAlive)
				{
					yield return alive;
				}
				if (Find.World != null)
				{
					foreach (Pawn p in Find.WorldPawns.AllPawnsDead)
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<Pawn> AllPawnsMapOrWorldAlive
		{
			get
			{
				for (int i = 0; i < PawnGroupMaker.MakingPawns.Count; i++)
				{
					yield return PawnGroupMaker.MakingPawns[i];
				}
				if (Find.World != null)
				{
					foreach (Pawn p in Find.WorldPawns.AllPawnsAlive)
					{
						yield return p;
					}
				}
				if (Current.ProgramState == ProgramState.MapPlaying)
				{
					foreach (Pawn p2 in Find.MapPawns.AllPawns)
					{
						yield return p2;
					}
				}
				else if (Find.GameInitData != null)
				{
					for (int j = 0; j < Find.GameInitData.startingPawns.Count; j++)
					{
						if (Find.GameInitData.startingPawns[j] != null)
						{
							yield return Find.GameInitData.startingPawns[j];
						}
					}
				}
			}
		}

		public static void TryDestroyStartingColonistFamily(Pawn pawn)
		{
			if (!pawn.relations.RelatedPawns.Any((Pawn x) => Find.GameInitData.startingPawns.Contains(x)))
			{
				PawnUtility.DestroyStartingColonistFamily(pawn);
			}
		}

		public static void DestroyStartingColonistFamily(Pawn pawn)
		{
			foreach (Pawn current in pawn.relations.RelatedPawns.ToList<Pawn>())
			{
				if (!Find.GameInitData.startingPawns.Contains(current))
				{
					WorldPawnSituation situation = Find.WorldPawns.GetSituation(current);
					if (situation == WorldPawnSituation.Free || situation == WorldPawnSituation.Dead)
					{
						Find.WorldPawns.RemovePawn(current);
						Find.WorldPawns.PassToWorld(current, PawnDiscardDecideMode.Discard);
					}
				}
			}
		}

		public static bool EnemiesAreNearby(Pawn pawn, int regionsToScan = 9, bool passDoors = false)
		{
			TraverseParms tp = (!passDoors) ? TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false) : TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			bool foundEnemy = false;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, (Region from, Region to) => to.Allows(tp, false), delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].HostileTo(pawn))
						{
							foundEnemy = true;
							return true;
						}
					}
				}
				return foundEnemy;
			}, regionsToScan);
			return foundEnemy;
		}

		public static bool WillSoonHaveBasicNeed(Pawn p)
		{
			return p.needs != null && ((p.needs.rest != null && p.needs.rest.CurLevel < 0.33f) || (p.needs.food != null && p.needs.food.CurLevelPercentage < p.needs.food.PercentageThreshHungry + 0.05f));
		}

		public static float AnimalFilthChancePerCell(ThingDef def, float bodySize)
		{
			float num = bodySize * 0.00125f;
			return num * (1f - def.race.petness);
		}

		public static bool CasualInterruptibleNow(this Pawn p)
		{
			return !p.Drafted && (p.playerSettings == null || p.playerSettings.master == null || !p.playerSettings.master.Drafted) && !p.InAggroMentalState && p.Awake() && (p.CurJob == null || (p.CurJob.def.casualInterruptible && !p.CurJob.playerForced));
		}

		[DebuggerHidden]
		public static IEnumerable<Pawn> SpawnedMasteredPawns(Pawn master)
		{
			if (Current.ProgramState == ProgramState.MapPlaying && master.Faction != null && master.RaceProps.Humanlike)
			{
				List<Pawn> pawns = Find.MapPawns.SpawnedPawnsInFaction(master.Faction);
				for (int i = 0; i < pawns.Count; i++)
				{
					if (pawns[i].playerSettings != null && pawns[i].playerSettings.master == master)
					{
						yield return pawns[i];
					}
				}
			}
		}

		public static bool InValidState(Pawn p)
		{
			return p.health != null && (p.Dead || (p.stances != null && p.mindState != null && p.needs != null && p.ageTracker != null));
		}

		public static PawnPosture GetPosture(this Pawn p)
		{
			if (p.Downed || p.Dead)
			{
				return PawnPosture.LayingAny;
			}
			if (p.jobs == null)
			{
				return PawnPosture.Standing;
			}
			if (p.jobs.curJob == null)
			{
				return PawnPosture.Standing;
			}
			return p.jobs.curDriver.Posture;
		}

		public static void ForceWait(Pawn pawn, int ticks, Thing faceTarget = null)
		{
			Job job = new Job(JobDefOf.Wait, faceTarget);
			job.expiryInterval = ticks;
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true, true, null);
		}

		public static void GiveNameBecauseOfNuzzle(Pawn namer, Pawn namee)
		{
			string text = (namee.Name != null) ? namee.Name.ToStringFull : namee.LabelIndefinite();
			namee.Name = NameGenerator.GeneratePawnName(namee, NameStyle.Full, null);
			if (namer.Faction == Faction.OfPlayer)
			{
				Messages.Message("MessageNuzzledPawnGaveNameTo".Translate(new object[]
				{
					namer,
					text,
					namee.Name.ToStringFull
				}), namee, MessageSound.Standard);
			}
		}

		public static void GainComfortFromCellIfPossible(this Pawn p)
		{
			if (Find.TickManager.TicksGame % 10 == 0)
			{
				Building edifice = p.Position.GetEdifice();
				if (edifice != null)
				{
					float statValue = edifice.GetStatValue(StatDefOf.Comfort, true);
					if (statValue >= 0f && p.needs != null && p.needs.comfort != null)
					{
						p.needs.comfort.ComfortUsed(statValue);
					}
				}
			}
		}

		public static float BodyResourceGrowthSpeed(Pawn pawn)
		{
			if (pawn.needs != null && pawn.needs.food != null)
			{
				switch (pawn.needs.food.CurCategory)
				{
				case HungerCategory.Fed:
					return 1f;
				case HungerCategory.Hungry:
					return 0.666f;
				case HungerCategory.UrgentlyHungry:
					return 0.333f;
				case HungerCategory.Starving:
					return 0f;
				}
			}
			return 1f;
		}

		public static bool FertileMateTarget(Pawn male, Pawn female)
		{
			if (female.gender != Gender.Female || !female.ageTracker.CurLifeStage.reproductive)
			{
				return false;
			}
			CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
			if (compEggLayer != null)
			{
				return !compEggLayer.FullyFertilized;
			}
			return !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant);
		}

		public static void Mated(Pawn male, Pawn female)
		{
			if (!female.ageTracker.CurLifeStage.reproductive)
			{
				return;
			}
			CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
			if (compEggLayer != null)
			{
				compEggLayer.Fertilize(male);
			}
			else if (Rand.Value < 0.5f && !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant))
			{
				Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, female, null);
				hediff_Pregnant.father = male;
				female.health.AddHediff(hediff_Pregnant, null, null);
			}
		}

		public static bool PlayerForcedJobNowOrSoon(Pawn pawn)
		{
			Job curJob = pawn.CurJob;
			return (curJob == null && JobQueueUtility.NextJobIsPlayerForced(pawn)) || (curJob != null && curJob.playerForced);
		}

		public static bool TrySpawnHatchedOrBornPawn(Pawn pawn, Thing motherOrEgg)
		{
			if (motherOrEgg.Spawned)
			{
				return GenSpawn.Spawn(pawn, motherOrEgg.Position) != null;
			}
			Pawn p = motherOrEgg as Pawn;
			if (p.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Keep);
				return true;
			}
			if (motherOrEgg.holder != null && motherOrEgg.holder.owner.Spawned)
			{
				GenSpawn.Spawn(pawn, motherOrEgg.holder.owner.GetPosition());
				return true;
			}
			return false;
		}

		public static ByteGrid GetAvoidGrid(this Pawn p)
		{
			if (p.Faction == null)
			{
				return null;
			}
			if (!p.Faction.def.canUseAvoidGrid)
			{
				return null;
			}
			if (p.Faction == Faction.OfPlayer || !p.Faction.RelationWith(Faction.OfPlayer, false).hostile)
			{
				return null;
			}
			Lord lord = p.GetLord();
			if (lord != null)
			{
				if (lord.CurLordToil.avoidGridMode == AvoidGridMode.Ignore)
				{
					return null;
				}
				if (lord.CurLordToil.avoidGridMode == AvoidGridMode.Basic)
				{
					return p.Faction.avoidGridBasic;
				}
				if (lord.CurLordToil.avoidGridMode == AvoidGridMode.Smart)
				{
					return p.Faction.avoidGridSmart;
				}
			}
			return p.Faction.avoidGridBasic;
		}

		public static bool ShouldCollideWithPawns(Pawn p)
		{
			return !p.Downed && !p.Dead && p.mindState.anyCloseHostilesRecently;
		}

		public static bool AnyPawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false)
		{
			List<Thing> thingList = c.GetThingList();
			if (thingList.Count == 0)
			{
				return false;
			}
			bool flag = false;
			if (actAsIfHadCollideWithPawnsJob)
			{
				flag = true;
			}
			else
			{
				Job curJob = forPawn.CurJob;
				if (curJob != null && curJob.def.collideWithPawns)
				{
					flag = true;
				}
				else if (forPawn.Drafted)
				{
					flag = true;
				}
			}
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null)
				{
					if (collideOnlyWithStandingPawns)
					{
						if (pawn.pather.MovingNow)
						{
							goto IL_122;
						}
						if (pawn.pather.Moving && pawn.pather.MovedRecently(60))
						{
							goto IL_122;
						}
					}
					if (pawn != forPawn && !pawn.Downed)
					{
						if (!PawnUtility.PawnsCanShareCellBecauseOfBodySize(pawn, forPawn))
						{
							if (pawn.HostileTo(forPawn))
							{
								return true;
							}
							if (flag)
							{
								Job curJob2 = pawn.CurJob;
								if (curJob2 != null && curJob2.def.collideWithPawns)
								{
									return true;
								}
							}
						}
					}
				}
				IL_122:;
			}
			return false;
		}

		private static bool PawnsCanShareCellBecauseOfBodySize(Pawn p1, Pawn p2)
		{
			if (p1.BodySize >= 1.5f || p2.BodySize >= 1.5f)
			{
				return false;
			}
			float num = p1.BodySize / p2.BodySize;
			if (num < 1f)
			{
				num = 1f / num;
			}
			return num > 3.57f;
		}

		public static bool ShouldSendNotificationAbout(Pawn p)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return false;
			}
			if (PawnGenerator.IsBeingGenerated(p))
			{
				return false;
			}
			if (p.IsWorldPawn() && p.corpse == null)
			{
				return false;
			}
			if (p.Faction != Faction.OfPlayer)
			{
				if (p.HostFaction != Faction.OfPlayer)
				{
					return false;
				}
				if (p.RaceProps.Humanlike && p.guest.released && !p.Downed && !p.InBed())
				{
					return false;
				}
				if (p.CurJob != null && p.CurJob.exitMapOnArrival && !PrisonBreakUtility.IsPrisonBreaking(p))
				{
					return false;
				}
			}
			return true;
		}

		public static LocomotionUrgency ResolveLocomotion(Pawn pawn, LocomotionUrgency secondPriority)
		{
			if (!pawn.Dead && pawn.mindState.duty != null && pawn.mindState.duty.locomotion != LocomotionUrgency.None)
			{
				return pawn.mindState.duty.locomotion;
			}
			return secondPriority;
		}

		public static LocomotionUrgency ResolveLocomotion(Pawn pawn, LocomotionUrgency secondPriority, LocomotionUrgency thirdPriority)
		{
			LocomotionUrgency locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, secondPriority);
			if (locomotionUrgency != LocomotionUrgency.None)
			{
				return locomotionUrgency;
			}
			return thirdPriority;
		}

		public static Danger ResolveMaxDanger(Pawn pawn, Danger secondPriority)
		{
			if (!pawn.Dead && pawn.mindState.duty != null && pawn.mindState.duty.maxDanger != Danger.Unspecified)
			{
				return pawn.mindState.duty.maxDanger;
			}
			return secondPriority;
		}

		public static Danger ResolveMaxDanger(Pawn pawn, Danger secondPriority, Danger thirdPriority)
		{
			Danger danger = PawnUtility.ResolveMaxDanger(pawn, secondPriority);
			if (danger != Danger.Unspecified)
			{
				return danger;
			}
			return thirdPriority;
		}

		public static float RecruitDifficulty(this Pawn pawn, Faction recruiterFaction, bool withPopIntent)
		{
			Rand.PushSeed();
			Rand.Seed = pawn.HashOffset();
			float num = pawn.kindDef.baseRecruitDifficulty;
			num += Rand.Range(-0.2f, 0.2f);
			int num2 = Mathf.Abs((int)(pawn.Faction.def.techLevel - recruiterFaction.def.techLevel));
			num += (float)num2 * 0.15f;
			if (withPopIntent)
			{
				float popIntent = (Current.ProgramState != ProgramState.MapPlaying) ? 1f : Find.Storyteller.intenderPopulation.PopulationIntent;
				num = PawnUtility.PopIntentAdjustedRecruitDifficulty(num, popIntent);
			}
			num = Mathf.Clamp(num, 0.33f, 0.99f);
			Rand.PopSeed();
			return num;
		}

		private static float PopIntentAdjustedRecruitDifficulty(float baseDifficulty, float popIntent)
		{
			float num = Mathf.Clamp(popIntent, 0.25f, 3f);
			return 1f - (1f - baseDifficulty) * num;
		}

		public static void DoTable_PopIntentRecruitDifficulty()
		{
			List<float> list = new List<float>();
			for (float num = -1f; num < 3f; num += 0.1f)
			{
				list.Add(num);
			}
			List<float> colValues = new List<float>
			{
				0.1f,
				0.2f,
				0.3f,
				0.4f,
				0.5f,
				0.6f,
				0.7f,
				0.8f,
				0.9f,
				0.95f,
				0.99f
			};
			DebugTables.MakeTablesDialog<float, float>(colValues, (float d) => "d=" + d.ToString("F0"), list, (float rv) => rv.ToString("F1"), (float d, float pi) => PawnUtility.PopIntentAdjustedRecruitDifficulty(d, pi).ToStringPercent(), "intents");
		}
	}
}
