using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class RetainedCaravanData : IExposable
	{
		private Map map;

		private bool shouldPassStoryState;

		private int nextTile = -1;

		private float nextTileCostLeftPct;

		private bool paused;

		private int destinationTile = -1;

		private CaravanArrivalAction arrivalAction;

		public bool HasDestinationTile
		{
			get
			{
				return this.destinationTile != -1;
			}
		}

		public RetainedCaravanData(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.shouldPassStoryState, "shouldPassStoryState", false, false);
			Scribe_Values.Look<int>(ref this.nextTile, "nextTile", -1, false);
			Scribe_Values.Look<float>(ref this.nextTileCostLeftPct, "nextTileCostLeftPct", -1f, false);
			Scribe_Values.Look<bool>(ref this.paused, "paused", false, false);
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Deep.Look<CaravanArrivalAction>(ref this.arrivalAction, "arrivalAction", new object[0]);
		}

		public void Notify_GeneratedTempIncidentMapFor(Caravan caravan)
		{
			if (!this.map.Parent.def.isTempIncidentMapOwner)
			{
				return;
			}
			this.Set(caravan);
		}

		public void Notify_CaravanFormed(Caravan caravan)
		{
			if (this.shouldPassStoryState)
			{
				this.shouldPassStoryState = false;
				this.map.StoryState.CopyTo(caravan.StoryState);
			}
			if (this.nextTile != -1 && this.nextTile != caravan.Tile && caravan.CanReach(this.nextTile))
			{
				caravan.pather.StartPath(this.nextTile, null, true, true);
				caravan.pather.nextTileCostLeft = caravan.pather.nextTileCostTotal * this.nextTileCostLeftPct;
				caravan.pather.Paused = this.paused;
				caravan.tweener.ResetTweenedPosToRoot();
			}
			if (this.HasDestinationTile && this.destinationTile != caravan.Tile)
			{
				caravan.pather.StartPath(this.destinationTile, this.arrivalAction, true, true);
				this.destinationTile = -1;
				this.arrivalAction = null;
			}
		}

		private void Set(Caravan caravan)
		{
			caravan.StoryState.CopyTo(this.map.StoryState);
			this.shouldPassStoryState = true;
			if (caravan.pather.Moving)
			{
				this.nextTile = caravan.pather.nextTile;
				this.nextTileCostLeftPct = caravan.pather.nextTileCostLeft / caravan.pather.nextTileCostTotal;
				this.paused = caravan.pather.Paused;
				this.destinationTile = caravan.pather.Destination;
				this.arrivalAction = caravan.pather.ArrivalAction;
			}
			else
			{
				this.nextTile = -1;
				this.nextTileCostLeftPct = 0f;
				this.paused = false;
				this.destinationTile = -1;
				this.arrivalAction = null;
			}
		}
	}
}
