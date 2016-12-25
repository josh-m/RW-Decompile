using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_Casket : Building, IOpenable, IThingContainerOwner
	{
		protected ThingContainer container;

		protected bool contentsKnown;

		public bool HasAnyContents
		{
			get
			{
				return this.container.Count > 0;
			}
		}

		public Thing ContainedThing
		{
			get
			{
				return (this.container.Count != 0) ? this.container[0] : null;
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
			this.container = new ThingContainer(this, false);
		}

		public ThingContainer GetContainer()
		{
			return this.container;
		}

		public IntVec3 GetPosition()
		{
			return base.PositionHeld;
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
			Scribe_Deep.LookDeep<ThingContainer>(ref this.container, "container", new object[]
			{
				this
			});
			Scribe_Values.LookValue<bool>(ref this.contentsKnown, "contentsKnown", false, false);
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				this.contentsKnown = true;
			}
		}

		public override bool ClaimableBy(Faction fac)
		{
			if (this.container.Count == 0)
			{
				return true;
			}
			for (int i = 0; i < this.container.Count; i++)
			{
				if (this.container[i].Faction == fac)
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool Accepts(Thing thing)
		{
			return this.container.CanAcceptAnyOf(thing);
		}

		public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!this.Accepts(thing))
			{
				return false;
			}
			bool flag;
			if (thing.holder != null)
			{
				thing.holder.TransferToContainer(thing, this.container, thing.stackCount);
				flag = true;
			}
			else
			{
				flag = this.container.TryAdd(thing);
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
			if (this.container.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.Kill))
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing current in this.container)
					{
						Pawn pawn = current as Pawn;
						if (pawn != null)
						{
							list.Add(pawn);
						}
					}
					foreach (Pawn current2 in list)
					{
						HealthUtility.GiveInjuriesToForceDowned(current2);
					}
				}
				this.EjectContents();
			}
			this.container.ClearAndDestroyContents(DestroyMode.Vanish);
			base.Destroy(mode);
		}

		public virtual void EjectContents()
		{
			this.container.TryDropAll(this.InteractionCell, ThingPlaceMode.Near);
			this.contentsKnown = true;
		}

		public override string GetInspectString()
		{
			string inspectString = base.GetInspectString();
			string text;
			if (!this.contentsKnown)
			{
				text = "UnknownLower".Translate();
			}
			else
			{
				text = this.container.ContentsString;
			}
			string text2 = inspectString;
			return string.Concat(new string[]
			{
				text2,
				"\n",
				"CasketContains".Translate(),
				": ",
				text
			});
		}

		virtual bool get_Spawned()
		{
			return base.Spawned;
		}
	}
}
