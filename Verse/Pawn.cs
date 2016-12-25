using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public class Pawn : ThingWithComps, IBillGiver, ITrader, IAttackTarget, ILoadReferenceable, IStrippable, IVerbOwner
	{
		public const int MaxMoveTicks = 450;

		private const int SleepDisturbanceMinInterval = 300;

		public PawnKindDef kindDef;

		private Name nameInt;

		public Gender gender;

		public Corpse corpse;

		public Pawn_AgeTracker ageTracker;

		public Pawn_HealthTracker health;

		public Pawn_RecordsTracker records;

		public Pawn_InventoryTracker inventory;

		public Pawn_MeleeVerbs meleeVerbs;

		public VerbTracker verbTracker;

		public Pawn_CarryTracker carrier;

		public Pawn_NeedsTracker needs;

		public Pawn_MindState mindState;

		public Pawn_PathFollower pather;

		public Pawn_Thinker thinker;

		public Pawn_JobTracker jobs;

		public Pawn_StanceTracker stances;

		public Pawn_NativeVerbs natives;

		public Pawn_FilthTracker filth;

		public Pawn_EquipmentTracker equipment;

		public Pawn_ApparelTracker apparel;

		public Pawn_Ownership ownership;

		public Pawn_SkillTracker skills;

		public Pawn_StoryTracker story;

		public Pawn_GuestTracker guest;

		public Pawn_WorkSettings workSettings;

		public Pawn_TraderTracker trader;

		public Pawn_TrainingTracker training;

		public Pawn_CallTracker caller;

		public Pawn_RelationsTracker relations;

		public Pawn_InteractionsTracker interactions;

		public Pawn_PlayerSettings playerSettings;

		public Pawn_OutfitTracker outfits;

		public Pawn_DrugPolicyTracker drugs;

		public Pawn_TimetableTracker timetable;

		public Pawn_DraftController drafter;

		private Pawn_DrawTracker drawer;

		public Queue<Job> jobQueue;

		private int lastSleepDisturbedTick;

		public Name Name
		{
			get
			{
				return this.nameInt;
			}
			set
			{
				this.nameInt = value;
			}
		}

		public string NameStringShort
		{
			get
			{
				if (this.Name != null)
				{
					return this.Name.ToStringShort;
				}
				return this.KindLabel;
			}
		}

		public RaceProperties RaceProps
		{
			get
			{
				return this.def.race;
			}
		}

		public Job CurJob
		{
			get
			{
				return (this.jobs == null) ? null : this.jobs.curJob;
			}
		}

		public bool Downed
		{
			get
			{
				return this.health.Downed;
			}
		}

		public bool Dead
		{
			get
			{
				return this.health.Dead;
			}
		}

		public string KindLabel
		{
			get
			{
				return GenLabel.BestKindLabel(this, false, false);
			}
		}

		public bool InMentalState
		{
			get
			{
				return !this.Dead && this.mindState.mentalStateHandler.InMentalState;
			}
		}

		public MentalState MentalState
		{
			get
			{
				if (this.Dead)
				{
					return null;
				}
				return this.mindState.mentalStateHandler.CurState;
			}
		}

		public MentalStateDef MentalStateDef
		{
			get
			{
				if (this.Dead)
				{
					return null;
				}
				return this.mindState.mentalStateHandler.CurStateDef;
			}
		}

		public bool InAggroMentalState
		{
			get
			{
				return !this.Dead && this.mindState.mentalStateHandler.InMentalState && this.mindState.mentalStateHandler.CurStateDef.IsAggro;
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				return this.Drawer.DrawPos;
			}
		}

		public VerbTracker VerbTracker
		{
			get
			{
				return this.verbTracker;
			}
		}

		public List<VerbProperties> VerbProperties
		{
			get
			{
				return this.def.Verbs;
			}
		}

		public bool IsColonist
		{
			get
			{
				return base.Faction != null && base.Faction.IsPlayer && this.RaceProps.Humanlike;
			}
		}

		public Faction HostFaction
		{
			get
			{
				if (this.guest == null)
				{
					return null;
				}
				return this.guest.HostFaction;
			}
		}

		public bool Drafted
		{
			get
			{
				return this.drafter != null && this.drafter.Drafted;
			}
		}

		public bool IsPrisoner
		{
			get
			{
				return this.guest != null && this.guest.IsPrisoner;
			}
		}

		public bool IsPrisonerOfColony
		{
			get
			{
				return this.guest != null && this.guest.IsPrisoner && this.guest.HostFaction.IsPlayer;
			}
		}

		public bool IsColonistPlayerControlled
		{
			get
			{
				return base.Spawned && this.IsColonist && this.MentalStateDef == null && this.HostFaction == null;
			}
		}

		public IEnumerable<IntVec3> IngredientStackCells
		{
			get
			{
				yield return this.InteractionCell;
			}
		}

		public bool InContainer
		{
			get
			{
				return this.holder != null && !(this.holder.owner is Pawn_CarryTracker);
			}
		}

		public Pawn CarriedBy
		{
			get
			{
				if (this.holder == null)
				{
					return null;
				}
				Pawn_CarryTracker pawn_CarryTracker = this.holder.owner as Pawn_CarryTracker;
				if (pawn_CarryTracker != null)
				{
					return pawn_CarryTracker.pawn;
				}
				return null;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (this.Name == null)
				{
					return this.KindLabel;
				}
				if (this.story == null || this.story.adulthood == null)
				{
					return this.Name.ToStringShort;
				}
				return this.Name.ToStringShort + ", " + this.story.adulthood.titleShort;
			}
		}

		public override string LabelShort
		{
			get
			{
				if (this.Name != null)
				{
					return this.Name.ToStringShort;
				}
				return this.LabelNoCount;
			}
		}

		public Pawn_DrawTracker Drawer
		{
			get
			{
				if (this.drawer == null)
				{
					this.drawer = new Pawn_DrawTracker(this);
				}
				return this.drawer;
			}
		}

		public BillStack BillStack
		{
			get
			{
				return this.health.surgeryBills;
			}
		}

		public override IntVec3 InteractionCell
		{
			get
			{
				Building_Bed building_Bed = this.CurrentBed();
				if (building_Bed != null)
				{
					IntVec3 position = base.Position;
					IntVec3 position2 = base.Position;
					IntVec3 position3 = base.Position;
					IntVec3 position4 = base.Position;
					if (building_Bed.Rotation.IsHorizontal)
					{
						position.z++;
						position2.z--;
						position3.x--;
						position4.x++;
					}
					else
					{
						position.x--;
						position2.x++;
						position3.z++;
						position4.z--;
					}
					if (position.Standable())
					{
						if (position.GetThingList().Find((Thing x) => x.def.IsBed) == null)
						{
							return position;
						}
					}
					if (position2.Standable())
					{
						if (position2.GetThingList().Find((Thing x) => x.def.IsBed) == null)
						{
							return position2;
						}
					}
					if (position3.Standable())
					{
						if (position3.GetThingList().Find((Thing x) => x.def.IsBed) == null)
						{
							return position3;
						}
					}
					if (position4.Standable())
					{
						if (position4.GetThingList().Find((Thing x) => x.def.IsBed) == null)
						{
							return position4;
						}
					}
					if (position.Standable())
					{
						return position;
					}
					if (position2.Standable())
					{
						return position2;
					}
					if (position3.Standable())
					{
						return position3;
					}
					if (position4.Standable())
					{
						return position4;
					}
				}
				return base.InteractionCell;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				return (this.trader == null) ? null : this.trader.traderKind;
			}
		}

		public IEnumerable<Thing> Goods
		{
			get
			{
				return this.trader.Goods;
			}
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy
		{
			get
			{
				return this.trader.ColonyThingsWillingToBuy;
			}
		}

		public int RandomPriceFactorSeed
		{
			get
			{
				return this.trader.RandomPriceFactorSeed;
			}
		}

		public string TraderName
		{
			get
			{
				return this.trader.TraderName;
			}
		}

		public bool CanTradeNow
		{
			get
			{
				return this.trader != null && this.trader.CanTradeNow;
			}
		}

		public float BodySize
		{
			get
			{
				return this.ageTracker.CurLifeStage.bodySizeFactor * this.RaceProps.baseBodySize;
			}
		}

		public float HealthScale
		{
			get
			{
				return this.ageTracker.CurLifeStage.healthScaleFactor * this.RaceProps.baseHealthScale;
			}
		}

		public int TicksPerMoveCardinal
		{
			get
			{
				return this.TicksPerMove(false);
			}
		}

		public int TicksPerMoveDiagonal
		{
			get
			{
				return this.TicksPerMove(true);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<PawnKindDef>(ref this.kindDef, "kindDef");
			Scribe_Values.LookValue<Gender>(ref this.gender, "gender", Gender.Male, false);
			Scribe_Deep.LookDeep<Name>(ref this.nameInt, "name", new object[0]);
			Scribe_References.LookReference<Corpse>(ref this.corpse, "corpse", false);
			Scribe_Deep.LookDeep<Pawn_MindState>(ref this.mindState, "mindState", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_JobTracker>(ref this.jobs, "jobs", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_StanceTracker>(ref this.stances, "stances", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_NativeVerbs>(ref this.natives, "natives", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_MeleeVerbs>(ref this.meleeVerbs, "meleeVerbs", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_PathFollower>(ref this.pather, "pather", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_CarryTracker>(ref this.carrier, "carrier", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_ApparelTracker>(ref this.apparel, "apparel", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_StoryTracker>(ref this.story, "story", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_EquipmentTracker>(ref this.equipment, "equipment", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_DraftController>(ref this.drafter, "drafter", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_AgeTracker>(ref this.ageTracker, "ageTracker", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_HealthTracker>(ref this.health, "healthTracker", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_RecordsTracker>(ref this.records, "records", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_InventoryTracker>(ref this.inventory, "inventory", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_FilthTracker>(ref this.filth, "filth", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_NeedsTracker>(ref this.needs, "needs", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_GuestTracker>(ref this.guest, "guest", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_RelationsTracker>(ref this.relations, "social", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_Ownership>(ref this.ownership, "ownership", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_InteractionsTracker>(ref this.interactions, "interactions", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_SkillTracker>(ref this.skills, "skills", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_WorkSettings>(ref this.workSettings, "workSettings", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_TraderTracker>(ref this.trader, "trader", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_OutfitTracker>(ref this.outfits, "outfits", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_DrugPolicyTracker>(ref this.drugs, "drugs", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_TimetableTracker>(ref this.timetable, "timetable", new object[0]);
			Scribe_Deep.LookDeep<Pawn_PlayerSettings>(ref this.playerSettings, "playerSettings", new object[]
			{
				this
			});
			Scribe_Deep.LookDeep<Pawn_TrainingTracker>(ref this.training, "training", new object[]
			{
				this
			});
		}

		public override string ToString()
		{
			if (this.story != null)
			{
				return this.NameStringShort;
			}
			if (this.thingIDNumber > 0)
			{
				return base.ThingID;
			}
			if (this.kindDef != null)
			{
				return this.KindLabel + "_" + base.ThingID;
			}
			if (this.def != null)
			{
				return base.ThingID;
			}
			return base.GetType().ToString();
		}

		public override void SpawnSetup()
		{
			if (this.Dead)
			{
				Log.Warning("Tried to spawn Dead Pawn " + this + ". Replacing with corpse.");
				Corpse corpse = (Corpse)ThingMaker.MakeThing(this.RaceProps.corpseDef, null);
				corpse.innerPawn = this;
				this.corpse = corpse;
				GenSpawn.Spawn(corpse, base.Position);
				return;
			}
			base.SpawnSetup();
			if (Find.WorldPawns.Contains(this))
			{
				Find.WorldPawns.RemovePawn(this);
			}
			PawnComponentsUtility.AddComponentsForSpawn(this);
			if (!PawnUtility.InValidState(this))
			{
				Log.Error("Pawn " + this.ToString() + " spawned in invalid state. Destroying...");
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			this.Drawer.Notify_Spawned();
			this.pather.ResetToCurrentPosition();
			Find.MapPawns.RegisterPawn(this);
			if (this.RaceProps.IsFlesh)
			{
				this.relations.everSeenByPlayer = true;
			}
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
			if (this.needs != null && this.needs.mood != null && this.needs.mood.recentMemory != null)
			{
				this.needs.mood.recentMemory.Notify_Spawned();
			}
		}

		public override void DrawAt(Vector3 drawLoc)
		{
			this.Drawer.renderer.RenderPawnAt(drawLoc, RotDrawMode.Fresh);
		}

		public override void DrawGUIOverlay()
		{
			this.Drawer.ui.DrawPawnGUIOverlay();
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (this.IsColonistPlayerControlled && this.pather.curPath != null)
			{
				this.pather.curPath.DrawPath(this);
			}
			this.mindState.priorityWork.DrawExtraSelectionOverlays(this);
		}

		public override void TickRare()
		{
			base.TickRare();
			if (this.apparel != null)
			{
				this.apparel.ApparelTrackerTickRare();
			}
		}

		public override void Tick()
		{
			if (DebugSettings.noAnimals && this.RaceProps.Animal)
			{
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			base.Tick();
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				this.TickRare();
			}
			if (base.Spawned)
			{
				this.pather.PatherTick();
			}
			this.Drawer.DrawTrackerTick();
			this.ageTracker.AgeTick();
			this.health.HealthTick();
			this.records.RecordsTick();
			if (base.Spawned)
			{
				this.stances.StanceTrackerTick();
			}
			if (base.Spawned)
			{
				this.verbTracker.VerbsTick();
			}
			if (base.Spawned)
			{
				this.natives.NativeVerbsTick();
			}
			if (!this.Dead)
			{
				this.mindState.MindTick();
			}
			if (this.equipment != null)
			{
				this.equipment.EquipmentTrackerTick();
			}
			if (this.apparel != null)
			{
				this.apparel.ApparelTrackerTick();
			}
			if (base.Spawned)
			{
				this.jobs.JobTrackerTick();
			}
			if (!this.Dead)
			{
				this.carrier.CarryHandsTick();
			}
			if (this.interactions != null)
			{
				this.interactions.InteractionsTrackerTick();
			}
			if (!this.Dead)
			{
				this.needs.NeedsTrackerTick();
			}
			if (this.caller != null)
			{
				this.caller.CallTrackerTick();
			}
			if (this.skills != null)
			{
				this.skills.SkillsTick();
			}
			if (this.inventory != null)
			{
				this.inventory.InventoryTrackerTick();
			}
			if (this.drafter != null)
			{
				this.drafter.PlayerControllerTick();
			}
			if (this.relations != null)
			{
				this.relations.SocialTrackerTick();
			}
			if (this.RaceProps.Humanlike)
			{
				this.guest.GuestTrackerTick();
			}
		}

		public void Notify_Teleported(bool endCurrentJob = true)
		{
			this.Drawer.tweener.ResetToPosition();
			this.pather.Notify_Teleported_Int();
			if (endCurrentJob && this.jobs != null && this.jobs.curJob != null)
			{
				this.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(dinfo, out absorbed);
			if (absorbed)
			{
				return;
			}
			this.health.PreApplyDamage(dinfo, out absorbed);
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.externalViolence)
			{
				this.records.AddTo(RecordDefOf.DamageTaken, totalDamageDealt);
			}
			this.health.PostApplyDamage(dinfo, totalDamageDealt);
		}

		public override Thing SplitOff(int count)
		{
			if (count <= 0 || count >= this.stackCount)
			{
				return base.SplitOff(count);
			}
			throw new NotImplementedException("Split off on Pawns is not supported.");
		}

		private int TicksPerMove(bool diagonal)
		{
			float num = this.GetStatValue(StatDefOf.MoveSpeed, true);
			if (this.HostFaction != null && !PrisonBreakUtility.IsPrisonBreaking(this))
			{
				num *= 0.35f;
			}
			if (this.carrier != null && this.carrier.CarriedThing != null && this.carrier.CarriedThing.def.category == ThingCategory.Pawn)
			{
				num *= 0.6f;
			}
			float num2 = num / 60f;
			float num3 = 1f / num2;
			if (!Find.RoofGrid.Roofed(base.Position))
			{
				num3 /= Find.WeatherManager.CurMoveSpeedMultiplier;
			}
			if (diagonal)
			{
				num3 *= 1.41421f;
			}
			int value = Mathf.RoundToInt(num3);
			return Mathf.Clamp(value, 1, 450);
		}

		public override void DeSpawn()
		{
			base.DeSpawn();
			if (this.jobs != null && this.jobs.curJob != null)
			{
				this.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			if (this.pather != null)
			{
				this.pather.StopDead();
			}
			this.ClearReservations();
			Find.MapPawns.DeRegisterPawn(this);
			PawnComponentsUtility.RemoveComponentsOnDespawned(this);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if ((mode == DestroyMode.Kill || mode == DestroyMode.Vanish) && Current.ProgramState == ProgramState.MapPlaying && base.Faction != null && base.Faction == Faction.OfPlayer)
			{
				Find.StoryWatcher.watcherRampUp.Notify_ColonistIncappedOrKilled(this);
			}
			base.Destroy(mode);
			if (this.ownership != null)
			{
				this.ownership.UnclaimAll();
			}
			this.ClearMind(false);
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				this.ClearReservations();
				Lord lord = this.GetLord();
				if (lord != null)
				{
					PawnLostCondition cond = (mode != DestroyMode.Kill) ? PawnLostCondition.Vanished : PawnLostCondition.IncappedOrKilled;
					lord.Notify_PawnLost(this, cond);
				}
				Find.MapPawns.DeRegisterPawn(this);
				Find.GameEnder.CheckGameOver();
				Find.TaleManager.Notify_PawnDestroyed(this);
			}
			foreach (Pawn current in from p in PawnUtility.AllPawnsMapOrWorldAlive
			where p.playerSettings != null && p.playerSettings.master == this
			select p)
			{
				current.playerSettings.master = null;
			}
			if (mode != DestroyMode.Kill)
			{
				if (this.equipment != null)
				{
					this.equipment.DestroyAllEquipment(DestroyMode.Vanish);
				}
				this.inventory.DestroyAll(DestroyMode.Vanish);
				if (this.apparel != null)
				{
					this.apparel.DestroyAll(DestroyMode.Vanish);
				}
			}
			WorldPawns worldPawns = Find.WorldPawns;
			if (!worldPawns.IsBeingDiscarded(this) && !worldPawns.Contains(this))
			{
				worldPawns.PassToWorld(this, PawnDiscardDecideMode.Decide);
			}
		}

		public override void Discard()
		{
			if (Find.WorldPawns.Contains(this))
			{
				Log.Warning("Tried to discard a world pawn " + this + ".");
				return;
			}
			base.Discard();
			if (this.relations != null)
			{
				this.relations.ClearAllRelations();
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.PlayLog.Notify_PawnDiscarded(this);
				Find.TaleManager.Notify_PawnDiscarded(this);
			}
		}

		public void ExitMap()
		{
			Lord lord = this.GetLord();
			if (lord != null)
			{
				lord.Notify_PawnLost(this, PawnLostCondition.ExitedMap);
			}
			if (this.carrier != null && this.carrier.CarriedThing != null)
			{
				Pawn pawn = this.carrier.CarriedThing as Pawn;
				if (pawn != null)
				{
					if (base.Faction != null && base.Faction != pawn.Faction)
					{
						base.Faction.kidnapped.KidnapPawn(pawn, this);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
				else
				{
					this.carrier.CarriedThing.Destroy(DestroyMode.Vanish);
				}
				this.carrier.container.Clear();
			}
			if (this.HostFaction != null && this.guest != null && (this.guest.released || !this.IsPrisoner) && !this.InMentalState && this.health.hediffSet.BleedingRate < 0.001f && base.Faction.def.appreciative && !base.Faction.def.hidden)
			{
				Messages.Message("MessagePawnExitMapRelationsGain".Translate(new object[]
				{
					this.LabelShort,
					base.Faction.Name,
					15f.ToString("F0")
				}), MessageSound.Benefit);
				base.Faction.AffectGoodwillWith(this.HostFaction, 15f);
			}
			if (this.ownership != null)
			{
				this.ownership.UnclaimAll();
			}
			if (this.guest != null)
			{
				this.guest.SetGuestStatus(null, false);
			}
			if (base.Spawned)
			{
				this.DeSpawn();
			}
			this.ClearMind(false);
			this.ClearReservations();
			Find.WorldPawns.PassToWorld(this, PawnDiscardDecideMode.Decide);
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			this.DropAndForbidEverything(false);
			if (this.ownership != null)
			{
				this.ownership.UnclaimAll();
			}
			if (this.guest != null)
			{
				this.guest.SetGuestStatus(null, false);
			}
			if (action == TradeAction.PlayerBuys)
			{
				this.SetFaction(Faction.OfPlayer, null);
			}
			else if (action == TradeAction.PlayerSells)
			{
				if (this.RaceProps.Humanlike)
				{
					TaleRecorder.RecordTale(TaleDefOf.SoldPrisoner, new object[]
					{
						playerNegotiator,
						this,
						trader
					});
				}
				if (base.Faction != null)
				{
					this.SetFaction(null, null);
				}
				if (this.RaceProps.IsFlesh)
				{
					this.relations.Notify_PawnSold(playerNegotiator);
				}
			}
			this.ClearMind(false);
			this.ClearReservations();
		}

		public void PreKidnapped(Pawn kidnapper)
		{
			if (this.IsColonist)
			{
				TaleRecorder.RecordTale(TaleDefOf.KidnappedColonist, new object[]
				{
					kidnapper,
					this
				});
			}
			if (this.ownership != null)
			{
				this.ownership.UnclaimAll();
			}
			if (this.guest != null)
			{
				this.guest.SetGuestStatus(null, false);
			}
			if (this.RaceProps.IsFlesh)
			{
				this.relations.Notify_PawnKidnapped();
			}
			this.ClearMind(false);
			this.ClearReservations();
		}

		public void NewlyDowned()
		{
			if (base.Spawned)
			{
				if (base.Faction == Faction.OfPlayer)
				{
					Find.StoryWatcher.watcherRampUp.Notify_ColonistIncappedOrKilled(this);
				}
				this.DropAndForbidEverything(true);
				this.stances.CancelBusyStanceSoft();
			}
			this.ClearMind(true);
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				this.ClearReservations();
				Lord lord = this.GetLord();
				if (lord != null)
				{
					lord.Notify_PawnLost(this, PawnLostCondition.IncappedOrKilled);
				}
			}
			if (this.Drafted)
			{
				this.drafter.Drafted = false;
			}
			PortraitsCache.SetDirty(this);
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			if (newFaction == base.Faction)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Used ChangePawnFactionTo to change ",
					this,
					" to same faction ",
					newFaction
				}));
				return;
			}
			if (this.guest != null)
			{
				this.guest.SetGuestStatus(null, false);
			}
			Find.MapPawns.DeRegisterPawn(this);
			Find.PawnDestinationManager.RemovePawnFromSystem(this);
			Find.DesignationManager.RemoveAllDesignationsOn(this, false);
			if (newFaction == Faction.OfPlayer || base.Faction == Faction.OfPlayer)
			{
				Find.ColonistBar.MarkColonistsListDirty();
			}
			Lord lord = this.GetLord();
			if (lord != null)
			{
				lord.Notify_PawnLost(this, PawnLostCondition.ChangedFaction);
			}
			if (newFaction == Faction.OfPlayer && this.RaceProps.Humanlike)
			{
				this.kindDef = newFaction.def.basicMemberKind;
			}
			base.SetFaction(newFaction, null);
			PawnComponentsUtility.AddAndRemoveDynamicComponents(this, false);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				if (this.workSettings != null)
				{
					this.workSettings.EnableAndInitialize();
				}
				Find.Storyteller.intenderPopulation.Notify_PopulationGained();
			}
			if (this.Drafted)
			{
				this.drafter.Drafted = false;
			}
			Reachability.ClearCache();
			this.health.surgeryBills.Clear();
			if (base.Spawned)
			{
				Find.MapPawns.RegisterPawn(this);
			}
			this.GenerateNecessaryName();
			if (this.playerSettings != null)
			{
				this.playerSettings.medCare = ((!this.RaceProps.Humanlike) ? (this.playerSettings.medCare = MedicalCareCategory.NoMeds) : MedicalCareCategory.Best);
			}
			this.ClearMind(true);
			if (!this.Dead && this.needs.mood != null)
			{
				this.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			Find.AttackTargetsCache.UpdateTarget(this);
			Find.GameEnder.CheckGameOver();
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
		}

		public void ClearMind(bool ifLayingKeepLaying = false)
		{
			if (this.jobQueue != null)
			{
				this.jobQueue.Clear();
			}
			if (this.pather != null)
			{
				this.pather.StopDead();
			}
			if (this.mindState != null)
			{
				this.mindState.Reset();
			}
			if (this.jobs != null)
			{
				this.jobs.StopAll(ifLayingKeepLaying);
			}
		}

		public void ClearReservations()
		{
			Find.PawnDestinationManager.RemovePawnFromSystem(this);
			Find.Reservations.ReleaseAllClaimedBy(this);
			Find.PhysicalInteractionReservations.ReleaseAllClaimedBy(this);
			Find.AttackTargetReservations.ReleaseAllClaimedBy(this);
		}

		public void DropAndForbidEverything(bool keepInventoryAndEquipmentIfInBed = false)
		{
			if (this.InContainer)
			{
				if (this.carrier != null && this.carrier.CarriedThing != null)
				{
					this.holder.TryAdd(this.carrier.CarriedThing);
					this.carrier.container.Clear();
				}
				if (this.equipment != null && this.equipment.Primary != null)
				{
					ThingWithComps thingWithComps;
					this.equipment.TryTransferEquipmentToContainer(this.equipment.Primary, this.holder, out thingWithComps);
				}
				if (this.inventory != null)
				{
					foreach (Thing current in this.inventory.container)
					{
						this.holder.TryAdd(current);
					}
					this.inventory.container.Clear();
				}
			}
			else
			{
				if (this.carrier != null && this.carrier.CarriedThing != null)
				{
					Thing thing;
					this.carrier.TryDropCarriedThing(base.PositionHeld, ThingPlaceMode.Near, out thing, null);
				}
				if (!keepInventoryAndEquipmentIfInBed || !this.InBed())
				{
					if (this.equipment != null)
					{
						this.equipment.DropAllEquipment(base.PositionHeld, true);
					}
					if (this.inventory != null && this.inventory.container.TotalStackCount > 0)
					{
						this.inventory.DropAllNearPawn(base.PositionHeld, true);
					}
				}
			}
		}

		public void GenerateNecessaryName()
		{
			if (base.Faction != Faction.OfPlayer || !this.RaceProps.Animal)
			{
				return;
			}
			if (this.Name == null || this.Name.Numerical)
			{
				if (Rand.Value < this.RaceProps.nameOnTameChance)
				{
					this.Name = NameGenerator.GeneratePawnName(this, NameStyle.Full, null);
				}
				else
				{
					this.Name = NameGenerator.GeneratePawnName(this, NameStyle.Numeric, null);
				}
			}
		}

		public Verb TryGetAttackVerb(bool allowManualCastWeapons = false)
		{
			if (this.equipment != null && this.equipment.Primary != null && (!this.equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast || (this.CurJob != null && this.CurJob.def != JobDefOf.WaitCombat) || allowManualCastWeapons))
			{
				return this.equipment.PrimaryEq.PrimaryVerb;
			}
			return this.meleeVerbs.TryGetMeleeVerb();
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
		{
			foreach (Thing t in base.ButcherProducts(butcher, efficiency))
			{
				yield return t;
			}
			if (this.RaceProps.meatDef != null)
			{
				int meatCount = GenMath.RoundRandom(this.GetStatValue(StatDefOf.MeatAmount, true) * efficiency);
				if (meatCount > 0)
				{
					Thing meat = ThingMaker.MakeThing(this.RaceProps.meatDef, null);
					meat.stackCount = meatCount;
					yield return meat;
				}
			}
			if (this.RaceProps.leatherDef != null)
			{
				int leatherCount = GenMath.RoundRandom(this.GetStatValue(StatDefOf.LeatherAmount, true) * efficiency);
				if (leatherCount > 0)
				{
					Thing leather = ThingMaker.MakeThing(this.RaceProps.leatherDef, null);
					leather.stackCount = leatherCount;
					yield return leather;
				}
			}
		}

		public string MainDesc(bool writeAge)
		{
			string text = GenLabel.BestKindLabel(this, true, true);
			if (base.Faction != null && !base.Faction.def.hidden)
			{
				text = "PawnMainDescFactionedWrap".Translate(new object[]
				{
					text,
					base.Faction.Name
				});
			}
			if (writeAge && this.ageTracker != null)
			{
				text = text + ", " + "AgeIndicator".Translate(new object[]
				{
					this.ageTracker.AgeNumberString
				});
			}
			return text.CapitalizeFirst();
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(this.MainDesc(true));
			if (this.TraderKind != null)
			{
				stringBuilder.AppendLine(this.TraderKind.LabelCap);
			}
			if (this.MentalState != null)
			{
				stringBuilder.AppendLine(this.MentalState.InspectLine);
			}
			if (this.equipment != null && this.equipment.Primary != null)
			{
				stringBuilder.AppendLine("Equipped".Translate() + ": " + ((this.equipment.Primary == null) ? "EquippedNothing".Translate() : this.equipment.Primary.Label));
			}
			if (this.carrier != null && this.carrier.CarriedThing != null)
			{
				stringBuilder.Append("Carrying".Translate() + ": ");
				stringBuilder.AppendLine(this.carrier.CarriedThing.LabelCap);
			}
			string text = null;
			Lord lord = this.GetLord();
			if (lord != null && lord.LordJob != null)
			{
				text = lord.LordJob.GetReport();
			}
			if (this.jobs.curJob != null)
			{
				try
				{
					string report = this.jobs.curDriver.GetReport();
					if (!text.NullOrEmpty())
					{
						text = text + ": " + report;
					}
					else
					{
						text = report.CapitalizeFirst();
					}
				}
				catch (Exception arg)
				{
					stringBuilder.AppendLine("JobDriver.GetReport() exception: " + arg);
				}
			}
			if (!text.NullOrEmpty())
			{
				stringBuilder.AppendLine(text);
			}
			if (this.IsPrisonerOfColony && !PrisonBreakUtility.IsPrisonBreaking(this))
			{
				stringBuilder.AppendLine("InRestraints".Translate());
			}
			stringBuilder.Append(base.InspectStringPartsFromComps());
			return stringBuilder.ToString();
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (this.IsColonistPlayerControlled)
			{
				foreach (Gizmo c in base.GetGizmos())
				{
					yield return c;
				}
				if (this.drafter != null)
				{
					foreach (Gizmo c2 in this.drafter.GetGizmos())
					{
						yield return c2;
					}
				}
				if (this.equipment != null)
				{
					foreach (Gizmo g in this.equipment.GetGizmos())
					{
						yield return g;
					}
				}
				if (this.apparel != null)
				{
					foreach (Gizmo g2 in this.apparel.GetGizmos())
					{
						yield return g2;
					}
				}
				if (this.playerSettings != null)
				{
					foreach (Gizmo g3 in this.playerSettings.GetGizmos())
					{
						yield return g3;
					}
				}
				foreach (Gizmo g4 in this.mindState.priorityWork.GetGizmos(this))
				{
					yield return g4;
				}
			}
		}

		[DebuggerHidden]
		public virtual IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
		{
		}

		public override TipSignal GetTooltip()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.LabelCap);
			string text = string.Empty;
			if (this.gender != Gender.None)
			{
				text = this.gender.GetLabel();
			}
			if (!this.LabelCap.EqualsIgnoreCase(this.KindLabel))
			{
				if (text != string.Empty)
				{
					text += " ";
				}
				text += this.KindLabel;
			}
			if (text != string.Empty)
			{
				stringBuilder.Append(" (" + text + ")");
			}
			stringBuilder.AppendLine();
			if (this.equipment != null && this.equipment.Primary != null)
			{
				stringBuilder.AppendLine(this.equipment.Primary.LabelCap);
			}
			stringBuilder.AppendLine(HealthUtility.GetGeneralConditionLabel(this));
			return new TipSignal(stringBuilder.ToString(), this.thingIDNumber * 152317, TooltipPriority.Pawn);
		}

		public bool CurrentlyUsable()
		{
			return (this.InBed() || (!this.RaceProps.IsFlesh && this.Downed)) && this.InteractionCell.IsValid;
		}

		public bool AnythingToStrip()
		{
			return (this.equipment != null && this.equipment.HasAnything()) || (this.apparel != null && this.apparel.WornApparelCount > 0) || (this.inventory != null && this.inventory.container.Count > 0);
		}

		public void Strip()
		{
			if (this.equipment != null)
			{
				this.equipment.DropAllEquipment(base.Position, false);
			}
			if (this.apparel != null)
			{
				this.apparel.DropAll(base.Position, false);
			}
			if (this.inventory != null)
			{
				this.inventory.DropAllNearPawn(base.Position, false);
			}
		}

		public void AddToStock(Thing thing)
		{
			this.trader.AddToStock(thing);
		}

		public void GiveSoldThingToBuyer(Thing toGive, Thing originalThingFromStock)
		{
			this.trader.GiveSoldThingToBuyer(toGive, originalThingFromStock);
		}

		public void HearClamor(Pawn source, ClamorType type)
		{
			if (this.Dead)
			{
				return;
			}
			if (type == ClamorType.Movement && this.needs.mood != null && !this.Awake() && base.Faction == Faction.OfPlayer && Find.TickManager.TicksGame > this.lastSleepDisturbedTick + 300 && !LovePartnerRelationUtility.LovePartnerRelationExists(this, source))
			{
				this.lastSleepDisturbedTick = Find.TickManager.TicksGame;
				this.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleepDisturbed, null);
				if (!base.Position.Fogged())
				{
					MoteMaker.ThrowMetaIcon(base.Position, ThingDefOf.Mote_SleepDisturbed);
				}
			}
			if (type == ClamorType.Harm && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction == source.Faction && this.HostFaction == null)
			{
				this.mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (this.CurJob != null)
				{
					this.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}

		public bool CheckAcceptArrest(Pawn arrester)
		{
			if (this.health.Downed)
			{
				return true;
			}
			if (this.story != null && this.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return true;
			}
			if (base.Faction != null && base.Faction != arrester.factionInt)
			{
				base.Faction.Notify_MemberCaptured(this, arrester.Faction);
			}
			if (Rand.Value < 0.5f)
			{
				return true;
			}
			Messages.Message("MessageRefusedArrest".Translate(new object[]
			{
				this.LabelShort
			}), this, MessageSound.SeriousAlert);
			if (base.Faction == null || !arrester.HostileTo(this))
			{
				this.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false);
			}
			return false;
		}

		public void QueueJob(Job newJob)
		{
			if (this.jobQueue == null)
			{
				this.jobQueue = new Queue<Job>();
			}
			if (newJob == null)
			{
				Log.Error("Cannot queue null job");
				return;
			}
			this.jobQueue.Enqueue(newJob);
		}

		public bool ThreatDisabled()
		{
			return (!this.InMentalState && this.GetCaravanRole() == TraderCaravanRole.Carrier) || base.Position.Fogged() || this.Downed;
		}

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (this.InAggroMentalState || (base.Faction.HostileTo(Faction.OfPlayer) && this.HostFaction == null && !this.Downed && !this.InMentalState))
			{
				reason = "Enemies".Translate();
				return true;
			}
			reason = null;
			return false;
		}
	}
}
