using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Pawn_Ownership : IExposable
	{
		private Pawn pawn;

		public Building_Bed OwnedBed
		{
			get;
			private set;
		}

		public Building_Grave AssignedGrave
		{
			get;
			private set;
		}

		public Room OwnedRoom
		{
			get
			{
				if (this.OwnedBed == null)
				{
					return null;
				}
				Room room = RoomQuery.RoomAt(this.OwnedBed);
				if (room == null)
				{
					return null;
				}
				if (room.Owners.Contains(this.pawn))
				{
					return room;
				}
				return null;
			}
		}

		public Pawn_Ownership(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Building_Bed ownedBed = this.OwnedBed;
			Building_Grave assignedGrave = this.AssignedGrave;
			Scribe_References.LookReference<Building_Bed>(ref ownedBed, "ownedBed", false);
			Scribe_References.LookReference<Building_Grave>(ref assignedGrave, "assignedGrave", false);
			this.OwnedBed = ownedBed;
			this.AssignedGrave = assignedGrave;
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.AssignedGrave != null)
				{
					this.AssignedGrave.assignedPawn = this.pawn;
				}
				if (this.OwnedBed != null)
				{
					this.OwnedBed.owners.Add(this.pawn);
					this.OwnedBed.SortOwners();
				}
			}
		}

		public void ClaimBedIfNonMedical(Building_Bed newBed)
		{
			if (newBed.owners.Contains(this.pawn) || newBed.Medical)
			{
				return;
			}
			this.UnclaimBed();
			if (newBed.owners.Count == newBed.SleepingSlotsCount)
			{
				newBed.owners[newBed.owners.Count - 1].ownership.UnclaimBed();
			}
			newBed.owners.Add(this.pawn);
			newBed.SortOwners();
			this.OwnedBed = newBed;
			if (newBed.Medical)
			{
				Log.Warning(this.pawn.LabelCap + " claimed medical bed.");
				this.UnclaimBed();
			}
		}

		public void UnclaimBed()
		{
			if (this.OwnedBed != null)
			{
				this.OwnedBed.owners.Remove(this.pawn);
				this.OwnedBed = null;
			}
		}

		public void ClaimGrave(Building_Grave newGrave)
		{
			if (newGrave.assignedPawn == this.pawn)
			{
				return;
			}
			this.UnclaimGrave();
			if (newGrave.assignedPawn != null)
			{
				newGrave.assignedPawn.ownership.UnclaimBed();
			}
			newGrave.assignedPawn = this.pawn;
			this.AssignedGrave = newGrave;
		}

		public void UnclaimGrave()
		{
			if (this.AssignedGrave != null)
			{
				this.AssignedGrave.assignedPawn = null;
				this.AssignedGrave = null;
			}
		}

		public void UnclaimAll()
		{
			this.UnclaimBed();
			this.UnclaimGrave();
		}

		public void Notify_ChangedGuestStatus()
		{
			if (this.OwnedBed != null && ((this.OwnedBed.ForPrisoners && !this.pawn.IsPrisoner) || (!this.OwnedBed.ForPrisoners && this.pawn.IsPrisoner)))
			{
				this.UnclaimBed();
			}
		}
	}
}
