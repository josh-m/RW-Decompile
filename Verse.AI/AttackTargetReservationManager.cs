using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class AttackTargetReservationManager : IExposable
	{
		public class AttackTargetReservation : IExposable
		{
			public IAttackTarget target;

			public Pawn claimant;

			public Job job;

			public void ExposeData()
			{
				Scribe_References.Look<IAttackTarget>(ref this.target, "target", false);
				Scribe_References.Look<Pawn>(ref this.claimant, "claimant", false);
				Scribe_References.Look<Job>(ref this.job, "job", false);
			}
		}

		private Map map;

		private List<AttackTargetReservationManager.AttackTargetReservation> reservations = new List<AttackTargetReservationManager.AttackTargetReservation>();

		public AttackTargetReservationManager(Map map)
		{
			this.map = map;
		}

		public void Reserve(Pawn claimant, Job job, IAttackTarget target)
		{
			if (target == null)
			{
				Log.Warning(claimant + " tried to reserve null attack target.");
				return;
			}
			if (this.IsReservedBy(claimant, target))
			{
				return;
			}
			AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = new AttackTargetReservationManager.AttackTargetReservation();
			attackTargetReservation.target = target;
			attackTargetReservation.claimant = claimant;
			attackTargetReservation.job = job;
			this.reservations.Add(attackTargetReservation);
		}

		public void Release(Pawn claimant, Job job, IAttackTarget target)
		{
			if (target == null)
			{
				Log.Warning(claimant + " tried to release reservation on null attack target.");
				return;
			}
			for (int i = 0; i < this.reservations.Count; i++)
			{
				AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = this.reservations[i];
				if (attackTargetReservation.target == target && attackTargetReservation.claimant == claimant && attackTargetReservation.job == job)
				{
					this.reservations.RemoveAt(i);
					return;
				}
			}
			Log.Warning(string.Concat(new object[]
			{
				claimant,
				" with job ",
				job,
				" tried to release reservation on target ",
				target,
				", but it's not reserved by him."
			}));
		}

		public bool CanReserve(Pawn claimant, IAttackTarget target)
		{
			if (this.IsReservedBy(claimant, target))
			{
				return true;
			}
			int reservationsCount = this.GetReservationsCount(target, claimant.Faction);
			int maxPreferredReservationsCount = this.GetMaxPreferredReservationsCount(target);
			return reservationsCount < maxPreferredReservationsCount;
		}

		public bool IsReservedBy(Pawn claimant, IAttackTarget target)
		{
			for (int i = 0; i < this.reservations.Count; i++)
			{
				AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = this.reservations[i];
				if (attackTargetReservation.target == target && attackTargetReservation.claimant == claimant)
				{
					return true;
				}
			}
			return false;
		}

		public void ReleaseAllForTarget(IAttackTarget target)
		{
			this.reservations.RemoveAll((AttackTargetReservationManager.AttackTargetReservation x) => x.target == target);
		}

		public void ReleaseClaimedBy(Pawn claimant, Job job)
		{
			for (int i = this.reservations.Count - 1; i >= 0; i--)
			{
				if (this.reservations[i].claimant == claimant && this.reservations[i].job == job)
				{
					this.reservations.RemoveAt(i);
				}
			}
		}

		public void ReleaseAllClaimedBy(Pawn claimant)
		{
			for (int i = this.reservations.Count - 1; i >= 0; i--)
			{
				if (this.reservations[i].claimant == claimant)
				{
					this.reservations.RemoveAt(i);
				}
			}
		}

		public IAttackTarget FirstReservationFor(Pawn claimant)
		{
			for (int i = this.reservations.Count - 1; i >= 0; i--)
			{
				if (this.reservations[i].claimant == claimant)
				{
					return this.reservations[i].target;
				}
			}
			return null;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<AttackTargetReservationManager.AttackTargetReservation>(ref this.reservations, "reservations", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.reservations.RemoveAll((AttackTargetReservationManager.AttackTargetReservation x) => x.target == null);
				if (this.reservations.RemoveAll((AttackTargetReservationManager.AttackTargetReservation x) => x.claimant.DestroyedOrNull()) != 0)
				{
					Log.Warning("Some attack target reservations had null or destroyed claimant.");
				}
			}
		}

		private int GetReservationsCount(IAttackTarget target, Faction faction)
		{
			int num = 0;
			for (int i = 0; i < this.reservations.Count; i++)
			{
				AttackTargetReservationManager.AttackTargetReservation attackTargetReservation = this.reservations[i];
				if (attackTargetReservation.target == target && attackTargetReservation.claimant.Faction == faction)
				{
					num++;
				}
			}
			return num;
		}

		private int GetMaxPreferredReservationsCount(IAttackTarget target)
		{
			int num = 0;
			Thing thing = target.Thing;
			CellRect cellRect = thing.OccupiedRect();
			foreach (IntVec3 current in cellRect.ExpandedBy(1))
			{
				if (!cellRect.Contains(current))
				{
					if (current.InBounds(this.map))
					{
						if (current.Standable(this.map))
						{
							num++;
						}
					}
				}
			}
			return num;
		}
	}
}
