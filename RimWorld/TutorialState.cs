using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TutorialState : IExposable
	{
		public List<Thing> startingItems = new List<Thing>();

		public CellRect roomRect;

		public CellRect sandbagsRect;

		public int endTick = -1;

		public bool introDone;

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && this.startingItems != null)
			{
				this.startingItems.RemoveAll((Thing it) => it == null || it.Destroyed || (it.Map == null && it.MapHeld == null));
			}
			Scribe_Collections.LookList<Thing>(ref this.startingItems, "startingItems", LookMode.Reference, new object[0]);
			Scribe_Values.LookValue<CellRect>(ref this.roomRect, "roomRect", default(CellRect), false);
			Scribe_Values.LookValue<CellRect>(ref this.sandbagsRect, "sandbagsRect", default(CellRect), false);
			Scribe_Values.LookValue<int>(ref this.endTick, "endTick", -1, false);
			Scribe_Values.LookValue<bool>(ref this.introDone, "introDone", false, false);
			if (this.startingItems != null)
			{
				this.startingItems.RemoveAll((Thing it) => it == null);
			}
		}

		public void Notify_TutorialEnding()
		{
			this.startingItems.Clear();
			this.roomRect = default(CellRect);
			this.sandbagsRect = default(CellRect);
			this.endTick = Find.TickManager.TicksGame;
		}

		public void AddStartingItem(Thing t)
		{
			if (this.startingItems.Contains(t))
			{
				return;
			}
			this.startingItems.Add(t);
		}
	}
}
