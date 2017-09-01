using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class CaravanRequestComp : WorldObjectComp, IThingHolder
	{
		public ThingDef requestThingDef;

		public int requestCount;

		public ThingOwner rewards;

		public int expiration = -1;

		public bool ActiveRequest
		{
			get
			{
				return this.expiration > Find.TickManager.TicksGame;
			}
		}

		public CaravanRequestComp()
		{
			this.rewards = new ThingOwner<Thing>(this, true, LookMode.Deep);
		}

		public override string CompInspectStringExtra()
		{
			if (this.ActiveRequest)
			{
				return "CaravanRequestInfo".Translate(new object[]
				{
					GenLabel.ThingLabel(this.requestThingDef, null, this.requestCount).CapitalizeFirst(),
					this.rewards[0].LabelCap,
					(this.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F1")
				});
			}
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.rewards;
		}

		public void Disable()
		{
			this.expiration = -1;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look<ThingDef>(ref this.requestThingDef, "requestThingDef");
			Scribe_Values.Look<int>(ref this.requestCount, "requestCount", 0, false);
			Scribe_Deep.Look<ThingOwner>(ref this.rewards, "rewards", new object[]
			{
				this
			});
			Scribe_Values.Look<int>(ref this.expiration, "expiration", 0, false);
		}

		public override void PostPostRemove()
		{
			base.PostPostRemove();
			this.rewards.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		virtual IThingHolder get_ParentHolder()
		{
			return base.ParentHolder;
		}
	}
}
