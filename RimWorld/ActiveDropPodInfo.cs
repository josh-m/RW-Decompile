using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ActiveDropPodInfo : IExposable, IThingContainerOwner
	{
		public const int DefaultOpenDelay = 110;

		public IActiveDropPod parent;

		public ThingContainer innerContainer;

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

		public bool Spawned
		{
			get
			{
				return this.parent != null && ((Thing)this.parent).Spawned;
			}
		}

		public ActiveDropPodInfo()
		{
			this.innerContainer = new ThingContainer(this);
		}

		public ActiveDropPodInfo(IActiveDropPod parent)
		{
			this.innerContainer = new ThingContainer(this);
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
			Scribe_Values.LookValue<bool>(ref this.savePawnsWithReferenceMode, "savePawnsWithReferenceMode", false, false);
			if (this.savePawnsWithReferenceMode)
			{
				Scribe_Collections.LookList<Pawn>(ref this.tmpSavedPawns, "tmpSavedPawns", LookMode.Reference, new object[0]);
			}
			Scribe_Deep.LookDeep<ThingContainer>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Values.LookValue<int>(ref this.openDelay, "openDelay", 110, false);
			Scribe_Values.LookValue<bool>(ref this.leaveSlag, "leaveSlag", false, false);
			if (this.savePawnsWithReferenceMode && (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving))
			{
				for (int j = 0; j < this.tmpSavedPawns.Count; j++)
				{
					this.innerContainer.TryAdd(this.tmpSavedPawns[j], true);
				}
				this.tmpSavedPawns.Clear();
			}
		}

		public ThingContainer GetInnerContainer()
		{
			return this.innerContainer;
		}

		public IntVec3 GetPosition()
		{
			if (this.parent == null)
			{
				return IntVec3.Invalid;
			}
			return ((Thing)this.parent).PositionHeld;
		}

		public Map GetMap()
		{
			if (this.parent == null)
			{
				return null;
			}
			return ((Thing)this.parent).MapHeld;
		}
	}
}
