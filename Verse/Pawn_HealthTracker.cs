using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public class Pawn_HealthTracker : IExposable
	{
		private Pawn pawn;

		private PawnHealthState healthState = PawnHealthState.Mobile;

		[Unsaved]
		public Effecter deflectionEffecter;

		public bool forceIncap;

		public HediffSet hediffSet;

		public PawnCapacitiesHandler capacities;

		public BillStack surgeryBills;

		public SummaryHealthHandler summaryHealth;

		public ImmunityHandler immunity;

		public PawnHealthState State
		{
			get
			{
				return this.healthState;
			}
		}

		public bool Downed
		{
			get
			{
				return this.healthState == PawnHealthState.Down;
			}
		}

		public bool Dead
		{
			get
			{
				return this.healthState == PawnHealthState.Dead;
			}
		}

		public bool InPainShock
		{
			get
			{
				return this.hediffSet.PainTotal >= this.pawn.GetStatValue(StatDefOf.PainShockThreshold, true);
			}
		}

		public Pawn_HealthTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.hediffSet = new HediffSet(pawn);
			this.capacities = new PawnCapacitiesHandler(pawn);
			this.summaryHealth = new SummaryHealthHandler(pawn);
			this.surgeryBills = new BillStack(pawn);
			this.immunity = new ImmunityHandler(pawn);
		}

		public void Reset()
		{
			this.healthState = PawnHealthState.Mobile;
			this.hediffSet.Clear();
			this.capacities.Clear();
			this.summaryHealth.Notify_HealthChanged();
			this.surgeryBills.Clear();
			this.immunity = new ImmunityHandler(this.pawn);
		}

		public void ExposeData()
		{
			Scribe_Values.Look<PawnHealthState>(ref this.healthState, "healthState", PawnHealthState.Mobile, false);
			Scribe_Values.Look<bool>(ref this.forceIncap, "forceIncap", false, false);
			Scribe_Deep.Look<HediffSet>(ref this.hediffSet, "hediffSet", new object[]
			{
				this.pawn
			});
			Scribe_Deep.Look<BillStack>(ref this.surgeryBills, "surgeryBills", new object[]
			{
				this.pawn
			});
			Scribe_Deep.Look<ImmunityHandler>(ref this.immunity, "immunity", new object[]
			{
				this.pawn
			});
		}

		public void AddHediff(HediffDef def, BodyPartRecord part = null, DamageInfo? dinfo = null)
		{
			this.AddHediff(HediffMaker.MakeHediff(def, this.pawn, null), part, dinfo);
		}

		public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null)
		{
			if (part != null)
			{
				hediff.Part = part;
			}
			this.hediffSet.AddDirect(hediff, dinfo);
			this.CheckForStateChange(dinfo, hediff);
			if (this.pawn.RaceProps.hediffGiverSets != null)
			{
				for (int i = 0; i < this.pawn.RaceProps.hediffGiverSets.Count; i++)
				{
					HediffGiverSetDef hediffGiverSetDef = this.pawn.RaceProps.hediffGiverSets[i];
					for (int j = 0; j < hediffGiverSetDef.hediffGivers.Count; j++)
					{
						hediffGiverSetDef.hediffGivers[j].OnHediffAdded(this.pawn, hediff);
					}
				}
			}
		}

		public void RemoveHediff(Hediff hediff)
		{
			this.hediffSet.hediffs.Remove(hediff);
			hediff.PostRemoved();
			this.Notify_HediffChanged(null);
		}

		public void Notify_HediffChanged(Hediff hediff)
		{
			this.hediffSet.DirtyCache();
			this.CheckForStateChange(null, hediff);
		}

		public void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			if (dinfo.Instigator != null && this.pawn.Faction != null && this.pawn.Faction.IsPlayer && !this.pawn.InAggroMentalState)
			{
				Pawn pawn = dinfo.Instigator as Pawn;
				if (pawn != null && pawn.guilt != null && pawn.mindState != null)
				{
					pawn.guilt.Notify_Guilty();
				}
			}
			if (this.pawn.Spawned)
			{
				if (!this.pawn.Position.Fogged(this.pawn.Map))
				{
					this.pawn.mindState.Active = true;
				}
				Lord lord = this.pawn.GetLord();
				if (lord != null)
				{
					lord.Notify_PawnDamaged(this.pawn, dinfo);
				}
				if (dinfo.Def.externalViolence)
				{
					GenClamor.DoClamor(this.pawn, 18f, ClamorType.Harm);
				}
				this.pawn.mindState.Notify_DamageTaken(dinfo);
				this.pawn.jobs.Notify_DamageTaken(dinfo);
			}
			if (this.pawn.Faction != null)
			{
				this.pawn.Faction.Notify_MemberTookDamage(this.pawn, dinfo);
				if (Current.ProgramState == ProgramState.Playing && this.pawn.Faction == Faction.OfPlayer && dinfo.Def.externalViolence && this.pawn.SpawnedOrAnyParentSpawned)
				{
					this.pawn.MapHeld.dangerWatcher.Notify_ColonistHarmedExternally();
				}
			}
			if (this.pawn.apparel != null)
			{
				List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i].CheckPreAbsorbDamage(dinfo))
					{
						absorbed = true;
						return;
					}
				}
			}
			if (this.pawn.Spawned)
			{
				this.pawn.stances.Notify_DamageTaken(dinfo);
				this.pawn.stances.stunner.Notify_DamageApplied(dinfo, !this.pawn.RaceProps.IsFlesh);
				if (dinfo.Def.makesBlood && !dinfo.InstantOldInjury)
				{
					this.TryDropBloodFilth();
				}
			}
			if (this.pawn.RaceProps.IsFlesh && dinfo.Def.externalViolence)
			{
				Pawn pawn2 = dinfo.Instigator as Pawn;
				if (pawn2 != null)
				{
					if (pawn2.HostileTo(this.pawn))
					{
						this.pawn.relations.canGetRescuedThought = true;
					}
					else if (this.pawn.RaceProps.Humanlike && pawn2.RaceProps.Humanlike)
					{
						this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.HarmedMe, pawn2);
					}
				}
				TaleRecorder.RecordTale(TaleDefOf.Wounded, new object[]
				{
					this.pawn,
					pawn2,
					dinfo.Weapon
				});
			}
			absorbed = false;
		}

		public void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (this.ShouldBeDead())
			{
				if (!this.pawn.Destroyed)
				{
					this.pawn.Kill(new DamageInfo?(dinfo), null);
				}
			}
			else
			{
				if (dinfo.Def.additionalHediffs != null)
				{
					List<DamageDefAdditionalHediff> additionalHediffs = dinfo.Def.additionalHediffs;
					for (int i = 0; i < additionalHediffs.Count; i++)
					{
						DamageDefAdditionalHediff damageDefAdditionalHediff = additionalHediffs[i];
						if (damageDefAdditionalHediff.hediff != null)
						{
							float num = totalDamageDealt * damageDefAdditionalHediff.severityPerDamageDealt;
							if (num >= 0f)
							{
								Hediff hediff = HediffMaker.MakeHediff(damageDefAdditionalHediff.hediff, this.pawn, null);
								hediff.Severity = num;
								this.AddHediff(hediff, null, new DamageInfo?(dinfo));
								if (this.Dead)
								{
									return;
								}
							}
						}
					}
				}
				if (!this.Dead)
				{
					this.pawn.mindState.mentalStateHandler.Notify_DamageTaken(dinfo);
				}
			}
		}

		public void RestorePart(BodyPartRecord part, Hediff diffException = null, bool checkStateChange = true)
		{
			if (part == null)
			{
				Log.Error("Tried to restore null body part.");
				return;
			}
			this.RestorePartRecursiveInt(part, diffException);
			this.hediffSet.DirtyCache();
			if (checkStateChange)
			{
				this.CheckForStateChange(null, null);
			}
		}

		private void RestorePartRecursiveInt(BodyPartRecord part, Hediff diffException = null)
		{
			List<Hediff> hediffs = this.hediffSet.hediffs;
			for (int i = hediffs.Count - 1; i >= 0; i--)
			{
				Hediff hediff = hediffs[i];
				if (hediff.Part == part && hediff != diffException)
				{
					Hediff hediff2 = hediffs[i];
					hediffs.RemoveAt(i);
					hediff2.PostRemoved();
				}
			}
			for (int j = 0; j < part.parts.Count; j++)
			{
				this.RestorePartRecursiveInt(part.parts[j], diffException);
			}
		}

		private void CheckForStateChange(DamageInfo? dinfo, Hediff hediff)
		{
			if (!this.Dead)
			{
				if (this.ShouldBeDead())
				{
					if (!this.pawn.Destroyed)
					{
						this.pawn.Kill(dinfo, hediff);
					}
					return;
				}
				if (!this.Downed)
				{
					if (this.ShouldBeDowned())
					{
						float num = (!this.pawn.RaceProps.Animal) ? 0.67f : 0.47f;
						if (!this.forceIncap && dinfo.HasValue && dinfo.Value.Def.externalViolence && (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer) && !this.pawn.IsPrisonerOfColony && this.pawn.RaceProps.IsFlesh && Rand.Value < num)
						{
							this.pawn.Kill(dinfo, null);
							return;
						}
						this.forceIncap = false;
						this.MakeDowned(dinfo, hediff);
						return;
					}
					else if (!this.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						if (this.pawn.carryTracker != null && this.pawn.carryTracker.CarriedThing != null && this.pawn.jobs != null && this.pawn.CurJob != null)
						{
							this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
						}
						if (this.pawn.equipment != null && this.pawn.equipment.Primary != null)
						{
							if (this.pawn.InContainerEnclosed)
							{
								this.pawn.equipment.TryTransferEquipmentToContainer(this.pawn.equipment.Primary, this.pawn.holdingOwner);
							}
							else if (this.pawn.SpawnedOrAnyParentSpawned)
							{
								ThingWithComps thingWithComps;
								this.pawn.equipment.TryDropEquipment(this.pawn.equipment.Primary, out thingWithComps, this.pawn.PositionHeld, true);
							}
							else
							{
								this.pawn.equipment.DestroyEquipment(this.pawn.equipment.Primary);
							}
						}
					}
				}
				else if (!this.ShouldBeDowned())
				{
					this.MakeUndowned();
					return;
				}
			}
		}

		private bool ShouldBeDowned()
		{
			return this.InPainShock || !this.capacities.CanBeAwake || !this.capacities.CapableOf(PawnCapacityDefOf.Moving);
		}

		private bool ShouldBeDead()
		{
			if (this.Dead)
			{
				return true;
			}
			for (int i = 0; i < this.hediffSet.hediffs.Count; i++)
			{
				if (this.hediffSet.hediffs[i].CauseDeathNow())
				{
					return true;
				}
			}
			if (this.ShouldBeDeadFromRequiredCapacity() != null)
			{
				return true;
			}
			float num = PawnCapacityUtility.CalculatePartEfficiency(this.hediffSet, this.pawn.RaceProps.body.corePart, false, null);
			return num <= 0.0001f;
		}

		public PawnCapacityDef ShouldBeDeadFromRequiredCapacity()
		{
			List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				PawnCapacityDef pawnCapacityDef = allDefsListForReading[i];
				bool flag = (!this.pawn.RaceProps.IsFlesh) ? pawnCapacityDef.lethalMechanoids : pawnCapacityDef.lethalFlesh;
				if (flag && !this.capacities.CapableOf(pawnCapacityDef))
				{
					return pawnCapacityDef;
				}
			}
			return null;
		}

		public bool WouldDieAfterAddingHediff(Hediff hediff)
		{
			if (this.Dead)
			{
				return true;
			}
			this.hediffSet.hediffs.Add(hediff);
			this.hediffSet.DirtyCache();
			bool result = this.ShouldBeDead();
			this.hediffSet.hediffs.Remove(hediff);
			this.hediffSet.DirtyCache();
			return result;
		}

		public bool WouldDieAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
		{
			Hediff hediff = HediffMaker.MakeHediff(def, this.pawn, part);
			hediff.Severity = severity;
			return this.WouldDieAfterAddingHediff(hediff);
		}

		public void SetDead()
		{
			if (this.Dead)
			{
				Log.Error(this.pawn + " set dead while already dead.");
			}
			this.healthState = PawnHealthState.Dead;
		}

		private void MakeDowned(DamageInfo? dinfo, Hediff hediff)
		{
			if (this.Downed)
			{
				Log.Error(this.pawn + " tried to do MakeDowned while already downed.");
				return;
			}
			this.healthState = PawnHealthState.Down;
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this.pawn, dinfo, PawnDiedOrDownedThoughtsKind.Downed);
			if (this.pawn.InMentalState)
			{
				this.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
			}
			if (this.pawn.Spawned)
			{
				if (this.pawn.IsColonist && dinfo.HasValue && dinfo.Value.Def.externalViolence)
				{
					Find.StoryWatcher.watcherRampUp.Notify_ColonistViolentlyDownedOrKilled(this.pawn);
				}
				this.pawn.DropAndForbidEverything(true);
				this.pawn.stances.CancelBusyStanceSoft();
			}
			this.pawn.ClearMind(true);
			if (Current.ProgramState == ProgramState.Playing)
			{
				Lord lord = this.pawn.GetLord();
				if (lord != null)
				{
					lord.Notify_PawnLost(this.pawn, PawnLostCondition.IncappedOrKilled);
				}
			}
			if (this.pawn.Drafted)
			{
				this.pawn.drafter.Drafted = false;
			}
			PortraitsCache.SetDirty(this.pawn);
			if (this.pawn.SpawnedOrAnyParentSpawned)
			{
				GenHostility.Notify_PawnLostForTutor(this.pawn, this.pawn.MapHeld);
			}
			if (this.pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing && this.pawn.SpawnedOrAnyParentSpawned)
			{
				if (this.pawn.HostileTo(Faction.OfPlayer))
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.Capturing, this.pawn, OpportunityType.Important);
				}
				if (this.pawn.Faction == Faction.OfPlayer)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.Rescuing, this.pawn, OpportunityType.Critical);
				}
			}
			if (dinfo.HasValue && dinfo.Value.Instigator != null)
			{
				Pawn pawn = dinfo.Value.Instigator as Pawn;
				if (pawn != null)
				{
					RecordsUtility.Notify_PawnDowned(this.pawn, pawn);
				}
			}
			if (this.pawn.Spawned)
			{
				TaleRecorder.RecordTale(TaleDefOf.Downed, new object[]
				{
					this.pawn,
					(!dinfo.HasValue) ? null : (dinfo.Value.Instigator as Pawn),
					(!dinfo.HasValue) ? null : dinfo.Value.Weapon
				});
				Find.BattleLog.Add(new BattleLogEntry_StateTransition(this.pawn, RulePackDefOf.Transition_Downed, (!dinfo.HasValue) ? null : (dinfo.Value.Instigator as Pawn), hediff, (!dinfo.HasValue) ? null : dinfo.Value.HitPart.def));
			}
		}

		private void MakeUndowned()
		{
			if (!this.Downed)
			{
				Log.Error(this.pawn + " tried to do MakeUndowned when already undowned.");
				return;
			}
			this.healthState = PawnHealthState.Mobile;
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				Messages.Message("MessageNoLongerDowned".Translate(new object[]
				{
					this.pawn.LabelCap
				}), this.pawn, MessageTypeDefOf.PositiveEvent);
			}
			if (this.pawn.Spawned && !this.pawn.InBed())
			{
				this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
			}
			PortraitsCache.SetDirty(this.pawn);
			if (this.pawn.guest != null)
			{
				this.pawn.guest.Notify_PawnUndowned();
			}
		}

		public void NotifyPlayerOfKilled(DamageInfo? dinfo, Hediff hediff, Caravan caravan)
		{
			string text = null;
			if (dinfo.HasValue)
			{
				text = string.Format(dinfo.Value.Def.deathMessage, this.pawn.NameStringShort.CapitalizeFirst());
			}
			else if (hediff != null)
			{
				text = "PawnDiedBecauseOf".Translate(new object[]
				{
					this.pawn.NameStringShort.CapitalizeFirst(),
					hediff.def.label
				});
			}
			if (!text.NullOrEmpty())
			{
				text = text.AdjustedFor(this.pawn);
				GlobalTargetInfo lookTarget;
				if (caravan != null)
				{
					lookTarget = caravan;
				}
				else
				{
					lookTarget = this.pawn;
				}
				Messages.Message(text, lookTarget, MessageTypeDefOf.PawnDeath);
			}
		}

		public void Notify_Resurrected()
		{
			this.healthState = PawnHealthState.Mobile;
			this.hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x.TryGetComp<HediffComp_Immunizable>() != null);
			this.hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x is Hediff_Injury && !x.IsOld());
			this.hediffSet.hediffs.RemoveAll(delegate(Hediff x)
			{
				bool arg_6B_0;
				if (x.def.everCurableByItem)
				{
					if (x.def.lethalSeverity < 0f)
					{
						if (x.def.stages != null)
						{
							arg_6B_0 = x.def.stages.Any((HediffStage y) => y.lifeThreatening);
						}
						else
						{
							arg_6B_0 = false;
						}
					}
					else
					{
						arg_6B_0 = true;
					}
				}
				else
				{
					arg_6B_0 = false;
				}
				return arg_6B_0;
			});
			this.hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x is Hediff_Injury && x.IsOld() && this.hediffSet.GetPartHealth(x.Part) <= 0f);
			while (true)
			{
				Hediff_MissingPart hediff_MissingPart = (from x in this.hediffSet.GetMissingPartsCommonAncestors()
				where !this.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x.Part)
				select x).FirstOrDefault<Hediff_MissingPart>();
				if (hediff_MissingPart == null)
				{
					break;
				}
				this.RestorePart(hediff_MissingPart.Part, null, false);
			}
			this.hediffSet.DirtyCache();
			if (this.ShouldBeDead())
			{
				this.hediffSet.hediffs.Clear();
			}
			this.Notify_HediffChanged(null);
		}

		public void HealthTick()
		{
			if (this.Dead)
			{
				return;
			}
			for (int i = this.hediffSet.hediffs.Count - 1; i >= 0; i--)
			{
				Hediff hediff = this.hediffSet.hediffs[i];
				hediff.Tick();
				hediff.PostTick();
			}
			bool flag = false;
			for (int j = this.hediffSet.hediffs.Count - 1; j >= 0; j--)
			{
				Hediff hediff2 = this.hediffSet.hediffs[j];
				if (hediff2.ShouldRemove)
				{
					this.hediffSet.hediffs.RemoveAt(j);
					hediff2.PostRemoved();
					flag = true;
				}
			}
			if (flag)
			{
				this.Notify_HediffChanged(null);
			}
			if (this.Dead)
			{
				return;
			}
			this.immunity.ImmunityHandlerTick();
			if (this.pawn.RaceProps.IsFlesh && this.pawn.IsHashIntervalTick(600) && (this.pawn.needs.food == null || !this.pawn.needs.food.Starving))
			{
				bool flag2 = false;
				if (this.hediffSet.HasNaturallyHealingInjury())
				{
					float num = 8f;
					if (this.pawn.GetPosture() != PawnPosture.Standing)
					{
						num += 4f;
						Building_Bed building_Bed = this.pawn.CurrentBed();
						if (building_Bed != null)
						{
							num += building_Bed.def.building.bed_healPerDay;
						}
					}
					Hediff_Injury hediff_Injury = this.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
					hediff_Injury.Heal(num * this.pawn.HealthScale * 0.01f);
					flag2 = true;
				}
				if (this.hediffSet.HasTendedAndHealingInjury() && (this.pawn.needs.food == null || !this.pawn.needs.food.Starving))
				{
					Hediff_Injury hediff_Injury2 = this.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealFromTending)).RandomElement<Hediff_Injury>();
					float tendQuality = hediff_Injury2.TryGetComp<HediffComp_TendDuration>().tendQuality;
					float num2 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
					hediff_Injury2.Heal(22f * num2 * this.pawn.HealthScale * 0.01f);
					flag2 = true;
				}
				if (flag2 && !this.HasHediffsNeedingTendByColony(false) && !HealthAIUtility.ShouldSeekMedicalRest(this.pawn) && !this.hediffSet.HasTendedAndHealingInjury() && PawnUtility.ShouldSendNotificationAbout(this.pawn))
				{
					Messages.Message("MessageFullyHealed".Translate(new object[]
					{
						this.pawn.LabelCap
					}), this.pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
			if (this.pawn.RaceProps.IsFlesh && this.hediffSet.BleedRateTotal >= 0.1f)
			{
				float num3 = this.hediffSet.BleedRateTotal * this.pawn.BodySize;
				if (this.pawn.GetPosture() == PawnPosture.Standing)
				{
					num3 *= 0.008f;
				}
				else
				{
					num3 *= 0.0008f;
				}
				if (Rand.Value < num3)
				{
					this.TryDropBloodFilth();
				}
			}
			List<HediffGiverSetDef> hediffGiverSets = this.pawn.RaceProps.hediffGiverSets;
			if (hediffGiverSets != null && this.pawn.IsHashIntervalTick(60))
			{
				for (int k = 0; k < hediffGiverSets.Count; k++)
				{
					List<HediffGiver> hediffGivers = hediffGiverSets[k].hediffGivers;
					for (int l = 0; l < hediffGivers.Count; l++)
					{
						hediffGivers[l].OnIntervalPassed(this.pawn, null);
						if (this.pawn.Dead)
						{
							return;
						}
					}
				}
			}
		}

		public bool HasHediffsNeedingTend(bool forAlert = false)
		{
			return this.hediffSet.HasTendableHediff(forAlert);
		}

		public bool HasHediffsNeedingTendByColony(bool forAlert = false)
		{
			if (this.HasHediffsNeedingTend(forAlert))
			{
				if (this.pawn.NonHumanlikeOrWildMan())
				{
					if (this.pawn.Faction == Faction.OfPlayer)
					{
						return true;
					}
					Building_Bed building_Bed = this.pawn.CurrentBed();
					if (building_Bed != null && building_Bed.Faction == Faction.OfPlayer)
					{
						return true;
					}
				}
				else if ((this.pawn.Faction == Faction.OfPlayer && this.pawn.HostFaction == null) || this.pawn.HostFaction == Faction.OfPlayer)
				{
					return true;
				}
			}
			return false;
		}

		protected void TryDropBloodFilth()
		{
			if (Rand.Value < 0.5f)
			{
				this.DropBloodFilth();
			}
		}

		public void DropBloodFilth()
		{
			if ((this.pawn.Spawned || this.pawn.ParentHolder is Pawn_CarryTracker) && this.pawn.SpawnedOrAnyParentSpawned && this.pawn.RaceProps.BloodDef != null)
			{
				FilthMaker.MakeFilth(this.pawn.PositionHeld, this.pawn.MapHeld, this.pawn.RaceProps.BloodDef, this.pawn.LabelIndefinite(), 1);
			}
		}
	}
}
