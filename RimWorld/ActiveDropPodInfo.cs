using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ActiveDropPodInfo : IExposable, IThingHolder
	{
		public const int DefaultOpenDelay = 110;

		public IThingHolder parent;

		public ThingOwner innerContainer;

		public int openDelay = 110;

		public bool leaveSlag;

		public bool savePawnsWithReferenceMode;

		private List<Thing> tmpThings = new List<Thing>();

		private List<Pawn> tmpSavedPawns = new List<Pawn>();

		public Thing SingleContainedThing
		{
			get
			{
				if (this.innerContainer.Count == 0)
				{
					return null;
				}
				if (this.innerContainer.Count > 1)
				{
					Log.Error("ContainedThing used on a DropPodInfo holding > 1 thing.");
				}
				return this.innerContainer[0];
			}
			set
			{
				this.innerContainer.Clear();
				this.innerContainer.TryAdd(value, true);
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return this.parent;
			}
		}

		public ActiveDropPodInfo()
		{
			this.innerContainer = new ThingOwner<Thing>(this);
		}

		public ActiveDropPodInfo(IThingHolder parent)
		{
			this.innerContainer = new ThingOwner<Thing>(this);
			this.parent = parent;
		}

		public void ExposeData()
		{
			if (this.savePawnsWithReferenceMode && Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpThings.Clear();
				this.tmpThings.AddRange(this.innerContainer);
				this.tmpSavedPawns.Clear();
				for (int i = 0; i < this.tmpThings.Count; i++)
				{
					Pawn pawn = this.tmpThings[i] as Pawn;
					if (pawn != null)
					{
						this.innerContainer.Remove(pawn);
						this.tmpSavedPawns.Add(pawn);
					}
				}
				this.tmpThings.Clear();
			}
			Scribe_Values.Look<bool>(ref this.savePawnsWithReferenceMode, "savePawnsWithReferenceMode", false, false);
			if (this.savePawnsWithReferenceMode)
			{
				Scribe_Collections.Look<Pawn>(ref this.tmpSavedPawns, "tmpSavedPawns", LookMode.Reference, new object[0]);
			}
			Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Values.Look<int>(ref this.openDelay, "openDelay", 110, false);
			Scribe_Values.Look<bool>(ref this.leaveSlag, "leaveSlag", false, false);
			if (this.savePawnsWithReferenceMode && (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving))
			{
				for (int j = 0; j < this.tmpSavedPawns.Count; j++)
				{
					this.innerContainer.TryAdd(this.tmpSavedPawns[j], true);
				}
				this.tmpSavedPawns.Clear();
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
