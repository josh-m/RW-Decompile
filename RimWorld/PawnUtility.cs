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
		private const float HumanFilthFactor = 4f;

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<string> tmpPawnKindsStr = new List<string>();

		private static HashSet<PawnKindDef> tmpAddedPawnKinds = new HashSet<PawnKindDef>();

		private static List<PawnKindDef> tmpPawnKinds = new List<PawnKindDef>();

		private const float RecruitDifficultyMin = 0.1f;

		private const float RecruitDifficultyMax = 0.99f;

		private const float RecruitDifficultyGaussianWidthFactor = 0.15f;

		private const float RecruitDifficultyOffsetPerTechDiff = 0.16f;

		private static List<Thing> tmpThings = new List<Thing>();

		public static bool IsFactionLeader(Pawn pawn)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].leader == pawn)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsKidnappedPawn(Pawn pawn)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading.Contains(pawn))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsTravelingInTransportPodWorldObject(Pawn pawn)
		{
			return pawn.IsWorldPawn() && ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(pawn);
		}

		public static bool ForSaleBySettlement(Pawn pawn)
		{
			return pawn.ParentHolder is SettlementBase_TraderTracker;
		}

		public static void TryDestroyStartingColonistFamily(Pawn pawn)
		{
			if (!pawn.relations.RelatedPawns.Any((Pawn x) => Find.GameInitData.startingAndOptionalPawns.Contains(x)))
			{
				PawnUtility.DestroyStartingColonistFamily(pawn);
			}
		}

		public static void DestroyStartingColonistFamily(Pawn pawn)
		{
			foreach (Pawn current in pawn.relations.RelatedPawns.ToList<Pawn>())
			{
				if (!Find.GameInitData.startingAndOptionalPawns.Contains(current))
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
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, false), delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].HostileTo(pawn))
					{
						foundEnemy = true;
						return true;
					}
				}
				return foundEnemy;
			}, regionsToScan, RegionType.Set_Passable);
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

		public static float HumanFilthChancePerCell(ThingDef def, float bodySize)
		{
			float num = bodySize * 0.00125f;
			return num * 4f;
		}

		public static bool CanCasuallyInteractNow(this Pawn p, bool twoWayInteraction = false)
		{
			if (p.Drafted)
			{
				return false;
			}
			if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(p))
			{
				return false;
			}
			if (p.InAggroMentalState)
			{
				return false;
			}
			if (!p.Awake())
			{
				return false;
			}
			if (p.IsFormingCaravan())
			{
				return false;
			}
			Job curJob = p.CurJob;
			return curJob == null || !twoWayInteraction || (curJob.def.casualInterruptible && curJob.playerForced);
		}

		[DebuggerHidden]
		public static IEnumerable<Pawn> SpawnedMasteredPawns(Pawn master)
		{
			if (Current.ProgramState == ProgramState.Playing && master.Faction != null && master.RaceProps.Humanlike)
			{
				if (master.Spawned)
				{
					List<Pawn> pawns = master.Map.mapPawns.SpawnedPawnsInFaction(master.Faction);
					for (int i = 0; i < pawns.Count; i++)
					{
						if (pawns[i].playerSettings != null && pawns[i].playerSettings.Master == master)
						{
							yield return pawns[i];
						}
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
			if (p.Dead)
			{
				return PawnPosture.LayingOnGroundNormal;
			}
			if (p.Downed)
			{
				if (p.jobs != null && p.jobs.posture.Laying())
				{
					return p.jobs.posture;
				}
				return PawnPosture.LayingOnGroundNormal;
			}
			else
			{
				if (p.jobs == null)
				{
					return PawnPosture.Standing;
				}
				return p.jobs.posture;
			}
		}

		public static void ForceWait(Pawn pawn, int ticks, Thing faceTarget = null, bool maintainPosture = false)
		{
			if (ticks <= 0)
			{
				Log.ErrorOnce("Forcing a wait for zero ticks", 47045639, false);
			}
			Job job = new Job((!maintainPosture) ? JobDefOf.Wait : JobDefOf.Wait_MaintainPosture, faceTarget);
			job.expiryInterval = ticks;
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true, true, null, null, false);
		}

		public static void GiveNameBecauseOfNuzzle(Pawn namer, Pawn namee)
		{
			string value = (namee.Name != null) ? namee.Name.ToStringFull : namee.LabelIndefinite();
			namee.Name = PawnBioAndNameGenerator.GeneratePawnName(namee, NameStyle.Full, null);
			if (namer.Faction == Faction.OfPlayer)
			{
				Messages.Message("MessageNuzzledPawnGaveNameTo".Translate(namer.Named("NAMER"), value, namee.Name.ToStringFull, namee.Named("NAMEE")), namee, MessageTypeDefOf.NeutralEvent, true);
			}
		}

		public static void GainComfortFromCellIfPossible(this Pawn p)
		{
			if (Find.TickManager.TicksGame % 10 == 0)
			{
				Building edifice = p.Position.GetEdifice(p.Map);
				if (edifice != null)
				{
					PawnUtility.GainComfortFromThingIfPossible(p, edifice);
				}
			}
		}

		public static void GainComfortFromThingIfPossible(Pawn p, Thing from)
		{
			if (Find.TickManager.TicksGame % 10 == 0)
			{
				float statValue = from.GetStatValue(StatDefOf.Comfort, true);
				if (statValue >= 0f && p.needs != null && p.needs.comfort != null)
				{
					p.needs.comfort.ComfortUsed(statValue);
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
			return !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant, false);
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
			else if (Rand.Value < 0.5f && !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant, false))
			{
				Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, female, null);
				hediff_Pregnant.father = male;
				female.health.AddHediff(hediff_Pregnant, null, null, null);
			}
		}

		public static bool PlayerForcedJobNowOrSoon(Pawn pawn)
		{
			if (pawn.jobs == null)
			{
				return false;
			}
			Job curJob = pawn.CurJob;
			if (curJob != null)
			{
				return curJob.playerForced;
			}
			return pawn.jobs.jobQueue.Any<QueuedJob>() && pawn.jobs.jobQueue.Peek().job.playerForced;
		}

		public static bool TrySpawnHatchedOrBornPawn(Pawn pawn, Thing motherOrEgg)
		{
			if (motherOrEgg.SpawnedOrAnyParentSpawned)
			{
				return GenSpawn.Spawn(pawn, motherOrEgg.PositionHeld, motherOrEgg.MapHeld, WipeMode.Vanish) != null;
			}
			Pawn pawn2 = motherOrEgg as Pawn;
			if (pawn2 != null)
			{
				if (pawn2.IsCaravanMember())
				{
					pawn2.GetCaravan().AddPawn(pawn, true);
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					return true;
				}
				if (pawn2.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					return true;
				}
			}
			else if (motherOrEgg.ParentHolder != null)
			{
				Pawn_InventoryTracker pawn_InventoryTracker = motherOrEgg.ParentHolder as Pawn_InventoryTracker;
				if (pawn_InventoryTracker != null)
				{
					if (pawn_InventoryTracker.pawn.IsCaravanMember())
					{
						pawn_InventoryTracker.pawn.GetCaravan().AddPawn(pawn, true);
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
						return true;
					}
					if (pawn_InventoryTracker.pawn.IsWorldPawn())
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
						return true;
					}
				}
			}
			return false;
		}

		public static ByteGrid GetAvoidGrid(this Pawn p, bool onlyIfLordAllows = true)
		{
			if (!p.Spawned)
			{
				return null;
			}
			if (p.Faction == null)
			{
				return null;
			}
			if (!p.Faction.def.canUseAvoidGrid)
			{
				return null;
			}
			if (p.Faction == Faction.OfPlayer || !p.Faction.HostileTo(Faction.OfPlayer))
			{
				return null;
			}
			if (!onlyIfLordAllows)
			{
				return p.Map.avoidGrid.Grid;
			}
			Lord lord = p.GetLord();
			if (lord != null && lord.CurLordToil.useAvoidGrid)
			{
				return lord.Map.avoidGrid.Grid;
			}
			return null;
		}

		public static bool ShouldCollideWithPawns(Pawn p)
		{
			return !p.Downed && !p.Dead && p.mindState.anyCloseHostilesRecently;
		}

		public static bool AnyPawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false, bool forPathFinder = false)
		{
			return PawnUtility.PawnBlockingPathAt(c, forPawn, actAsIfHadCollideWithPawnsJob, collideOnlyWithStandingPawns, forPathFinder) != null;
		}

		public static Pawn PawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false, bool forPathFinder = false)
		{
			List<Thing> thingList = c.GetThingList(forPawn.Map);
			if (thingList.Count == 0)
			{
				return null;
			}
			bool flag = false;
			if (actAsIfHadCollideWithPawnsJob)
			{
				flag = true;
			}
			else
			{
				Job curJob = forPawn.CurJob;
				if (curJob != null && (curJob.collideWithPawns || curJob.def.collideWithPawns || forPawn.jobs.curDriver.collideWithPawns))
				{
					flag = true;
				}
				else if (!forPawn.Drafted || forPawn.pather.Moving)
				{
				}
			}
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null && pawn != forPawn && !pawn.Downed)
				{
					if (collideOnlyWithStandingPawns)
					{
						if (pawn.pather.MovingNow)
						{
							goto IL_19D;
						}
						if (pawn.pather.Moving && pawn.pather.MovedRecently(60))
						{
							goto IL_19D;
						}
					}
					if (!PawnUtility.PawnsCanShareCellBecauseOfBodySize(pawn, forPawn))
					{
						if (pawn.HostileTo(forPawn))
						{
							return pawn;
						}
						if (flag)
						{
							if (forPathFinder || !forPawn.Drafted || !pawn.RaceProps.Animal)
							{
								Job curJob2 = pawn.CurJob;
								if (curJob2 != null && (curJob2.collideWithPawns || curJob2.def.collideWithPawns || pawn.jobs.curDriver.collideWithPawns))
								{
									return pawn;
								}
							}
						}
					}
				}
				IL_19D:;
			}
			return null;
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

		public static bool KnownDangerAt(IntVec3 c, Map map, Pawn forPawn)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice.IsDangerousFor(forPawn);
		}

		public static bool ShouldSendNotificationAbout(Pawn p)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (PawnGenerator.IsBeingGenerated(p))
			{
				return false;
			}
			if (p.IsWorldPawn() && (!p.IsCaravanMember() || !p.GetCaravan().IsPlayerControlled) && !PawnUtility.IsTravelingInTransportPodWorldObject(p) && p.Corpse.DestroyedOrNull())
			{
				return false;
			}
			if (p.Faction != Faction.OfPlayer)
			{
				if (p.HostFaction != Faction.OfPlayer)
				{
					return false;
				}
				if (p.RaceProps.Humanlike && p.guest.Released && !p.Downed && !p.InBed())
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

		public static bool ShouldGetThoughtAbout(Pawn pawn, Pawn subject)
		{
			return pawn.Faction == subject.Faction || (!subject.IsWorldPawn() && !pawn.IsWorldPawn());
		}

		public static bool IsTeetotaler(this Pawn pawn)
		{
			return pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0;
		}

		public static bool IsProsthophobe(this Pawn pawn)
		{
			return pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.BodyPurist);
		}

		public static bool IsPrisonerInPrisonCell(this Pawn pawn)
		{
			return pawn.IsPrisoner && pawn.Spawned && pawn.Position.IsInPrisonCell(pawn.Map);
		}

		public static string PawnKindsToCommaList(IEnumerable<Pawn> pawns, bool useAnd = false)
		{
			PawnUtility.tmpPawns.Clear();
			PawnUtility.tmpPawns.AddRange(pawns);
			if (PawnUtility.tmpPawns.Count >= 2)
			{
				PawnUtility.tmpPawns.SortBy((Pawn x) => !x.RaceProps.Humanlike, (Pawn x) => x.GetKindLabelPlural(-1));
			}
			PawnUtility.tmpAddedPawnKinds.Clear();
			PawnUtility.tmpPawnKindsStr.Clear();
			for (int i = 0; i < PawnUtility.tmpPawns.Count; i++)
			{
				if (!PawnUtility.tmpAddedPawnKinds.Contains(PawnUtility.tmpPawns[i].kindDef))
				{
					PawnUtility.tmpAddedPawnKinds.Add(PawnUtility.tmpPawns[i].kindDef);
					int num = 0;
					for (int j = 0; j < PawnUtility.tmpPawns.Count; j++)
					{
						if (PawnUtility.tmpPawns[j].kindDef == PawnUtility.tmpPawns[i].kindDef)
						{
							num++;
						}
					}
					if (num == 1)
					{
						PawnUtility.tmpPawnKindsStr.Add("1 " + PawnUtility.tmpPawns[i].KindLabel);
					}
					else
					{
						PawnUtility.tmpPawnKindsStr.Add(num + " " + PawnUtility.tmpPawns[i].GetKindLabelPlural(num));
					}
				}
			}
			return PawnUtility.tmpPawnKindsStr.ToCommaList(useAnd);
		}

		public static string PawnKindsToCommaList(IEnumerable<PawnKindDef> pawnKinds, bool useAnd = false)
		{
			PawnUtility.tmpPawnKinds.Clear();
			PawnUtility.tmpPawnKinds.AddRange(pawnKinds);
			if (PawnUtility.tmpPawnKinds.Count >= 2)
			{
				PawnUtility.tmpPawnKinds.SortBy((PawnKindDef x) => !x.RaceProps.Humanlike, (PawnKindDef x) => GenLabel.BestKindLabel(x, Gender.None, true, -1));
			}
			PawnUtility.tmpAddedPawnKinds.Clear();
			PawnUtility.tmpPawnKindsStr.Clear();
			for (int i = 0; i < PawnUtility.tmpPawnKinds.Count; i++)
			{
				if (!PawnUtility.tmpAddedPawnKinds.Contains(PawnUtility.tmpPawnKinds[i]))
				{
					PawnUtility.tmpAddedPawnKinds.Add(PawnUtility.tmpPawnKinds[i]);
					int num = 0;
					for (int j = 0; j < PawnUtility.tmpPawnKinds.Count; j++)
					{
						if (PawnUtility.tmpPawnKinds[j] == PawnUtility.tmpPawnKinds[i])
						{
							num++;
						}
					}
					if (num == 1)
					{
						PawnUtility.tmpPawnKindsStr.Add("1 " + GenLabel.BestKindLabel(PawnUtility.tmpPawnKinds[i], Gender.None, false, -1));
					}
					else
					{
						PawnUtility.tmpPawnKindsStr.Add(num + " " + GenLabel.BestKindLabel(PawnUtility.tmpPawnKinds[i], Gender.None, true, num));
					}
				}
			}
			return PawnUtility.tmpPawnKindsStr.ToCommaList(useAnd);
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

		public static bool IsFighting(this Pawn pawn)
		{
			return pawn.CurJob != null && (pawn.CurJob.def == JobDefOf.AttackMelee || pawn.CurJob.def == JobDefOf.AttackStatic || pawn.CurJob.def == JobDefOf.Wait_Combat || pawn.CurJob.def == JobDefOf.PredatorHunt);
		}

		public static float RecruitDifficulty(this Pawn pawn, Faction recruiterFaction)
		{
			float num = pawn.kindDef.baseRecruitDifficulty;
			Rand.PushState();
			Rand.Seed = pawn.HashOffset();
			num += Rand.Gaussian(0f, 0.15f);
			Rand.PopState();
			if (pawn.Faction != null)
			{
				int num2 = Mathf.Min((int)pawn.Faction.def.techLevel, 4);
				int num3 = Mathf.Min((int)recruiterFaction.def.techLevel, 4);
				int num4 = Mathf.Abs(num2 - num3);
				num += (float)num4 * 0.16f;
			}
			return Mathf.Clamp(num, 0.1f, 0.99f);
		}

		public static void GiveAllStartingPlayerPawnsThought(ThoughtDef thought)
		{
			foreach (Pawn current in Find.GameInitData.startingAndOptionalPawns)
			{
				if (thought.IsSocial)
				{
					foreach (Pawn current2 in Find.GameInitData.startingAndOptionalPawns)
					{
						if (current2 != current)
						{
							current.needs.mood.thoughts.memories.TryGainMemory(thought, current2);
						}
					}
				}
				else
				{
					current.needs.mood.thoughts.memories.TryGainMemory(thought, null);
				}
			}
		}

		public static IntVec3 DutyLocation(this Pawn pawn)
		{
			if (pawn.mindState.duty != null && pawn.mindState.duty.focus.IsValid)
			{
				return pawn.mindState.duty.focus.Cell;
			}
			return pawn.Position;
		}

		public static bool EverBeenColonistOrTameAnimal(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsColonistOrColonyAnimal) > 0;
		}

		public static bool EverBeenPrisoner(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsPrisoner) > 0;
		}

		public static void RecoverFromUnwalkablePositionOrKill(IntVec3 c, Map map)
		{
			if (!c.InBounds(map) || c.Walkable(map))
			{
				return;
			}
			PawnUtility.tmpThings.Clear();
			PawnUtility.tmpThings.AddRange(c.GetThingList(map));
			for (int i = 0; i < PawnUtility.tmpThings.Count; i++)
			{
				Pawn pawn = PawnUtility.tmpThings[i] as Pawn;
				if (pawn != null)
				{
					IntVec3 position;
					if (CellFinder.TryFindBestPawnStandCell(pawn, out position, false))
					{
						pawn.Position = position;
						pawn.Notify_Teleported(true, false);
					}
					else
					{
						DamageDef crush = DamageDefOf.Crush;
						float amount = 99999f;
						float armorPenetration = 999f;
						BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
						DamageInfo damageInfo = new DamageInfo(crush, amount, armorPenetration, -1f, null, brain, null, DamageInfo.SourceCategory.Collapse, null);
						pawn.TakeDamage(damageInfo);
						if (!pawn.Dead)
						{
							pawn.Kill(new DamageInfo?(damageInfo), null);
						}
					}
				}
			}
		}

		public static float GetManhunterOnDamageChance(Pawn pawn, float distance, Thing instigator)
		{
			float num = PawnUtility.GetManhunterOnDamageChance(pawn.kindDef);
			num *= GenMath.LerpDoubleClamped(1f, 30f, 3f, 1f, distance);
			if (instigator != null)
			{
				num *= 1f - instigator.GetStatValue(StatDefOf.HuntingStealth, true);
			}
			return num;
		}

		public static float GetManhunterOnDamageChance(Pawn pawn, Thing instigator = null)
		{
			if (instigator != null)
			{
				return PawnUtility.GetManhunterOnDamageChance(pawn, pawn.Position.DistanceTo(instigator.Position), instigator);
			}
			return PawnUtility.GetManhunterOnDamageChance(pawn.kindDef);
		}

		public static float GetManhunterOnDamageChance(PawnKindDef kind)
		{
			return kind.RaceProps.manhunterOnDamageChance * Find.Storyteller.difficulty.manhunterChanceOnDamageFactor;
		}

		public static float GetManhunterOnDamageChance(RaceProperties race)
		{
			return race.manhunterOnDamageChance * Find.Storyteller.difficulty.manhunterChanceOnDamageFactor;
		}
	}
}
