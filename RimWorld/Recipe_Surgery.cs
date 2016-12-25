using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Recipe_Surgery : RecipeWorker
	{
		private const float CatastrophicFailChance = 0.5f;

		private const float RidiculousFailChance = 0.1f;

		protected bool CheckSurgeryFail(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part)
		{
			float num = 1f;
			float num2 = surgeon.GetStatValue(StatDefOf.SurgerySuccessChance, true);
			if (num2 < 1f)
			{
				num2 = Mathf.Pow(num2, this.recipe.surgeonSurgerySuccessChanceExponent);
			}
			num *= num2;
			Room room = surgeon.GetRoom();
			if (room != null)
			{
				float num3 = room.GetStat(RoomStatDefOf.SurgerySuccessChanceFactor);
				if (num3 < 1f)
				{
					num3 = Mathf.Pow(num3, this.recipe.roomSurgerySuccessChanceFactorExponent);
				}
				num *= num3;
			}
			num *= this.GetAverageMedicalPotency(ingredients);
			num *= this.recipe.surgerySuccessChanceFactor;
			if (Rand.Value > num)
			{
				if (Rand.Value < this.recipe.deathOnFailedSurgeryChance)
				{
					int num4 = 0;
					while (!patient.Dead)
					{
						HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
						num4++;
						if (num4 > 300)
						{
							Log.Error("Could not kill patient.");
							break;
						}
					}
				}
				else if (Rand.Value < 0.5f)
				{
					if (Rand.Value < 0.1f)
					{
						Messages.Message("MessageMedicalOperationFailureRidiculous".Translate(new object[]
						{
							surgeon.LabelShort,
							patient.LabelShort
						}), patient, MessageSound.SeriousAlert);
						HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
					}
					else
					{
						Messages.Message("MessageMedicalOperationFailureCatastrophic".Translate(new object[]
						{
							surgeon.LabelShort,
							patient.LabelShort
						}), patient, MessageSound.SeriousAlert);
						HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
					}
				}
				else
				{
					Messages.Message("MessageMedicalOperationFailureMinor".Translate(new object[]
					{
						surgeon.LabelShort,
						patient.LabelShort
					}), patient, MessageSound.Negative);
					HealthUtility.GiveInjuriesOperationFailureMinor(patient, part);
				}
				if (!patient.Dead)
				{
					this.TryGainBotchedSurgeryThought(patient, surgeon);
				}
				return true;
			}
			return false;
		}

		private void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
		{
			if (!patient.RaceProps.Humanlike)
			{
				return;
			}
			patient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.BotchedMySurgery, surgeon);
		}

		private float GetAverageMedicalPotency(List<Thing> ingredients)
		{
			if (ingredients.NullOrEmpty<Thing>())
			{
				return 1f;
			}
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < ingredients.Count; i++)
			{
				Medicine medicine = ingredients[i] as Medicine;
				if (medicine != null)
				{
					num += medicine.stackCount;
					num2 += medicine.GetStatValue(StatDefOf.MedicalPotency, true) * (float)medicine.stackCount;
				}
			}
			if (num == 0)
			{
				return 1f;
			}
			return num2 / (float)num;
		}
	}
}
