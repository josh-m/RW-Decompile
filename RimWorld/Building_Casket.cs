using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_Casket : Building, IThingHolder, IOpenable
	{
		protected ThingOwner innerContainer;

		protected bool contentsKnown;

		public bool HasAnyContents
		{
			get
			{
				return this.innerContainer.Count > 0;
			}
		}

		public Thing ContainedThing
		{
			get
			{
				return (this.innerContainer.Count != 0) ? this.innerContainer[0] : null;
			}
		}

		public bool CanOpen
		{
			get
			{
				return this.HasAnyContents;
			}
		}

		public Building_Casket()
		{
			this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		public override void TickRare()
		{
			base.TickRare();
			this.innerContainer.ThingOwnerTickRare(true);
		}

		public override void Tick()
		{
			base.Tick();
			this.innerContainer.ThingOwnerTick(true);
		}

		public virtual void Open()
		{
			if (!this.HasAnyContents)
			{
				return;
			}
			this.EjectContents();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Values.Look<bool>(ref this.contentsKnown, "contentsKnown", false, false);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				this.contentsKnown = true;
			}
		}

		public override bool ClaimableBy(Faction fac)
		{
			if (this.innerContainer.Any)
			{
				for (int i = 0; i < this.innerContainer.Count; i++)
				{
					if (this.innerContainer[i].Faction == fac)
					{
						return true;
					}
				}
				return false;
			}
			return base.ClaimableBy(fac);
		}

		public virtual bool Accepts(Thing thing)
		{
			return this.innerContainer.CanAcceptAnyOf(thing, true);
		}

		public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!this.Accepts(thing))
			{
				return false;
			}
			bool flag;
			if (thing.holdingOwner != null)
			{
				thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
				flag = true;
			}
			else
			{
				flag = this.innerContainer.TryAdd(thing, true);
			}
			if (flag)
			{
				if (thing.Faction != null && thing.Faction.IsPlayer)
				{
					this.contentsKnown = true;
				}
				return true;
			}
			return false;
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
					{
						Pawn pawn = current as Pawn;
						if (pawn != null)
						{
							list.Add(pawn);
						}
					}
					foreach (Pawn current2 in list)
					{
						HealthUtility.DamageUntilDowned(current2);
					}
				}
				this.EjectContents();
			}
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			base.Destroy(mode);
		}

		public virtual void EjectContents()
		{
			this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near);
			this.contentsKnown = true;
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			string str;
			if (!this.contentsKnown)
			{
				str = "UnknownLower".Translate();
			}
			else
			{
				str = this.innerContainer.ContentsString;
			}
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + "CasketContains".Translate() + ": " + str;
		}
	}
}
