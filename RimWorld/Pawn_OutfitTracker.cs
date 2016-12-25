using System;
using Verse;

namespace RimWorld
{
	public class Pawn_OutfitTracker : IExposable
	{
		public Pawn pawn;

		private Outfit curOutfit;

		public OutfitForcedHandler forcedHandler = new OutfitForcedHandler();

		public Outfit CurrentOutfit
		{
			get
			{
				if (this.curOutfit == null)
				{
					this.curOutfit = Current.Game.outfitDatabase.DefaultOutfit();
				}
				return this.curOutfit;
			}
			set
			{
				if (this.curOutfit == value)
				{
					return;
				}
				this.curOutfit = value;
				this.pawn.mindState.Notify_OutfitChanged();
			}
		}

		public Pawn_OutfitTracker()
		{
		}

		public Pawn_OutfitTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Outfit>(ref this.curOutfit, "curOutfit", false);
			Scribe_Deep.LookDeep<OutfitForcedHandler>(ref this.forcedHandler, "overrideHandler", new object[0]);
		}
	}
}
