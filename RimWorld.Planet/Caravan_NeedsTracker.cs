using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_NeedsTracker : IExposable
	{
		public Caravan caravan;

		private static List<JoyKindDef> tmpAvailableJoyKinds = new List<JoyKindDef>();

		private static List<Thing> tmpInvFood = new List<Thing>();

		public Caravan_NeedsTracker()
		{
		}

		public Caravan_NeedsTracker(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void ExposeData()
		{
		}

		public void NeedsTrackerTick()
		{
			this.TrySatisfyPawnsNeeds();
		}

		public void TrySatisfyPawnsNeeds()
		{
			List<Pawn> pawnsListForReading = this.caravan.PawnsListForReading;
			for (int i = pawnsListForReading.Count - 1; i >= 0; i--)
			{
				this.TrySatisfyPawnNeeds(pawnsListForReading[i]);
			}
		}

		private void TrySatisfyPawnNeeds(Pawn pawn)
		{
			if (pawn.Dead)
			{
				return;
			}
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				Need need = allNeeds[i];
				Need_Rest need_Rest = need as Need_Rest;
				Need_Food need_Food = need as Need_Food;
				Need_Chemical need_Chemical = need as Need_Chemical;
				Need_Joy need_Joy = need as Need_Joy;
				if (need_Rest != null)
				{
					this.TrySatisfyRestNeed(pawn, need_Rest);
				}
				else if (need_Food != null)
				{
					this.TrySatisfyFoodNeed(pawn, need_Food);
				}
				else if (need_Chemical != null)
				{
					this.TrySatisfyChemicalNeed(pawn, need_Chemical);
				}
				else if (need_Joy != null)
				{
					this.TrySatisfyJoyNeed(pawn, need_Joy);
				}
			}
		}

		private void TrySatisfyRestNeed(Pawn pawn, Need_Rest rest)
		{
			if (!this.caravan.pather.MovingNow || pawn.InCaravanBed() || pawn.CarriedByCaravan())
			{
				Building_Bed building_Bed = pawn.CurrentCaravanBed();
				float restEffectiveness = (building_Bed == null) ? 0.8f : building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness, true);
				rest.TickResting(restEffectiveness);
			}
		}

		private void TrySatisfyFoodNeed(Pawn pawn, Need_Food food)
		{
			if (food.CurCategory < HungerCategory.Hungry)
			{
				return;
			}
			if (VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
			{
				VirtualPlantsUtility.EatVirtualPlants(pawn);
				return;
			}
			Thing thing;
			Pawn pawn2;
			if (CaravanInventoryUtility.TryGetBestFood(this.caravan, pawn, out thing, out pawn2))
			{
				food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
				if (thing.Destroyed)
				{
					if (pawn2 != null)
					{
						pawn2.inventory.innerContainer.Remove(thing);
						this.caravan.RecacheImmobilizedNow();
						this.caravan.RecacheDaysWorthOfFood();
					}
					if (!this.caravan.notifiedOutOfFood && !CaravanInventoryUtility.TryGetBestFood(this.caravan, pawn, out thing, out pawn2))
					{
						Messages.Message("MessageCaravanRanOutOfFood".Translate(this.caravan.LabelCap, pawn.Label, pawn.Named("PAWN")), this.caravan, MessageTypeDefOf.ThreatBig, true);
						this.caravan.notifiedOutOfFood = true;
					}
				}
			}
		}

		private void TrySatisfyChemicalNeed(Pawn pawn, Need_Chemical chemical)
		{
			if (chemical.CurCategory >= DrugDesireCategory.Satisfied)
			{
				return;
			}
			Thing drug;
			Pawn drugOwner;
			if (CaravanInventoryUtility.TryGetDrugToSatisfyChemicalNeed(this.caravan, pawn, chemical, out drug, out drugOwner))
			{
				this.IngestDrug(pawn, drug, drugOwner);
			}
		}

		public void IngestDrug(Pawn pawn, Thing drug, Pawn drugOwner)
		{
			float num = drug.Ingested(pawn, 0f);
			Need_Food food = pawn.needs.food;
			if (food != null)
			{
				food.CurLevel += num;
			}
			if (drug.Destroyed && drugOwner != null)
			{
				drugOwner.inventory.innerContainer.Remove(drug);
				this.caravan.RecacheImmobilizedNow();
				this.caravan.RecacheDaysWorthOfFood();
			}
		}

		private void TrySatisfyJoyNeed(Pawn pawn, Need_Joy joy)
		{
			if (pawn.IsHashIntervalTick(1250))
			{
				float num = this.GetCurrentJoyGainPerTick(pawn);
				if (num <= 0f)
				{
					return;
				}
				num *= 1250f;
				Caravan_NeedsTracker.tmpAvailableJoyKinds.Clear();
				this.GetAvailableJoyKindsFor(pawn, Caravan_NeedsTracker.tmpAvailableJoyKinds);
				JoyKindDef joyKind;
				if (!Caravan_NeedsTracker.tmpAvailableJoyKinds.TryRandomElementByWeight((JoyKindDef x) => 1f - Mathf.Clamp01(pawn.needs.joy.tolerances[x]), out joyKind))
				{
					return;
				}
				joy.GainJoy(num, joyKind);
				Caravan_NeedsTracker.tmpAvailableJoyKinds.Clear();
			}
		}

		public float GetCurrentJoyGainPerTick(Pawn pawn)
		{
			if (this.caravan.pather.MovingNow)
			{
				return 0f;
			}
			return 4E-05f;
		}

		public bool AnyPawnOutOfFood(out string malnutritionHediff)
		{
			Caravan_NeedsTracker.tmpInvFood.Clear();
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(this.caravan);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def.IsNutritionGivingIngestible)
				{
					Caravan_NeedsTracker.tmpInvFood.Add(list[i]);
				}
			}
			List<Pawn> pawnsListForReading = this.caravan.PawnsListForReading;
			for (int j = 0; j < pawnsListForReading.Count; j++)
			{
				Pawn pawn = pawnsListForReading[j];
				if (pawn.RaceProps.EatsFood && !VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
				{
					bool flag = false;
					for (int k = 0; k < Caravan_NeedsTracker.tmpInvFood.Count; k++)
					{
						if (CaravanPawnsNeedsUtility.CanEatForNutritionEver(Caravan_NeedsTracker.tmpInvFood[k].def, pawn))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						int num = -1;
						string text = null;
						for (int l = 0; l < pawnsListForReading.Count; l++)
						{
							Hediff firstHediffOfDef = pawnsListForReading[l].health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
							if (firstHediffOfDef != null && (text == null || firstHediffOfDef.CurStageIndex > num))
							{
								num = firstHediffOfDef.CurStageIndex;
								text = firstHediffOfDef.LabelCap;
							}
						}
						malnutritionHediff = text;
						Caravan_NeedsTracker.tmpInvFood.Clear();
						return true;
					}
				}
			}
			malnutritionHediff = null;
			Caravan_NeedsTracker.tmpInvFood.Clear();
			return false;
		}

		private void GetAvailableJoyKindsFor(Pawn p, List<JoyKindDef> outJoyKinds)
		{
			outJoyKinds.Clear();
			if (!p.needs.joy.tolerances.BoredOf(JoyKindDefOf.Meditative))
			{
				outJoyKinds.Add(JoyKindDefOf.Meditative);
			}
			if (!p.needs.joy.tolerances.BoredOf(JoyKindDefOf.Social))
			{
				int num = 0;
				for (int i = 0; i < this.caravan.pawns.Count; i++)
				{
					if (this.caravan.pawns[i].RaceProps.Humanlike && !this.caravan.pawns[i].Downed && !this.caravan.pawns[i].InMentalState)
					{
						num++;
					}
				}
				if (num >= 2)
				{
					outJoyKinds.Add(JoyKindDefOf.Social);
				}
			}
		}
	}
}
