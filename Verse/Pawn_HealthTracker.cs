using RimWorld;
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
				return this.hediffSet.Pain >= 0.8f;
			}
		}

		public bool ShouldEverReceiveMedicalCare
		{
			get
			{
				return (this.pawn.playerSettings == null || this.pawn.playerSettings.medCare != MedicalCareCategory.NoCare) && (this.pawn.guest == null || this.pawn.guest.interactionMode != PrisonerInteractionMode.Execution) && Find.DesignationManager.DesignationOn(this.pawn, DesignationDefOf.Slaughter) == null;
			}
		}

		public bool ShouldBeTendedNow
		{
			get
			{
				return this.pawn.playerSettings != null && this.ShouldEverReceiveMedicalCare && this.HasHediffsNeedingTendByColony(false);
			}
		}

		public bool NeedsMedicalRest
		{
			get
			{
				return this.Downed || this.HasHediffsNeedingTend(false) || this.ShouldDoSurgeryNow;
			}
		}

		public bool PrefersMedicalRest
		{
			get
			{
				return this.hediffSet.HasTendedAndHealingInjury || this.hediffSet.HasTendedImmunizableNonInjuryNonMissingPartHediff || this.NeedsMedicalRest;
			}
		}

		public bool ShouldDoSurgeryNow
		{
			get
			{
				return this.surgeryBills.AnyShouldDoNow;
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

		public bool HasHediffsNeedingTend(bool forAlert = false)
		{
			return this.hediffSet.HasTendableInjury || this.hediffSet.HasFreshMissingPartsCommonAncestor() || this.hediffSet.HasTendableNonInjuryNonMissingPartHediff(forAlert);
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
			this.hediffSet.AddHediffDirect(hediff, dinfo);
			this.CheckForStateChange(dinfo, hediff);
			if (this.pawn.RaceProps.hediffGiverSets != null)
			{
				for (int i = 0; i < this.pawn.RaceProps.hediffGiverSets.Count; i++)
				{
					HediffGiverSetDef hediffGiverSetDef = this.pawn.RaceProps.hediffGiverSets[i];
					for (int j = 0; j < hediffGiverSetDef.hediffGivers.Count; j++)
					{
						hediffGiverSetDef.hediffGivers[j].CheckGiveHediffAdded(this.pawn, hediff);
					}
				}
			}
		}

		public void RemoveHediff(Hediff hediff)
		{
			this.hediffSet.hediffs.Remove(hediff);
			hediff.HediffRemoved();
			this.Notify_HediffChanged(null);
		}

		public void HealHediff(Hediff hediff, int amount)
		{
			this.hediffSet.HealHediff(hediff, amount);
			this.Notify_HediffChanged(hediff);
		}

		public void Notify_HediffChanged(Hediff hediff)
		{
			this.hediffSet.DirtyCache();
			this.CheckForStateChange(null, hediff);
		}

		public void RestorePart(BodyPartRecord part, Hediff diffException = null, bool checkStateChange = true)
		{
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
					hediff2.HediffRemoved();
				}
			}
			for (int j = 0; j < part.parts.Count; j++)
			{
				this.RestorePartRecursive(part.parts[j], diffException);
			}
		}

		public void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			if (this.pawn.Spawned)
			{
				if (!this.pawn.Position.Fogged())
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
			if (this.pawn.Faction != null)
			{
				this.pawn.Faction.Notify_MemberTookDamage(this.pawn, dinfo);
				if (Current.ProgramState == ProgramState.MapPlaying && this.pawn.Faction == Faction.OfPlayer && dinfo.Def.externalViolence)
				{
					Find.StoryWatcher.watcherDanger.Notify_ColonistHarmedExternally();
				}
			}
			if (this.pawn.RaceProps.IsFlesh && dinfo.Def.externalViolence)
			{
				Pawn pawn = dinfo.Instigator as Pawn;
				if (pawn != null)
				{
					if (pawn.HostileTo(this.pawn))
					{
						this.pawn.relations.canGetRescuedThought = true;
					}
					else if (this.pawn.RaceProps.Humanlike && pawn.RaceProps.Humanlike)
					{
						this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.HarmedMe, pawn);
					}
				}
			}
			absorbed = false;
		}

		public void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (this.ShouldBecomeDead())
			{
				if (!this.pawn.Destroyed)
				{
					this.Kill(new DamageInfo?(dinfo), null);
				}
				return;
			}
			this.ApplyAdditionalHediffs(dinfo, totalDamageDealt);
			if (this.Dead)
			{
				return;
			}
			if (this.pawn.Spawned)
			{
				this.pawn.mindState.mentalStateHandler.Notify_DamageTaken(dinfo);
			}
		}

		private void CheckForStateChange(DamageInfo? dinfo, Hediff hediff)
		{
			if (!this.Dead)
			{
				if (this.ShouldBecomeDead())
				{
					if (!this.pawn.Destroyed)
					{
						this.Kill(dinfo, hediff);
					}
					return;
				}
				if (!this.Downed)
				{
					if (!this.capacities.CanBeAwake || !this.capacities.CapableOf(PawnCapacityDefOf.Moving) || this.InPainShock)
					{
						float num = (!this.pawn.RaceProps.Animal) ? 0.67f : 0.47f;
						if (!this.forceIncap && (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer) && !this.pawn.IsPrisonerOfColony && this.pawn.RaceProps.IsFlesh && Rand.Value < num)
						{
							this.Kill(dinfo, null);
							return;
						}
						this.forceIncap = false;
						this.NewlyDowned(dinfo);
						return;
					}
					else if (!this.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						if (this.pawn.carrier != null && this.pawn.carrier.CarriedThing != null && this.pawn.jobs != null && this.pawn.CurJob != null)
						{
							this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
						}
						if (this.pawn.equipment != null && this.pawn.equipment.Primary != null)
						{
							if (this.pawn.InContainer)
							{
								ThingWithComps thingWithComps;
								this.pawn.equipment.TryTransferEquipmentToContainer(this.pawn.equipment.Primary, this.pawn.holder, out thingWithComps);
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
				else if (this.capacities.CanBeAwake && this.capacities.CapableOf(PawnCapacityDefOf.Moving) && !this.InPainShock)
				{
					this.NewlyUndowned();
					return;
				}
			}
		}

		private bool NeedActivityToLive(PawnCapacityDef act)
		{
			if (this.pawn.RaceProps.IsFlesh)
			{
				return act.lethalFlesh;
			}
			return act.lethalMechanoids;
		}

		private bool ShouldBecomeDead()
		{
			if (this.Dead)
			{
				return true;
			}
			if (this.pawn.RaceProps.IsFlesh)
			{
				for (int i = 0; i < this.hediffSet.hediffs.Count; i++)
				{
					if (this.hediffSet.hediffs[i].CauseDeathNow())
					{
						return true;
					}
				}
			}
			List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
			for (int j = 0; j < allDefsListForReading.Count; j++)
			{
				PawnCapacityDef pawnCapacityDef = allDefsListForReading[j];
				if (this.NeedActivityToLive(pawnCapacityDef) && !this.capacities.CapableOf(pawnCapacityDef))
				{
					return true;
				}
			}
			float num = PawnCapacityUtility.CalculatePartEfficiency(this.hediffSet, this.pawn.RaceProps.body.corePart, false);
			return num <= 0.0001f;
		}

		public void Kill(DamageInfo? dinfo, Hediff hediff)
		{
			ThoughtUtility.GiveThoughtsForPawnDied(this.pawn, dinfo, hediff);
			this.healthState = PawnHealthState.Dead;
			if (this.pawn.apparel != null)
			{
				this.pawn.apparel.Notify_PawnKilled(dinfo);
			}
			if (this.pawn.holder != null)
			{
				Pawn_CarryTracker pawn_CarryTracker = this.pawn.holder.owner as Pawn_CarryTracker;
				if (pawn_CarryTracker != null)
				{
					Thing thing;
					this.pawn.holder.TryDrop(this.pawn, pawn_CarryTracker.pawn.Position, ThingPlaceMode.Near, out thing, null);
				}
			}
			bool spawned = this.pawn.Spawned;
			bool inContainer = this.pawn.InContainer;
			if (spawned && dinfo.HasValue && dinfo.Value.Def.externalViolence)
			{
				LifeStageUtility.PlayNearestLifestageSound(this.pawn, (LifeStageAge ls) => ls.soundDeath, 1f);
			}
			this.surgeryBills.Clear();
			if (spawned)
			{
				this.pawn.DropAndForbidEverything(false);
			}
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
			if (inContainer)
			{
				thingContainer = this.pawn.holder;
			}
			bool flag2 = false;
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				flag2 = (Find.DesignationManager.DesignationOn(this.pawn, DesignationDefOf.Hunt) != null);
			}
			float num2 = 0f;
			Thing attachment = this.pawn.GetAttachment(ThingDefOf.Fire);
			if (attachment != null)
			{
				num2 = ((Fire)attachment).CurrentSize();
			}
			Corpse corpse = null;
			if (!PawnGenerator.IsBeingGenerated(this.pawn))
			{
				corpse = (Corpse)ThingMaker.MakeThing(this.pawn.RaceProps.corpseDef, null);
				corpse.innerPawn = this.pawn;
				this.pawn.corpse = corpse;
			}
			this.pawn.Destroy(DestroyMode.Kill);
			if (this.pawn.RaceProps.IsFlesh)
			{
				this.pawn.relations.Notify_PawnDied(dinfo);
			}
			this.pawn.meleeVerbs.Notify_PawnKilled();
			if (dinfo.HasValue && dinfo.Value.Instigator != null)
			{
				Pawn pawn = dinfo.Value.Instigator as Pawn;
				if (pawn != null)
				{
					RecordsUtility.Notify_PawnKilled(this.pawn, pawn);
				}
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
			if (this.pawn.IsColonist)
			{
				Find.StoryWatcher.statsRecord.colonistsKilled++;
			}
			if (corpse != null)
			{
				if (building_Grave != null)
				{
					corpse.innerPawn.ownership.ClaimGrave(building_Grave);
				}
				if (flag)
				{
					corpse.innerPawn.Drawer.renderer.wiggler.SetToCustomRotation(num + 180f);
				}
				if (spawned)
				{
					GenPlace.TryPlaceThing(corpse, this.pawn.Position, ThingPlaceMode.Direct, null);
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
						FireUtility.TryStartFireIn(corpse.Position, num2);
					}
				}
				else if (inContainer)
				{
					thingContainer.TryAdd(corpse);
				}
				else
				{
					corpse.Destroy(DestroyMode.Vanish);
				}
				if (this.pawn.RaceProps.DeathActionWorker != null && spawned)
				{
					this.pawn.RaceProps.DeathActionWorker.PawnDied(corpse);
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_PawnDied(corpse);
				}
			}
			PawnComponentsUtility.RemoveComponentsOnKilled(this.pawn);
			if (Current.ProgramState == ProgramState.MapPlaying && dinfo.HasValue)
			{
				Pawn pawn2 = dinfo.Value.Instigator as Pawn;
				if (pawn2 == null || pawn2.CurJob == null || !(pawn2.jobs.curDriver is JobDriver_Execute))
				{
					bool flag3 = true;
					if (pawn2 != null)
					{
						if (this.pawn.Faction != Faction.OfPlayer && this.pawn.kindDef.combatPower >= 280f && pawn2.Faction == Faction.OfPlayer)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledMajorColonyEnemy, new object[]
							{
								pawn2,
								this.pawn
							});
							flag3 = false;
						}
						else if (this.pawn.IsColonist)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledColonist, new object[]
							{
								pawn2,
								this.pawn
							});
							flag3 = false;
						}
						else if (this.pawn.Faction == Faction.OfPlayer && this.pawn.RaceProps.Animal)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledColonyAnimal, new object[]
							{
								pawn2,
								this.pawn
							});
							flag3 = false;
						}
					}
					if (this.pawn.Faction == Faction.OfPlayer)
					{
						if (!this.pawn.RaceProps.Humanlike && dinfo.Value.Instigator != null && dinfo.Value.Instigator.Faction == Faction.OfPlayer)
						{
							flag3 = false;
						}
						if (flag3)
						{
							TaleRecorder.RecordTale(TaleDefOf.KilledBy, new object[]
							{
								this.pawn,
								dinfo.Value
							});
						}
					}
				}
			}
			if (this.pawn.Faction != null && this.pawn == this.pawn.Faction.leader)
			{
				this.pawn.Faction.Notify_LeaderDied();
			}
			this.hediffSet.DirtyCache();
			PortraitsCache.SetDirty(this.pawn);
			for (int i = 0; i < this.hediffSet.hediffs.Count; i++)
			{
				this.hediffSet.hediffs[i].Notify_PawnDied();
			}
		}

		private void NewlyDowned(DamageInfo? dinfo)
		{
			if (this.Downed)
			{
				Log.Error(this.pawn + " was newly downed while already downed.");
				return;
			}
			this.healthState = PawnHealthState.Down;
			if (this.pawn.MentalState != null)
			{
				this.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
			}
			this.pawn.NewlyDowned();
			if (this.pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.MapPlaying)
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

		private void NewlyUndowned()
		{
			if (!this.Downed)
			{
				Log.Error(this.pawn + " was made undowned when already undowned.");
				return;
			}
			this.healthState = PawnHealthState.Mobile;
			if (this.pawn.guest != null)
			{
				this.pawn.guest.Notify_PawnUndowned();
			}
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				Messages.Message("MessageNoLongerDowned".Translate(new object[]
				{
					this.pawn.LabelCap
				}), this.pawn, MessageSound.Benefit);
			}
			if (this.pawn.Spawned && !this.pawn.InBed())
			{
				this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			PortraitsCache.SetDirty(this.pawn);
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
			if ((this.pawn.Spawned || (this.pawn.holder != null && this.pawn.holder.owner is Pawn_CarryTracker)) && this.pawn.PositionHeld.InBounds() && this.pawn.RaceProps.BloodDef != null && !this.pawn.InContainer)
			{
				FilthMaker.MakeFilth(this.pawn.PositionHeld, this.pawn.RaceProps.BloodDef, this.pawn.LabelIndefinite(), 1);
			}
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
					hediff2.HediffRemoved();
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
			bool flag2 = false;
			if (this.pawn.IsHashIntervalTick(5000) && !this.Downed && this.hediffSet.HasNaturallyHealingInjuries && (this.pawn.needs.food == null || !this.pawn.needs.food.Starving))
			{
				Hediff_Injury injury = (from x in this.hediffSet.GetHediffs<Hediff_Injury>()
				where x.IsNaturallyHealing()
				select x).RandomElement<Hediff_Injury>();
				BodyPartDamageInfo value = new BodyPartDamageInfo(injury);
				this.pawn.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, 1, null, new BodyPartDamageInfo?(value), null));
				flag2 = true;
			}
			if (this.pawn.IsHashIntervalTick(650) && this.hediffSet.HasTendedAndHealingInjury && (this.pawn.needs.food == null || !this.pawn.needs.food.Starving))
			{
				Hediff_Injury hediff_Injury = (from x in this.hediffSet.GetHediffs<Hediff_Injury>()
				where x.IsTendedAndHealing()
				select x).RandomElement<Hediff_Injury>();
				int amount = (!hediff_Injury.IsTendedWell()) ? 1 : 2;
				BodyPartDamageInfo value2 = new BodyPartDamageInfo(hediff_Injury);
				this.pawn.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, amount, null, new BodyPartDamageInfo?(value2), null));
				flag2 = true;
			}
			if (flag2 && !this.HasHediffsNeedingTendByColony(false) && !this.PrefersMedicalRest && !this.hediffSet.HasTendedAndHealingInjury && PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				Messages.Message("MessageFullyHealed".Translate(new object[]
				{
					this.pawn.LabelCap
				}), this.pawn, MessageSound.Benefit);
			}
			bool flag3 = this.hediffSet.BleedingRate >= 0.1f;
			if (this.pawn.RaceProps.IsFlesh && flag3)
			{
				float num = this.hediffSet.BleedingRate * this.pawn.BodySize;
				if (this.pawn.GetPosture() == PawnPosture.Standing)
				{
					num *= 0.008f;
				}
				else
				{
					num *= 0.0008f;
				}
				if (Rand.Value < num)
				{
					this.TryDropBloodFilth();
				}
			}
			if (this.pawn.Spawned && this.pawn.IsHashIntervalTick(400))
			{
				float num2 = this.pawn.ComfortableTemperatureRange().max + 150f;
				float temperatureForCell = GenTemperature.GetTemperatureForCell(this.pawn.Position);
				if (temperatureForCell > num2)
				{
					float num3 = temperatureForCell - num2;
					num3 = HediffGiver_Heatstroke.TemperatureOverageAdjustmentCurve.Evaluate(num3);
					int amount2 = Mathf.Max(Mathf.RoundToInt(num3 * 0.06f), 1);
					BodyPartDamageInfo value3 = new BodyPartDamageInfo(null, new BodyPartDepth?(BodyPartDepth.Outside));
					this.pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, amount2, null, new BodyPartDamageInfo?(value3), null));
					if (this.Dead)
					{
						return;
					}
				}
			}
			List<HediffGiverSetDef> hediffGiverSets = this.pawn.RaceProps.hediffGiverSets;
			if (hediffGiverSets != null)
			{
				for (int k = 0; k < hediffGiverSets.Count; k++)
				{
					List<HediffGiver> hediffGivers = hediffGiverSets[k].hediffGivers;
					if (this.pawn.IsHashIntervalTick(60))
					{
						for (int l = 0; l < hediffGivers.Count; l++)
						{
							HediffGiver hediffGiver = hediffGivers[l];
							if (hediffGiver.CheckGiveEverySecond(this.pawn) && PawnUtility.ShouldSendNotificationAbout(this.pawn))
							{
								Find.LetterStack.ReceiveLetter("LetterHediffFromRandomHediffGiverLabel".Translate(new object[]
								{
									this.pawn.LabelShort,
									hediffGiver.hediff.label
								}), "LetterHediffFromRandomHediffGiver".Translate(new object[]
								{
									this.pawn.LabelShort,
									hediffGiver.hediff.label
								}), LetterType.BadNonUrgent, this.pawn, null);
							}
							if (this.pawn.Dead)
							{
								return;
							}
						}
					}
				}
			}
		}

		private void ApplyAdditionalHediffs(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.additionalHediffs == null)
			{
				return;
			}
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
	}
}
