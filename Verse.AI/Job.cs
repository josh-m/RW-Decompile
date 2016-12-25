using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class Job : IExposable
	{
		public JobDef def;

		public LocalTargetInfo targetA = LocalTargetInfo.Invalid;

		public LocalTargetInfo targetB = LocalTargetInfo.Invalid;

		public LocalTargetInfo targetC = LocalTargetInfo.Invalid;

		public List<LocalTargetInfo> targetQueueA;

		public List<LocalTargetInfo> targetQueueB;

		public int count = -1;

		public List<int> countQueue;

		public int startTick = -1;

		public int expiryInterval = -1;

		public bool checkOverrideOnExpire;

		public bool playerForced;

		public List<ThingStackPartClass> placedThings;

		public int maxNumMeleeAttacks = 2147483647;

		public LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;

		public HaulMode haulMode;

		public Bill bill;

		public ICommunicable commTarget;

		public ThingDef plantDefToSow;

		public Verb verbToUse;

		public bool haulOpportunisticDuplicates;

		public bool exitMapOnArrival;

		public bool failIfCantJoinOrCreateCaravan;

		public bool killIncappedTarget;

		public bool ignoreForbidden;

		public bool ignoreDesignations;

		public bool canBash;

		public bool haulDroppedApparel;

		public bool restUntilHealed;

		public bool ignoreJoyTimeAssignment;

		public bool overeat;

		public bool attackDoorIfTargetLost;

		public int takeExtraIngestibles;

		public bool expireRequiresEnemiesNearby;

		public RecipeDef RecipeDef
		{
			get
			{
				return this.bill.recipe;
			}
		}

		public Job()
		{
		}

		public Job(JobDef def) : this(def, null)
		{
		}

		public Job(JobDef def, LocalTargetInfo targetA) : this(def, targetA, null)
		{
		}

		public Job(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB)
		{
			this.def = def;
			this.targetA = targetA;
			this.targetB = targetB;
		}

		public Job(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
		{
			this.def = def;
			this.targetA = targetA;
			this.targetB = targetB;
			this.targetC = targetC;
		}

		public Job(JobDef def, LocalTargetInfo targetA, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			this.def = def;
			this.targetA = targetA;
			this.expiryInterval = expiryInterval;
			this.checkOverrideOnExpire = checkOverrideOnExpiry;
		}

		public Job(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			this.def = def;
			this.expiryInterval = expiryInterval;
			this.checkOverrideOnExpire = checkOverrideOnExpiry;
		}

		public LocalTargetInfo GetTarget(TargetIndex ind)
		{
			switch (ind)
			{
			case TargetIndex.A:
				return this.targetA;
			case TargetIndex.B:
				return this.targetB;
			case TargetIndex.C:
				return this.targetC;
			default:
				throw new ArgumentException();
			}
		}

		public List<LocalTargetInfo> GetTargetQueue(TargetIndex ind)
		{
			if (ind == TargetIndex.A)
			{
				if (this.targetQueueA == null)
				{
					this.targetQueueA = new List<LocalTargetInfo>();
				}
				return this.targetQueueA;
			}
			if (ind != TargetIndex.B)
			{
				throw new ArgumentException();
			}
			if (this.targetQueueB == null)
			{
				this.targetQueueB = new List<LocalTargetInfo>();
			}
			return this.targetQueueB;
		}

		public void SetTarget(TargetIndex ind, LocalTargetInfo pack)
		{
			switch (ind)
			{
			case TargetIndex.A:
				this.targetA = pack;
				return;
			case TargetIndex.B:
				this.targetB = pack;
				return;
			case TargetIndex.C:
				this.targetC = pack;
				return;
			default:
				throw new ArgumentException();
			}
		}

		public void AddQueuedTarget(TargetIndex ind, LocalTargetInfo target)
		{
			this.GetTargetQueue(ind).Add(target);
		}

		public void ExposeData()
		{
			ILoadReferenceable loadReferenceable = (ILoadReferenceable)this.commTarget;
			Scribe_References.LookReference<ILoadReferenceable>(ref loadReferenceable, "commTarget", false);
			this.commTarget = (ICommunicable)loadReferenceable;
			Scribe_References.LookReference<Verb>(ref this.verbToUse, "verbToUse", false);
			Scribe_References.LookReference<Bill>(ref this.bill, "bill", false);
			Scribe_Defs.LookDef<JobDef>(ref this.def, "def");
			Scribe_TargetInfo.LookTargetInfo(ref this.targetA, "targetA");
			Scribe_TargetInfo.LookTargetInfo(ref this.targetB, "targetB");
			Scribe_TargetInfo.LookTargetInfo(ref this.targetC, "targetC");
			Scribe_Collections.LookList<LocalTargetInfo>(ref this.targetQueueA, "targetQueueA", LookMode.Undefined, new object[0]);
			Scribe_Collections.LookList<LocalTargetInfo>(ref this.targetQueueB, "targetQueueB", LookMode.Undefined, new object[0]);
			Scribe_Values.LookValue<int>(ref this.count, "count", -1, false);
			Scribe_Collections.LookList<int>(ref this.countQueue, "countQueue", LookMode.Undefined, new object[0]);
			Scribe_Values.LookValue<int>(ref this.startTick, "startTick", -1, false);
			Scribe_Values.LookValue<int>(ref this.expiryInterval, "expiryInterval", -1, false);
			Scribe_Values.LookValue<bool>(ref this.checkOverrideOnExpire, "checkOverrideOnExpire", false, false);
			Scribe_Values.LookValue<bool>(ref this.playerForced, "playerForced", false, false);
			Scribe_Collections.LookList<ThingStackPartClass>(ref this.placedThings, "placedThings", LookMode.Undefined, new object[0]);
			Scribe_Values.LookValue<int>(ref this.maxNumMeleeAttacks, "maxNumMeleeAttacks", 2147483647, false);
			Scribe_Values.LookValue<bool>(ref this.exitMapOnArrival, "exitMapOnArrival", false, false);
			Scribe_Values.LookValue<bool>(ref this.failIfCantJoinOrCreateCaravan, "failIfCantJoinOrCreateCaravan", false, false);
			Scribe_Values.LookValue<bool>(ref this.killIncappedTarget, "killIncappedTarget", false, false);
			Scribe_Values.LookValue<bool>(ref this.haulOpportunisticDuplicates, "haulOpportunisticDuplicates", false, false);
			Scribe_Values.LookValue<HaulMode>(ref this.haulMode, "haulMode", HaulMode.Undefined, false);
			Scribe_Defs.LookDef<ThingDef>(ref this.plantDefToSow, "plantDefToSow");
			Scribe_Values.LookValue<LocomotionUrgency>(ref this.locomotionUrgency, "locomotionUrgency", LocomotionUrgency.Jog, false);
			Scribe_Values.LookValue<bool>(ref this.ignoreDesignations, "ignoreDesignations", false, false);
			Scribe_Values.LookValue<bool>(ref this.canBash, "canBash", false, false);
			Scribe_Values.LookValue<bool>(ref this.haulDroppedApparel, "haulDroppedApparel", false, false);
			Scribe_Values.LookValue<bool>(ref this.restUntilHealed, "restUntilHealed", false, false);
			Scribe_Values.LookValue<bool>(ref this.ignoreJoyTimeAssignment, "ignoreJoyTimeAssignment", false, false);
			Scribe_Values.LookValue<bool>(ref this.overeat, "overeat", false, false);
			Scribe_Values.LookValue<bool>(ref this.attackDoorIfTargetLost, "attackDoorIfTargetLost", false, false);
			Scribe_Values.LookValue<int>(ref this.takeExtraIngestibles, "takeExtraIngestibles", 0, false);
			Scribe_Values.LookValue<bool>(ref this.expireRequiresEnemiesNearby, "expireRequiresEnemiesNearby", false, false);
		}

		public JobDriver MakeDriver(Pawn driverPawn)
		{
			JobDriver jobDriver = (JobDriver)Activator.CreateInstance(this.def.driverClass);
			jobDriver.pawn = driverPawn;
			return jobDriver;
		}

		public bool CanBeginNow(Pawn pawn)
		{
			if (!pawn.Downed)
			{
				return true;
			}
			if (this.def != JobDefOf.LayDown)
			{
				return false;
			}
			if (this.targetA.HasThing)
			{
				return RestUtility.GetBedSleepingSlotPosFor(pawn, (Building_Bed)this.targetA.Thing) == pawn.Position;
			}
			return this.targetA.Cell == pawn.Position;
		}

		public bool JobIsSameAs(Job other)
		{
			return other != null && this.def == other.def && !(this.targetA != other.targetA) && !(this.targetB != other.targetB) && this.verbToUse == other.verbToUse && !(this.targetC != other.targetC) && this.commTarget == other.commTarget && this.bill == other.bill;
		}

		public override string ToString()
		{
			string text = this.def.ToString();
			if (this.targetA.IsValid)
			{
				text = text + " A=" + this.targetA.ToString();
			}
			if (this.targetB.IsValid)
			{
				text = text + " B=" + this.targetB.ToString();
			}
			if (this.targetC.IsValid)
			{
				text = text + " C=" + this.targetC.ToString();
			}
			return text;
		}
	}
}
