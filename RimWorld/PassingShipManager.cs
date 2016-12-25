using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class PassingShipManager : IExposable
	{
		public List<PassingShip> passingShips = new List<PassingShip>();

		public void ExposeData()
		{
			Scribe_Collections.LookList<PassingShip>(ref this.passingShips, "passingShips", LookMode.Deep, new object[0]);
		}

		public void AddShip(PassingShip vis)
		{
			this.passingShips.Add(vis);
		}

		public void RemoveShip(PassingShip vis)
		{
			this.passingShips.Remove(vis);
		}

		public void PassingShipManagerTick()
		{
			for (int i = this.passingShips.Count - 1; i >= 0; i--)
			{
				this.passingShips[i].VisitorTick();
			}
		}

		internal void DebugSendAllShipsAway()
		{
			this.passingShips.Clear();
			Messages.Message("All passing ships sent away.", MessageSound.Standard);
		}
	}
}
