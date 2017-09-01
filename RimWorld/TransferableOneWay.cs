using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TransferableOneWay : Transferable, IExposable
	{
		public List<Thing> things = new List<Thing>();

		public override Thing AnyThing
		{
			get
			{
				return (this.things.Count <= 0) ? null : this.things[0];
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
				return this.AnyThing.LabelCapNoCount;
			}
		}

		public override string TipDescription
		{
			get
			{
				return this.ThingDef.description;
			}
		}

		public override bool Interactive
		{
			get
			{
				return true;
			}
		}

		public override TransferablePositiveCountDirection PositiveCountDirection
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

		public override int GetMinimum()
		{
			return 0;
		}

		public override int GetMaximum()
		{
			return this.MaxCount;
		}

		public override AcceptanceReport OverflowReport()
		{
			return new AcceptanceReport("ColonyHasNoMore".Translate());
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Collections.Look<Thing>(ref this.things, "things", LookMode.Reference, new object[0]);
			int countToTransfer = base.CountToTransfer;
			Scribe_Values.Look<int>(ref countToTransfer, "countToTransfer", 0, false);
			base.CountToTransfer = countToTransfer;
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
