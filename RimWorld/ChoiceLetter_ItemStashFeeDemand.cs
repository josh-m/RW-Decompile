using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_ItemStashFeeDemand : ChoiceLetter, IThingHolder
	{
		public Map map;

		public int fee;

		public int siteDaysTimeout;

		public ThingOwner items;

		public Faction siteFaction;

		public SitePartDef sitePart;

		public Faction alliedFaction;

		public bool sitePartsKnown;

		protected override IEnumerable<DiaOption> Choices
		{
			get
			{
				DiaOption accept = new DiaOption("ItemStashQuest_Accept".Translate());
				accept.action = delegate
				{
					int tile;
					if (!TileFinder.TryFindNewSiteTile(out tile, 8, 30, false, true, -1))
					{
						Find.LetterStack.RemoveLetter(this.$this);
						return;
					}
					Site o = IncidentWorker_QuestItemStash.CreateSite(tile, this.$this.sitePart, this.$this.siteDaysTimeout, this.$this.siteFaction, this.$this.items, this.$this.sitePartsKnown);
					CameraJumper.TryJumpAndSelect(o);
					TradeUtility.LaunchSilver(this.$this.map, this.$this.fee);
					Find.LetterStack.RemoveLetter(this.$this);
				};
				accept.resolveTree = true;
				if (this.map == null || !TradeUtility.ColonyHasEnoughSilver(this.map, this.fee))
				{
					accept.Disable("NeedSilverLaunchable".Translate(new object[]
					{
						this.fee
					}));
				}
				yield return accept;
				yield return base.Reject;
				yield return base.Postpone;
			}
		}

		public override bool StillValid
		{
			get
			{
				return base.StillValid && !this.alliedFaction.HostileTo(Faction.OfPlayer) && (this.map == null || Find.Maps.Contains(this.map));
			}
		}

		public ChoiceLetter_ItemStashFeeDemand()
		{
			this.items = new ThingOwner<Thing>(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Map>(ref this.map, "map", false);
			Scribe_Values.Look<int>(ref this.fee, "fee", 0, false);
			Scribe_Values.Look<int>(ref this.siteDaysTimeout, "siteDaysTimeout", 0, false);
			Scribe_Deep.Look<ThingOwner>(ref this.items, "items", new object[]
			{
				this
			});
			Scribe_References.Look<Faction>(ref this.siteFaction, "siteFaction", false);
			Scribe_Defs.Look<SitePartDef>(ref this.sitePart, "sitePart");
			Scribe_References.Look<Faction>(ref this.alliedFaction, "alliedFaction", false);
			Scribe_Values.Look<bool>(ref this.sitePartsKnown, "sitePartsKnown", false, false);
		}

		public override void Removed()
		{
			base.Removed();
			this.items.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.items;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
