using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TransferableOneWay : ITransferable, IExposable
	{
		public List<Thing> things = new List<Thing>();

		public int countToTransfer;

		public string editBuffer;

		public Thing AnyThing
		{
			get
			{
				return (this.things.Count <= 0) ? null : this.things[0];
			}
		}

		public ThingDef ThingDef
		{
			get
			{
				return this.AnyThing.def;
			}
		}

		public bool HasAnyThing
		{
			get
			{
				return this.things.Count != 0;
			}
		}

		public string Label
		{
			get
			{
				return this.AnyThing.LabelCapNoCount;
			}
		}

		public string TipDescription
		{
			get
			{
				return this.ThingDef.description;
			}
		}

		public bool Interactive
		{
			get
			{
				return true;
			}
		}

		public int CountToTransfer
		{
			get
			{
				return this.countToTransfer;
			}
			set
			{
				this.countToTransfer = value;
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

		public TransferablePositiveCountDirection PositiveCountDirection
		{
			get
			{
				return TransferablePositiveCountDirection.Destination;
			}
		}

		public int MaxCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.things.Count; i++)
				{
					num += this.things[i].stackCount;
				}
				return num;
			}
		}

		public AcceptanceReport CanSetToTransferOneMoreToSource()
		{
			return this.countToTransfer > 0;
		}

		public AcceptanceReport TrySetToTransferOneMoreToSource()
		{
			if (!this.CanSetToTransferOneMoreToSource().Accepted)
			{
				return false;
			}
			this.countToTransfer--;
			return true;
		}

		public void SetToTransferMaxToSource()
		{
			this.countToTransfer = 0;
		}

		public AcceptanceReport CanSetToTransferOneMoreToDest()
		{
			return this.countToTransfer < this.MaxCount;
		}

		public AcceptanceReport TrySetToTransferOneMoreToDest()
		{
			if (!this.CanSetToTransferOneMoreToDest().Accepted)
			{
				return false;
			}
			this.countToTransfer++;
			return true;
		}

		public void SetToTransferMaxToDest()
		{
			this.countToTransfer = this.MaxCount;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Collections.LookList<Thing>(ref this.things, "things", LookMode.Reference, new object[0]);
			Scribe_Values.LookValue<int>(ref this.countToTransfer, "countToTransfer", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.things.RemoveAll((Thing x) => x == null) != 0)
				{
					Log.Warning("Some of the things were null after loading.");
				}
			}
		}
	}
}
