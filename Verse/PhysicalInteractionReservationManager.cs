using System;
using System.Collections.Generic;

namespace Verse
{
	public class PhysicalInteractionReservationManager : IExposable
	{
		public class PhysicalInteractionReservation : IExposable
		{
			public LocalTargetInfo target;

			public Pawn claimant;

			public void ExposeData()
			{
				Scribe_TargetInfo.LookTargetInfo(ref this.target, "target");
				Scribe_References.LookReference<Pawn>(ref this.claimant, "claimant", false);
			}
		}

		private List<PhysicalInteractionReservationManager.PhysicalInteractionReservation> reservations = new List<PhysicalInteractionReservationManager.PhysicalInteractionReservation>();

		public void Reserve(Pawn claimant, LocalTargetInfo target)
		{
			if (this.IsReservedBy(claimant, target))
			{
				return;
			}
			PhysicalInteractionReservationManager.PhysicalInteractionReservation physicalInteractionReservation = new PhysicalInteractionReservationManager.PhysicalInteractionReservation();
			physicalInteractionReservation.target = target;
			physicalInteractionReservation.claimant = claimant;
			this.reservations.Add(physicalInteractionReservation);
		}

		public void Release(Pawn claimant, LocalTargetInfo target)
		{
			for (int i = 0; i < this.reservations.Count; i++)
			{
				PhysicalInteractionReservationManager.PhysicalInteractionReservation physicalInteractionReservation = this.reservations[i];
				if (physicalInteractionReservation.target == target && physicalInteractionReservation.claimant == claimant)
				{
					this.reservations.RemoveAt(i);
					return;
				}
			}
			Log.Warning(string.Concat(new object[]
			{
				claimant,
				" tried to release reservation on target ",
				target,
				", but it's not reserved by him."
			}));
		}

		public bool IsReservedBy(Pawn claimant, LocalTargetInfo target)
		{
			for (int i = 0; i < this.reservations.Count; i++)
			{
				PhysicalInteractionReservationManager.PhysicalInteractionReservation physicalInteractionReservation = this.reservations[i];
				if (physicalInteractionReservation.target == target && physicalInteractionReservation.claimant == claimant)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsReserved(LocalTargetInfo target)
		{
			return this.FirstReserverOf(target) != null;
		}

		public Pawn FirstReserverOf(LocalTargetInfo target)
		{
			for (int i = 0; i < this.reservations.Count; i++)
			{
				PhysicalInteractionReservationManager.PhysicalInteractionReservation physicalInteractionReservation = this.reservations[i];
				if (physicalInteractionReservation.target == target)
				{
					return physicalInteractionReservation.claimant;
				}
			}
			return null;
		}

		public void ReleaseAllForTarget(LocalTargetInfo target)
		{
			this.reservations.RemoveAll((PhysicalInteractionReservationManager.PhysicalInteractionReservation x) => x.target == target);
		}

		public void ReleaseAllClaimedBy(Pawn claimant)
		{
			this.reservations.RemoveAll((PhysicalInteractionReservationManager.PhysicalInteractionReservation x) => x.claimant == claimant);
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<PhysicalInteractionReservationManager.PhysicalInteractionReservation>(ref this.reservations, "reservations", LookMode.Deep, new object[0]);
		}
	}
}
