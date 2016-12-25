using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class PassingShipManager : IExposable
	{
		public Map map;

		public List<PassingShip> passingShips = new List<PassingShip>();

		private static List<PassingShip> tmpPassingShips = new List<PassingShip>();

		public PassingShipManager(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<PassingShip>(ref this.passingShips, "passingShips", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < this.passingShips.Count; i++)
				{
					this.passingShips[i].passingShipManager = this;
				}
			}
		}

		public void AddShip(PassingShip vis)
		{
			this.passingShips.Add(vis);
			vis.passingShipManager = this;
		}

		public void RemoveShip(PassingShip vis)
		{
			this.passingShips.Remove(vis);
			vis.passingShipManager = null;
		}

		public void PassingShipManagerTick()
		{
			for (int i = this.passingShips.Count - 1; i >= 0; i--)
			{
				this.passingShips[i].PassingShipTick();
			}
		}

		internal void DebugSendAllShipsAway()
		{
			PassingShipManager.tmpPassingShips.Clear();
			PassingShipManager.tmpPassingShips.AddRange(this.passingShips);
			for (int i = 0; i < PassingShipManager.tmpPassingShips.Count; i++)
			{
				PassingShipManager.tmpPassingShips[i].Depart();
			}
			Messages.Message("All passing ships sent away.", MessageSound.Standard);
		}
	}
}
