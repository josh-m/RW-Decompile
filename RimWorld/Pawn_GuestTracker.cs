using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_GuestTracker : IExposable
	{
		private const int CheckInitiatePrisonBreakIntervalTicks = 2500;

		private Pawn pawn;

		private bool getsFoodInt = true;

		public PrisonerInteractionMode interactionMode;

		private Faction hostFactionInt;

		public bool isPrisonerInt;

		public bool released;

		public int MinInteractionInterval = 7500;

		public Faction HostFaction
		{
			get
			{
				return this.hostFactionInt;
			}
		}

		public bool GetsFood
		{
			get
			{
				if (this.HostFaction == null)
				{
					Log.Error("GetsFood without host faction.");
					return true;
				}
				return this.getsFoodInt;
			}
			set
			{
				this.getsFoodInt = value;
			}
		}

		public bool ShouldBeBroughtFood
		{
			get
			{
				return this.GetsFood && this.interactionMode != PrisonerInteractionMode.Execution && (this.interactionMode != PrisonerInteractionMode.Release || this.pawn.Downed);
			}
		}

		public bool IsPrisoner
		{
			get
			{
				return this.isPrisonerInt;
			}
		}

		public bool ScheduledForInteraction
		{
			get
			{
				return this.pawn.mindState.lastAssignedInteractTime < Find.TickManager.TicksGame - this.MinInteractionInterval;
			}
		}

		public bool PrisonerIsSecure
		{
			get
			{
				if (this.released)
				{
					return false;
				}
				if (this.pawn.HostFaction == null)
				{
					return false;
				}
				if (this.pawn.InMentalState)
				{
					return false;
				}
				if (this.pawn.Spawned)
				{
					if (this.pawn.jobs.curJob != null && this.pawn.jobs.curJob.exitMapOnArrival)
					{
						return false;
					}
					if (PrisonBreakUtility.IsPrisonBreaking(this.pawn))
					{
						return false;
					}
				}
				return true;
			}
		}

		public Pawn_GuestTracker()
		{
		}

		public Pawn_GuestTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void GuestTrackerTick()
		{
			if (this.pawn.IsHashIntervalTick(2500))
			{
				float num = PrisonBreakUtility.InitiatePrisonBreakMtbDays(this.pawn);
				if (num >= 0f && Rand.MTBEventOccurs(num, 60000f, 2500f))
				{
					PrisonBreakUtility.StartPrisonBreak(this.pawn);
				}
			}
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Faction>(ref this.hostFactionInt, "hostFaction", false);
			Scribe_Values.LookValue<bool>(ref this.isPrisonerInt, "prisoner", false, false);
			Scribe_Values.LookValue<bool>(ref this.getsFoodInt, "getsFood", false, false);
			Scribe_Values.LookValue<PrisonerInteractionMode>(ref this.interactionMode, "interactionMode", PrisonerInteractionMode.NoInteraction, false);
			Scribe_Values.LookValue<bool>(ref this.released, "released", false, false);
		}

		public void SetGuestStatus(Faction newHost, bool prisoner = false)
		{
			if (newHost != null)
			{
				this.released = false;
			}
			if (newHost == this.HostFaction && prisoner == this.IsPrisoner)
			{
				return;
			}
			if (!prisoner && this.pawn.Faction.HostileTo(newHost))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to make ",
					this.pawn,
					" a guest of ",
					newHost,
					" but their faction ",
					this.pawn.Faction,
					" is hostile to ",
					newHost
				}));
				return;
			}
			if (newHost == this.pawn.Faction && !prisoner)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to make ",
					this.pawn,
					" a guest of their own faction ",
					this.pawn.Faction
				}));
				return;
			}
			bool flag = prisoner && (!this.IsPrisoner || this.HostFaction != newHost);
			this.isPrisonerInt = prisoner;
			this.hostFactionInt = newHost;
			this.pawn.ClearMind(false);
			this.pawn.ClearReservations();
			if (flag)
			{
				this.pawn.DropAndForbidEverything(false);
				Lord lord = this.pawn.GetLord();
				if (lord != null)
				{
					lord.Notify_PawnLost(this.pawn, PawnLostCondition.MadePrisoner);
				}
				if (this.pawn.Drafted)
				{
					this.pawn.drafter.Drafted = false;
				}
			}
			PawnComponentsUtility.AddAndRemoveDynamicComponents(this.pawn, false);
			this.pawn.health.surgeryBills.Clear();
			if (this.pawn.ownership != null)
			{
				this.pawn.ownership.Notify_ChangedGuestStatus();
			}
			Reachability.ClearCache();
			Find.MapPawns.UpdateRegistryForPawn(this.pawn);
			Find.AttackTargetsCache.UpdateTarget(this.pawn);
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this.pawn);
		}

		internal void Notify_PawnUndowned()
		{
		}
	}
}
