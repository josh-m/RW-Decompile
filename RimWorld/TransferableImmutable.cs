using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TransferableImmutable : Transferable
	{
		public List<Thing> things = new List<Thing>();

		public override Thing AnyThing
		{
			get
			{
				return (!this.HasAnyThing) ? null : this.things[0];
			}
		}

		public override ThingDef ThingDef
		{
			get
			{
				return (!this.HasAnyThing) ? null : this.AnyThing.def;
			}
		}

		public override bool HasAnyThing
		{
			get
			{
				return this.things.Count != 0;
			}
		}

		public override string Label
		{
			get
			{
				return this.AnyThing.LabelNoCount;
			}
		}

		public override bool Interactive
		{
			get
			{
				return false;
			}
		}

		public override TransferablePositiveCountDirection PositiveCountDirection
		{
			get
			{
				return TransferablePositiveCountDirection.Destination;
			}
		}

		public override string TipDescription
		{
			get
			{
				return (!this.HasAnyThing) ? string.Empty : this.AnyThing.DescriptionDetailed;
			}
		}

		public override int CountToTransfer
		{
			get
			{
				return 0;
			}
			protected set
			{
				if (value != 0)
				{
					throw new InvalidOperationException("immutable transferable");
				}
			}
		}

		public string LabelWithTotalStackCount
		{
			get
			{
				string text = this.Label;
				int totalStackCount = this.TotalStackCount;
				if (totalStackCount != 1)
				{
					text = text + " x" + totalStackCount.ToStringCached();
				}
				return text;
			}
		}

		public string LabelCapWithTotalStackCount
		{
			get
			{
				return this.LabelWithTotalStackCount.CapitalizeFirst();
			}
		}

		public int TotalStackCount
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

		public override int GetMinimumToTransfer()
		{
			return 0;
		}

		public override int GetMaximumToTransfer()
		{
			return 0;
		}

		public override AcceptanceReport OverflowReport()
		{
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Collections.Look<Thing>(ref this.things, "things", LookMode.Reference, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.things.RemoveAll((Thing x) => x == null) != 0)
				{
					Log.Warning("Some of the things were null after loading.", false);
				}
			}
		}
	}
}
