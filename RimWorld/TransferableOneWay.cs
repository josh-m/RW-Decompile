using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TransferableOneWay : Transferable
	{
		public List<Thing> things = new List<Thing>();

		private int countToTransfer;

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
				return this.countToTransfer;
			}
			protected set
			{
				this.countToTransfer = value;
				base.EditBuffer = value.ToStringCached();
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

		public override int GetMinimumToTransfer()
		{
			return 0;
		}

		public override int GetMaximumToTransfer()
		{
			return this.MaxCount;
		}

		public override AcceptanceReport OverflowReport()
		{
			return new AcceptanceReport("ColonyHasNoMore".Translate());
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.things.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Values.Look<int>(ref this.countToTransfer, "countToTransfer", 0, false);
			Scribe_Collections.Look<Thing>(ref this.things, "things", LookMode.Reference, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				base.EditBuffer = this.countToTransfer.ToStringCached();
			}
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
