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
				return this.hediffSet.PainTotal >= 0.8f;
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
			Scribe_Values.LookValue<PawnHealthState>(ref this.healthState, "healthState", PawnHealthState.Mobile, false);
			Scribe_Values.LookValue<bool>(ref this.forceIncap, "forceIncap", false, false);
			Scribe_Deep.LookDeep<HediffSet>(ref this.hediffSet, "hediffSet", new object[]
			{
				this.pawn
			});
			Scribe_Deep.LookDeep<BillStack>(ref this.surgeryBills, "surgeryBills", new object[]
			{
				this.pawn
			});
			Scribe_Deep.LookDeep<ImmunityHandler>(ref this.immunity, "immunity", new object[]
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
					lord.Notify_PawnTookDamage(this.pawn, dinfo);
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
				if (Current.ProgramState == ProgramState.Playing && this.pawn.Faction == Faction.OfPlayer && dinfo.Def.externalViolence && this.pawn.MapHeld != null)
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
						this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.HarmedMe, pawn2);
					}
				}
			}
			absorbed = false;
		}

		public void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (this.ShouldBeDead())
			{
				if (!this.pawn.Destroyed)
				{
					this.Kill(new DamageInfo?(dinfo), null);
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
			this.RestorePartRecursive(part, diffException);
			this.hediffSet.DirtyCache();
			if (checkStateChange)
			{
				this.CheckForStateChange(null, null);
			}
		}

		private void RestorePartRecursive(BodyPartRecord part, Hediff diffException = null)
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
				this.RestorePartRecursive(part.parts[j], diffException);
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
						this.Kill(dinfo, hediff);
					}
					return;
				}
				if (!this.Downed)
				{
					if (this.ShouldBeDowned())
					{
						float num = (!this.pawn.RaceProps.Animal) ? 0.67f : 0.47f;
						if (!this.forceIncap && (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer) && !this.pawn.IsPrisonerOfColony && this.pawn.RaceProps.IsFlesh && Rand.Value < num)
						{
							this.Kill(dinfo, null);
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
								ThingWithComps thingWithComps;
								this.pawn.equipment.TryTransferEquipmentToContainer(this.pawn.equipment.Primary, this.pawn.holdingContainer, out thingWithComps);
							}
							else if (this.pawn.Spawned)
							{
								ThingWithComps thingWithComps;
								this.pawn.equipment.TryDropEquipment(this.pawn.equipment.Primary, out thingWithComps, this.pawn.Position, true);
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
			List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
			for (int j = 0; j < allDefsListForReading.Count; j++)
			{
				PawnCapacityDef pawnCapacityDef = allDefsListForReading[j];
				bool flag = (!this.pawn.RaceProps.IsFlesh) ? pawnCapacityDef.lethalMechanoids : pawnCapacityDef.lethalFlesh;
				if (flag && !this.capacities.CapableOf(pawnCapacityDef))
				{
					return true;
				}
			}
			float num = PawnCapacityUtility.CalculatePartEfficiency(this.hediffSet, this.pawn.RaceProps.body.corePart, false);
			return num <= 0.0001f;
		}

		private void MakeDowned(DamageInfo? dinfo, Hediff hediff)
		{
			if (this.Downed)
			{
				Log.Error(this.pawn + " tried to do MakeDowned while already downed.");
				return;
			}
			this.healthState = PawnHealthState.Down;
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this.pawn, dinfo, hediff, PawnDiedOrDownedThoughtsKind.Downed);
			if (this.pawn.MentalState != null)
			{
				this.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
			}
			if (this.pawn.Spawned)
			{
				if (this.pawn.Faction == Faction.OfPlayer)
				{
					Find.StoryWatcher.watcherRampUp.Notify_PlayerPawnIncappedOrKilled(this.pawn);
				}
				this.pawn.DropAndForbidEverything(true);
				this.pawn.stances.CancelBusyStanceSoft();
			}
			this.pawn.ClearMind(true);
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.pawn.ClearReservations();
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
			GenHostility.Notify_PawnLostForTutor(this.pawn);
			if (this.pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing)
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
				}), this.pawn, MessageSound.Benefit);
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

		public void Kill(DamageInfo? dinfo, Hediff hediff)
		{
			Map map = this.pawn.Map;
			bool spawned = this.pawn.Spawned;
			Building_Grave building_Grave = null;
			if (this.pawn.ownership != null)
			{
				building_Grave = this.pawn.ownership.AssignedGrave;
			}
			bool flag = this.pawn.InBed();
			float num = 0f;
			if (flag)
			{
				num = this.pawn.CurrentBed().Rotation.AsAngle;
			}
			ThingContainer thingContainer = null;
			bool inContainerEnclosed = this.pawn.InContainerEnclosed;
			if (inContainerEnclosed)
			{
				thingContainer = this.pawn.holdingContainer;
				thingContainer.Remove(this.pawn);
			}
			bool flag2 = false;
			if (Current.ProgramState == ProgramState.Playing && map != null)
			{
				flag2 = (map.designationManager.DesignationOn(this.pawn, DesignationDefOf.Hunt) != null);
			}
			float num2 = 0f;
			Thing attachment = this.pawn.GetAttachment(ThingDefOf.Fire);
			if (attachment != null)
			{
				num2 = ((Fire)attachment).CurrentSize();
			}
			if (Current.ProgramState == ProgramState.Playing && this.pawn.Faction != null && this.pawn.Faction == Faction.OfPlayer)
			{
				Find.StoryWatcher.watcherRampUp.Notify_PlayerPawnIncappedOrKilled(this.pawn);
			}
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this.pawn, dinfo, hediff, PawnDiedOrDownedThoughtsKind.Died);
			if (spawned && dinfo.HasValue && dinfo.Value.Def.externalViolence)
			{
				LifeStageUtility.PlayNearestLifestageSound(this.pawn, (LifeStageAge ls) => ls.soundDeath, 1f);
			}
			if (dinfo.HasValue && dinfo.Value.Instigator != null)
			{
				Pawn pawn = dinfo.Value.Instigator as Pawn;
				if (pawn != null)
				{
					RecordsUtility.Notify_PawnKilled(this.pawn, pawn);
				}
			}
			if (this.pawn.IsColonist)
			{
				Find.StoryWatcher.statsRecord.colonistsKilled++;
			}
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
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
					Messages.Message(text, this.pawn, MessageSound.Negative);
				}
			}
			if (Current.ProgramState == ProgramState.Playing && dinfo.HasValue)
			{
				Pawn pawn2 = dinfo.Value.Instigator as Pawn;
				if (pawn2 == null || pawn2.CurJob == null || !(pawn2.jobs.curDriver is JobDriver_Execute))
				{
					if (pawn2 != null)
					{
						if (this.pawn.Faction != Faction.OfPlayer && this.pawn.kindDef.combatPower >= 250f && pawn2.Faction == Faction.OfPlayer)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledMajorColonyEnemy, new object[]
							{
								pawn2,
								this.pawn
							});
						}
						else if (this.pawn.IsColonist)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledColonist, new object[]
							{
								pawn2,
								this.pawn
							});
						}
						else if (this.pawn.Faction == Faction.OfPlayer && this.pawn.RaceProps.Animal)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledColonyAnimal, new object[]
							{
								pawn2,
								this.pawn
							});
						}
					}
					if (this.pawn.Faction == Faction.OfPlayer && (this.pawn.RaceProps.Humanlike || dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction != Faction.OfPlayer))
					{
						TaleRecorder.RecordTale(TaleDefOf.KilledBy, new object[]
						{
							this.pawn,
							dinfo.Value
						});
					}
				}
			}
			this.surgeryBills.Clear();
			if (this.pawn.apparel != null)
			{
				this.pawn.apparel.Notify_PawnKilled(dinfo);
			}
			if (this.pawn.RaceProps.IsFlesh)
			{
				this.pawn.relations.Notify_PawnKilled(dinfo, map);
			}
			this.pawn.meleeVerbs.Notify_PawnKilled();
			this.healthState = PawnHealthState.Dead;
			if (this.pawn.holdingContainer != null)
			{
				Pawn_CarryTracker pawn_CarryTracker = this.pawn.holdingContainer.owner as Pawn_CarryTracker;
				if (pawn_CarryTracker != null)
				{
					Thing thing;
					this.pawn.holdingContainer.TryDrop(this.pawn, pawn_CarryTracker.pawn.Position, pawn_CarryTracker.pawn.Map, ThingPlaceMode.Near, out thing, null);
				}
			}
			if (spawned)
			{
				this.pawn.DropAndForbidEverything(false);
			}
			Corpse corpse = null;
			if (!PawnGenerator.IsBeingGenerated(this.pawn))
			{
				corpse = (Corpse)ThingMaker.MakeThing(this.pawn.RaceProps.corpseDef, null);
				corpse.InnerPawn = this.pawn;
				if (building_Grave != null)
				{
					corpse.InnerPawn.ownership.ClaimGrave(building_Grave);
				}
				if (flag)
				{
					corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(num + 180f);
				}
				if (spawned)
				{
					GenPlace.TryPlaceThing(corpse, this.pawn.Position, map, ThingPlaceMode.Direct, null);
					corpse.Rotation = this.pawn.Rotation;
					if (HuntJobUtility.WasKilledByHunter(this.pawn, dinfo))
					{
						((Pawn)dinfo.Value.Instigator).Reserve(corpse, 1);
					}
					else if (!flag2)
					{
						corpse.SetForbiddenIfOutsideHomeArea();
					}
					if (num2 > 0f)
					{
						FireUtility.TryStartFireIn(corpse.Position, corpse.Map, num2);
					}
				}
				else if (inContainerEnclosed)
				{
					thingContainer.TryAdd(corpse, true);
				}
				else
				{
					corpse.Destroy(DestroyMode.Vanish);
				}
			}
			this.pawn.Destroy(DestroyMode.Kill);
			this.pawn.ClearMapReferencesToMeForDestroy(map);
			PawnComponentsUtility.RemoveComponentsOnKilled(this.pawn);
			this.hediffSet.DirtyCache();
			PortraitsCache.SetDirty(this.pawn);
			for (int i = 0; i < this.hediffSet.hediffs.Count; i++)
			{
				this.hediffSet.hediffs[i].Notify_PawnDied();
			}
			if (this.pawn.Faction != null && this.pawn == this.pawn.Faction.leader)
			{
				this.pawn.Faction.Notify_LeaderDied();
			}
			Caravan caravan = this.pawn.GetCaravan();
			if (caravan != null)
			{
				caravan.Notify_MemberDied(this.pawn);
			}
			if (corpse != null)
			{
				if (this.pawn.RaceProps.DeathActionWorker != null && spawned)
				{
					this.pawn.RaceProps.DeathActionWorker.PawnDied(corpse);
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_PawnDied(corpse);
				}
			}
			GenHostility.Notify_PawnLostForTutor(this.pawn);
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
					Building_Bed building_Bed = this.pawn.CurrentBed();
					if (building_Bed != null)
					{
						num += building_Bed.def.building.bed_healPerDay;
					}
					Hediff_Injury hediff_Injury = (from x in this.hediffSet.GetHediffs<Hediff_Injury>()
					where x.CanHealNaturally()
					select x).RandomElement<Hediff_Injury>();
					hediff_Injury.Heal(num * this.pawn.HealthScale * 0.01f);
					flag2 = true;
				}
				if (this.hediffSet.HasTendedAndHealingInjury() && (this.pawn.needs.food == null || !this.pawn.needs.food.Starving))
				{
					Hediff_Injury hediff_Injury2 = (from x in this.hediffSet.GetHediffs<Hediff_Injury>()
					where x.CanHealFromTending()
					select x).RandomElement<Hediff_Injury>();
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
					}), this.pawn, MessageSound.Benefit);
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
			return this.hediffSet.HasTendableInjury() || this.hediffSet.HasFreshMissingPartsCommonAncestor() || this.hediffSet.HasTendableNonInjuryNonMissingPartHediff(forAlert);
		}

		public bool HasHediffsNeedingTendByColony(bool forAlert = false)
		{
			if (this.HasHediffsNeedingTend(forAlert))
			{
				if (!this.pawn.RaceProps.Humanlike)
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
			if ((this.pawn.Spawned || (this.pawn.holdingContainer != null && this.pawn.holdingContainer.owner is Pawn_CarryTracker)) && this.pawn.PositionHeld.InBounds(this.pawn.MapHeld) && this.pawn.RaceProps.BloodDef != null && !this.pawn.InContainerEnclosed)
			{
				FilthMaker.MakeFilth(this.pawn.PositionHeld, this.pawn.MapHeld, this.pawn.RaceProps.BloodDef, this.pawn.LabelIndefinite(), 1);
			}
		}
	}
}
