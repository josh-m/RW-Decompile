using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatsRecord : IExposable
	{
		public int numRaidsEnemy;

		public int numThreatBigs;

		public int colonistsKilled;

		public int colonistsLaunched;

		public int greatestPopulation;

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.numRaidsEnemy, "numRaidsEnemy", 0, false);
			Scribe_Values.Look<int>(ref this.numThreatBigs, "numThreatsQueued", 0, false);
			Scribe_Values.Look<int>(ref this.colonistsKilled, "colonistsKilled", 0, false);
			Scribe_Values.Look<int>(ref this.colonistsLaunched, "colonistsLaunched", 0, false);
			Scribe_Values.Look<int>(ref this.greatestPopulation, "greatestPopulation", 3, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.UpdateGreatestPopulation();
			}
		}

		public void Notify_ColonistKilled()
		{
			this.colonistsKilled++;
		}

		public void UpdateGreatestPopulation()
		{
			int a = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Count<Pawn>();
			this.greatestPopulation = Mathf.Max(a, this.greatestPopulation);
		}
	}
}
