using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_BedsTracker : IExposable
	{
		public Caravan caravan;

		private Dictionary<Pawn, Building_Bed> usedBeds = new Dictionary<Pawn, Building_Bed>();

		private static List<Building_Bed> tmpUsableBeds = new List<Building_Bed>();

		private static List<string> tmpPawnLabels = new List<string>();

		public Caravan_BedsTracker()
		{
		}

		public Caravan_BedsTracker(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void BedsTrackerTick()
		{
			this.RecalculateUsedBeds();
			foreach (KeyValuePair<Pawn, Building_Bed> current in this.usedBeds)
			{
				PawnUtility.GainComfortFromThingIfPossible(current.Key, current.Value);
			}
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.RecalculateUsedBeds();
			}
		}

		private void RecalculateUsedBeds()
		{
			this.usedBeds.Clear();
			if (!this.caravan.Spawned)
			{
				return;
			}
			Caravan_BedsTracker.tmpUsableBeds.Clear();
			this.GetUsableBeds(Caravan_BedsTracker.tmpUsableBeds);
			if (!this.caravan.pather.MovingNow)
			{
				Caravan_BedsTracker.tmpUsableBeds.SortByDescending((Building_Bed x) => x.GetStatValue(StatDefOf.BedRestEffectiveness, true));
				for (int i = 0; i < this.caravan.pawns.Count; i++)
				{
					Pawn pawn = this.caravan.pawns[i];
					if (pawn.needs != null && pawn.needs.rest != null)
					{
						Building_Bed andRemoveFirstAvailableBedFor = this.GetAndRemoveFirstAvailableBedFor(pawn, Caravan_BedsTracker.tmpUsableBeds);
						if (andRemoveFirstAvailableBedFor != null)
						{
							this.usedBeds.Add(pawn, andRemoveFirstAvailableBedFor);
						}
					}
				}
			}
			else
			{
				Caravan_BedsTracker.tmpUsableBeds.SortByDescending((Building_Bed x) => x.GetStatValue(StatDefOf.ImmunityGainSpeedFactor, true));
				for (int j = 0; j < this.caravan.pawns.Count; j++)
				{
					Pawn pawn2 = this.caravan.pawns[j];
					if (pawn2.needs != null && pawn2.needs.rest != null)
					{
						if (CaravanBedUtility.WouldBenefitFromRestingInBed(pawn2))
						{
							if (!this.caravan.pather.MovingNow || pawn2.CarriedByCaravan())
							{
								Building_Bed andRemoveFirstAvailableBedFor2 = this.GetAndRemoveFirstAvailableBedFor(pawn2, Caravan_BedsTracker.tmpUsableBeds);
								if (andRemoveFirstAvailableBedFor2 != null)
								{
									this.usedBeds.Add(pawn2, andRemoveFirstAvailableBedFor2);
								}
							}
						}
					}
				}
			}
			Caravan_BedsTracker.tmpUsableBeds.Clear();
		}

		public void Notify_CaravanSpawned()
		{
			this.RecalculateUsedBeds();
		}

		public void Notify_PawnRemoved()
		{
			this.RecalculateUsedBeds();
		}

		public Building_Bed GetBedUsedBy(Pawn p)
		{
			Building_Bed building_Bed;
			if (this.usedBeds.TryGetValue(p, out building_Bed) && !building_Bed.DestroyedOrNull())
			{
				return building_Bed;
			}
			return null;
		}

		public bool IsInBed(Pawn p)
		{
			return this.GetBedUsedBy(p) != null;
		}

		public int GetUsedBedCount()
		{
			return this.usedBeds.Count;
		}

		private void GetUsableBeds(List<Building_Bed> outBeds)
		{
			outBeds.Clear();
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(this.caravan);
			for (int i = 0; i < list.Count; i++)
			{
				Building_Bed building_Bed = list[i].GetInnerIfMinified() as Building_Bed;
				if (building_Bed != null && building_Bed.def.building.bed_caravansCanUse)
				{
					for (int j = 0; j < list[i].stackCount; j++)
					{
						for (int k = 0; k < building_Bed.SleepingSlotsCount; k++)
						{
							outBeds.Add(building_Bed);
						}
					}
				}
			}
		}

		private Building_Bed GetAndRemoveFirstAvailableBedFor(Pawn p, List<Building_Bed> beds)
		{
			for (int i = 0; i < beds.Count; i++)
			{
				if (RestUtility.CanUseBedEver(p, beds[i].def))
				{
					Building_Bed result = beds[i];
					beds.RemoveAt(i);
					return result;
				}
			}
			return null;
		}

		public string GetInBedForMedicalReasonsInspectStringLine()
		{
			if (this.usedBeds.Count == 0)
			{
				return null;
			}
			Caravan_BedsTracker.tmpPawnLabels.Clear();
			foreach (KeyValuePair<Pawn, Building_Bed> current in this.usedBeds)
			{
				if (!this.caravan.carryTracker.IsCarried(current.Key))
				{
					if (CaravanBedUtility.WouldBenefitFromRestingInBed(current.Key))
					{
						Caravan_BedsTracker.tmpPawnLabels.Add(current.Key.LabelShort);
					}
				}
			}
			if (!Caravan_BedsTracker.tmpPawnLabels.Any<string>())
			{
				return null;
			}
			string str = (Caravan_BedsTracker.tmpPawnLabels.Count <= 5) ? Caravan_BedsTracker.tmpPawnLabels.ToCommaList(true) : (Caravan_BedsTracker.tmpPawnLabels.Take(5).ToCommaList(false) + "...");
			Caravan_BedsTracker.tmpPawnLabels.Clear();
			return "UsingBedrollsDueToIllness".Translate() + ": " + str;
		}
	}
}
