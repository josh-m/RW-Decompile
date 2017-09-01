using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Transferable
	{
		private int countToTransfer;

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

		public abstract string TipDescription
		{
			get;
		}

		public abstract TransferablePositiveCountDirection PositiveCountDirection
		{
			get;
		}

		public int CountToTransfer
		{
			get
			{
				return this.countToTransfer;
			}
			protected set
			{
				this.countToTransfer = value;
				this.editBuffer = value.ToStringCached();
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

		public abstract int GetMinimum();

		public abstract int GetMaximum();

		public int GetRange()
		{
			return this.GetMaximum() - this.GetMinimum();
		}

		public int ClampAmount(int amount)
		{
			return Mathf.Clamp(amount, this.GetMinimum(), this.GetMaximum());
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
				Log.Error("Failed to adjust transferable counts");
				return;
			}
			this.CountToTransfer = this.ClampAmount(destination);
		}

		public void ForceTo(int destination)
		{
			this.CountToTransfer = destination;
		}

		public virtual AcceptanceReport UnderflowReport()
		{
			return false;
		}

		public virtual AcceptanceReport OverflowReport()
		{
			return false;
		}
	}
}
