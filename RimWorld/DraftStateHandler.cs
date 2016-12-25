using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class DraftStateHandler : IExposable
	{
		public Pawn pawn;

		private bool draftedInt;

		private AutoUndrafter autoUndrafter;

		public bool Drafted
		{
			get
			{
				return this.draftedInt;
			}
			set
			{
				if (value == this.draftedInt)
				{
					return;
				}
				this.pawn.mindState.priorityWork.Clear();
				this.draftedInt = value;
				if (!value)
				{
					Find.PawnDestinationManager.UnreserveAllFor(this.pawn);
				}
				if (this.pawn.jobs.curJob != null && this.pawn.drafter.CanTakeOrderedJob())
				{
					this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				if (this.draftedInt)
				{
					foreach (Pawn current in PawnUtility.SpawnedMasteredPawns(this.pawn))
					{
						current.jobs.Notify_MasterDrafted();
					}
					Lord lord = this.pawn.GetLord();
					if (lord != null)
					{
						lord.Notify_PawnLost(this.pawn, PawnLostCondition.Drafted);
					}
				}
				else if (this.pawn.playerSettings != null)
				{
					this.pawn.playerSettings.animalsReleased = false;
				}
			}
		}

		public DraftStateHandler(Pawn pawn)
		{
			this.pawn = pawn;
			this.autoUndrafter = new AutoUndrafter(pawn);
		}

		public void DrafterTick()
		{
			this.autoUndrafter.AutoUndraftTick();
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.draftedInt, "drafted", false, false);
			Scribe_Deep.LookDeep<AutoUndrafter>(ref this.autoUndrafter, "autoUndrafter", new object[]
			{
				this.pawn
			});
		}
	}
}
