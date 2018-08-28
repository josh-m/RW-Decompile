using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Transferable : IExposable
	{
		private string editBuffer = "0";

		public abstract Thing AnyThing
		{
			get;
		}

		public abstract ThingDef ThingDef
		{
			get;
		}

		public abstract bool Interactive
		{
			get;
		}

		public abstract bool HasAnyThing
		{
			get;
		}

		public abstract string Label
		{
			get;
		}

		public string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public abstract string TipDescription
		{
			get;
		}

		public abstract TransferablePositiveCountDirection PositiveCountDirection
		{
			get;
		}

		public abstract int CountToTransfer
		{
			get;
			protected set;
		}

		public int CountToTransferToSource
		{
			get
			{
				return (this.PositiveCountDirection != TransferablePositiveCountDirection.Source) ? (-this.CountToTransfer) : this.CountToTransfer;
			}
		}

		public int CountToTransferToDestination
		{
			get
			{
				return (this.PositiveCountDirection != TransferablePositiveCountDirection.Source) ? this.CountToTransfer : (-this.CountToTransfer);
			}
		}

		public string EditBuffer
		{
			get
			{
				return this.editBuffer;
			}
			set
			{
				this.editBuffer = value;
			}
		}

		public abstract int GetMinimumToTransfer();

		public abstract int GetMaximumToTransfer();

		public int GetRange()
		{
			return this.GetMaximumToTransfer() - this.GetMinimumToTransfer();
		}

		public int ClampAmount(int amount)
		{
			return Mathf.Clamp(amount, this.GetMinimumToTransfer(), this.GetMaximumToTransfer());
		}

		public AcceptanceReport CanAdjustBy(int adjustment)
		{
			return this.CanAdjustTo(this.CountToTransfer + adjustment);
		}

		public AcceptanceReport CanAdjustTo(int destination)
		{
			if (destination == this.CountToTransfer)
			{
				return AcceptanceReport.WasAccepted;
			}
			int num = this.ClampAmount(destination);
			if (num != this.CountToTransfer)
			{
				return AcceptanceReport.WasAccepted;
			}
			if (destination < this.CountToTransfer)
			{
				return this.UnderflowReport();
			}
			return this.OverflowReport();
		}

		public void AdjustBy(int adjustment)
		{
			this.AdjustTo(this.CountToTransfer + adjustment);
		}

		public void AdjustTo(int destination)
		{
			if (!this.CanAdjustTo(destination).Accepted)
			{
				Log.Error("Failed to adjust transferable counts", false);
				return;
			}
			this.CountToTransfer = this.ClampAmount(destination);
		}

		public void ForceTo(int value)
		{
			this.CountToTransfer = value;
		}

		public void ForceToSource(int value)
		{
			if (this.PositiveCountDirection == TransferablePositiveCountDirection.Source)
			{
				this.ForceTo(value);
			}
			else
			{
				this.ForceTo(-value);
			}
		}

		public void ForceToDestination(int value)
		{
			if (this.PositiveCountDirection == TransferablePositiveCountDirection.Source)
			{
				this.ForceTo(-value);
			}
			else
			{
				this.ForceTo(value);
			}
		}

		public virtual AcceptanceReport UnderflowReport()
		{
			return false;
		}

		public virtual AcceptanceReport OverflowReport()
		{
			return false;
		}

		public virtual void ExposeData()
		{
		}
	}
}
