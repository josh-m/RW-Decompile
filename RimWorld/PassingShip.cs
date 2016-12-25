using System;
using Verse;

namespace RimWorld
{
	public class PassingShip : ICommunicable, ILoadReferenceable, IExposable
	{
		public PassingShipManager passingShipManager;

		public string name = "Nameless";

		protected int loadID = -1;

		public int ticksUntilDeparture = 40000;

		public virtual string FullTitle
		{
			get
			{
				return "ErrorFullTitle";
			}
		}

		public bool Departed
		{
			get
			{
				return this.ticksUntilDeparture <= 0;
			}
		}

		public Map Map
		{
			get
			{
				return this.passingShipManager.map;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.name, "name", null, false);
			Scribe_Values.LookValue<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.LookValue<int>(ref this.ticksUntilDeparture, "ticksUntilDeparture", 0, false);
		}

		public virtual void PassingShipTick()
		{
			this.ticksUntilDeparture--;
			if (this.Departed)
			{
				this.Depart();
			}
		}

		public virtual void Depart()
		{
			if (this.Map.listerBuildings.ColonistsHaveBuilding((Thing b) => b.def.IsCommsConsole))
			{
				Messages.Message("MessageShipHasLeftCommsRange".Translate(new object[]
				{
					this.FullTitle
				}), MessageSound.Silent);
			}
			this.passingShipManager.RemoveShip(this);
		}

		public virtual void TryOpenComms(Pawn negotiator)
		{
			throw new NotImplementedException();
		}

		public virtual string GetCallLabel()
		{
			return this.name;
		}

		public string GetInfoText()
		{
			return this.FullTitle;
		}

		public string GetUniqueLoadID()
		{
			return "PassingShip_" + this.loadID;
		}
	}
}
