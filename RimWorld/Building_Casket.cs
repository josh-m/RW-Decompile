using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_Casket : Building, IOpenable, IThingContainerOwner
	{
		protected ThingContainer innerContainer;

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
			this.innerContainer = new ThingContainer(this, false, LookMode.Deep);
		}

		public ThingContainer GetInnerContainer()
		{
			return this.innerContainer;
		}

		public IntVec3 GetPosition()
		{
			return base.PositionHeld;
		}

		public Map GetMap()
		{
			return base.MapHeld;
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
			Scribe_Deep.LookDeep<ThingContainer>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Values.LookValue<bool>(ref this.contentsKnown, "contentsKnown", false, false);
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				this.contentsKnown = true;
			}
		}

		public override bool ClaimableBy(Faction fac)
		{
			if (this.innerContainer.Count == 0)
			{
				return true;
			}
			for (int i = 0; i < this.innerContainer.Count; i++)
			{
				if (this.innerContainer[i].Faction == fac)
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool Accepts(Thing thing)
		{
			return this.innerContainer.CanAcceptAnyOf(thing);
		}

		public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!this.Accepts(thing))
			{
				return false;
			}
			bool flag;
			if (thing.holdingContainer != null)
			{
				thing.holdingContainer.TransferToContainer(thing, this.innerContainer, thing.stackCount);
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
			if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.Kill))
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing current in this.innerContainer)
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
			string inspectString = base.GetInspectString();
			string str;
			if (!this.contentsKnown)
			{
				str = "UnknownLower".Translate();
			}
			else
			{
				str = this.innerContainer.ContentsString;
			}
			return inspectString + "CasketContains".Translate() + ": " + str;
		}

		virtual bool get_Spawned()
		{
			return base.Spawned;
		}
	}
}
