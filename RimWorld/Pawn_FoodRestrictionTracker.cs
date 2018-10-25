using System;
using Verse;

namespace RimWorld
{
	public class Pawn_FoodRestrictionTracker : IExposable
	{
		public Pawn pawn;

		private FoodRestriction curRestriction;

		public FoodRestriction CurrentFoodRestriction
		{
			get
			{
				if (this.curRestriction == null)
				{
					this.curRestriction = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
				}
				return this.curRestriction;
			}
			set
			{
				this.curRestriction = value;
			}
		}

		public bool Configurable
		{
			get
			{
				return this.pawn.RaceProps.Humanlike && !this.pawn.Destroyed && (this.pawn.Faction == Faction.OfPlayer || this.pawn.HostFaction == Faction.OfPlayer);
			}
		}

		public Pawn_FoodRestrictionTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Pawn_FoodRestrictionTracker()
		{
		}

		public FoodRestriction GetCurrentRespectedRestriction(Pawn getter = null)
		{
			if (!this.Configurable)
			{
				return null;
			}
			if (this.pawn.Faction != Faction.OfPlayer && (getter == null || getter.Faction != Faction.OfPlayer))
			{
				return null;
			}
			if (this.pawn.InMentalState)
			{
				return null;
			}
			return this.CurrentFoodRestriction;
		}

		public void ExposeData()
		{
			Scribe_References.Look<FoodRestriction>(ref this.curRestriction, "curRestriction", false);
		}
	}
}
