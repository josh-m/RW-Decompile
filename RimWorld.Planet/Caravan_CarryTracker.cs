using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_CarryTracker : IExposable
	{
		public Caravan caravan;

		private List<Pawn> carriedPawns = new List<Pawn>();

		private static List<Pawn> tmpPawnsWhoCanCarry = new List<Pawn>();

		private static List<string> tmpPawnLabels = new List<string>();

		public List<Pawn> CarriedPawnsListForReading
		{
			get
			{
				return this.carriedPawns;
			}
		}

		public Caravan_CarryTracker()
		{
		}

		public Caravan_CarryTracker(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void CarryTrackerTick()
		{
			this.RecalculateCarriedPawns();
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.RecalculateCarriedPawns();
			}
		}

		public bool IsCarried(Pawn p)
		{
			return this.carriedPawns.Contains(p);
		}

		private void RecalculateCarriedPawns()
		{
			this.carriedPawns.Clear();
			if (!this.caravan.Spawned)
			{
				return;
			}
			if (this.caravan.pather.MovingNow)
			{
				Caravan_CarryTracker.tmpPawnsWhoCanCarry.Clear();
				this.CalculatePawnsWhoCanCarry(Caravan_CarryTracker.tmpPawnsWhoCanCarry);
				for (int i = 0; i < this.caravan.pawns.Count; i++)
				{
					if (!Caravan_CarryTracker.tmpPawnsWhoCanCarry.Any<Pawn>())
					{
						break;
					}
					Pawn pawn = this.caravan.pawns[i];
					if (this.WantsToBeCarried(pawn))
					{
						if (Caravan_CarryTracker.tmpPawnsWhoCanCarry.Any<Pawn>())
						{
							this.carriedPawns.Add(pawn);
							Caravan_CarryTracker.tmpPawnsWhoCanCarry.RemoveLast<Pawn>();
						}
					}
				}
				Caravan_CarryTracker.tmpPawnsWhoCanCarry.Clear();
			}
		}

		public void Notify_CaravanSpawned()
		{
			this.RecalculateCarriedPawns();
		}

		public void Notify_PawnRemoved()
		{
			this.RecalculateCarriedPawns();
		}

		private void CalculatePawnsWhoCanCarry(List<Pawn> outPawns)
		{
			outPawns.Clear();
			for (int i = 0; i < this.caravan.pawns.Count; i++)
			{
				Pawn pawn = this.caravan.pawns[i];
				if (pawn.RaceProps.Humanlike && !pawn.Downed && !pawn.InMentalState)
				{
					if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						if (!this.WantsToBeCarried(pawn))
						{
							outPawns.Add(pawn);
						}
					}
				}
			}
		}

		private bool WantsToBeCarried(Pawn p)
		{
			return p.health.beCarriedByCaravanIfSick && CaravanCarryUtility.WouldBenefitFromBeingCarried(p);
		}

		public string GetInspectStringLine()
		{
			if (!this.carriedPawns.Any<Pawn>())
			{
				return null;
			}
			Caravan_CarryTracker.tmpPawnLabels.Clear();
			int num = 0;
			for (int i = 0; i < this.carriedPawns.Count; i++)
			{
				Caravan_CarryTracker.tmpPawnLabels.Add(this.carriedPawns[i].LabelShort);
				if (this.caravan.beds.IsInBed(this.carriedPawns[i]))
				{
					num++;
				}
			}
			string str = (Caravan_CarryTracker.tmpPawnLabels.Count <= 5) ? Caravan_CarryTracker.tmpPawnLabels.ToCommaList(true) : (Caravan_CarryTracker.tmpPawnLabels.Take(5).ToCommaList(false) + "...");
			string result = CaravanBedUtility.AppendUsingBedsLabel("BeingCarriedDueToIllness".Translate() + ": " + str.CapitalizeFirst(), num);
			Caravan_CarryTracker.tmpPawnLabels.Clear();
			return result;
		}
	}
}
