using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanPawnsAndItemsAbandonUtility
	{
		private const float DeathChanceForPawnAbandonedToDie = 0.8f;

		private static List<Hediff> tmpHediffs = new List<Hediff>();

		public static void TryAbandonViaInterface(Thing t, Caravan caravan)
		{
			Pawn p = t as Pawn;
			if (p != null)
			{
				if (!caravan.PawnsListForReading.Any((Pawn x) => x != p && caravan.IsOwner(x)))
				{
					Messages.Message("MessageCantAbandonLastColonist".Translate(), caravan, MessageSound.RejectInput);
					return;
				}
				Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(CaravanPawnsAndItemsAbandonUtility.GetAbandonPawnDialogText(p, caravan), delegate
				{
					bool flag = CaravanPawnsAndItemsAbandonUtility.WouldBeLeftToDie(p, caravan);
					PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(p, null, (!flag) ? PawnDiedOrDownedThoughtsKind.Abandoned : PawnDiedOrDownedThoughtsKind.AbandonedToDie);
					CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(p, caravan.PawnsListForReading, null);
					caravan.RemovePawn(p);
					if (flag)
					{
						if (Rand.Value < 0.8f)
						{
							p.Destroy(DestroyMode.Vanish);
						}
						else
						{
							CaravanPawnsAndItemsAbandonUtility.HealIfPossible(p);
						}
					}
					Find.WorldPawns.DiscardIfUnimportant(p);
				}, true, null);
				Find.WindowStack.Add(window);
			}
			else
			{
				Dialog_MessageBox window2 = Dialog_MessageBox.CreateConfirmation("ConfirmAbandonItemDialog".Translate(new object[]
				{
					t.Label
				}), delegate
				{
					Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, t);
					if (ownerOf == null)
					{
						Log.Error("Could not find owner of " + t);
						return;
					}
					ownerOf.inventory.innerContainer.Remove(t);
					t.Destroy(DestroyMode.Vanish);
					caravan.RecacheImmobilizedNow();
					caravan.RecacheDaysWorthOfFood();
				}, true, null);
				Find.WindowStack.Add(window2);
			}
		}

		public static void TryAbandonSpecificCountViaInterface(Thing t, Caravan caravan)
		{
			Find.WindowStack.Add(new Dialog_Slider("AbandonSliderText".Translate(new object[]
			{
				t.LabelNoCount
			}), 1, t.stackCount, delegate(int x)
			{
				Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, t);
				if (ownerOf == null)
				{
					Log.Error("Could not find owner of " + t);
					return;
				}
				if (x == t.stackCount)
				{
					ownerOf.inventory.innerContainer.Remove(t);
					t.Destroy(DestroyMode.Vanish);
				}
				else
				{
					t.SplitOff(x).Destroy(DestroyMode.Vanish);
				}
				caravan.RecacheImmobilizedNow();
				caravan.RecacheDaysWorthOfFood();
			}, -2147483648));
		}

		public static string GetAbandonButtonTooltip(Thing t, Caravan caravan, bool abandonSpecificCount)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				stringBuilder.Append("AbandonTip".Translate());
				if (CaravanPawnsAndItemsAbandonUtility.WouldBeLeftToDie(pawn, caravan))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.Append("AbandonTipWillDie".Translate(new object[]
					{
						pawn.LabelShort
					}).CapitalizeFirst());
				}
			}
			else
			{
				if (t.stackCount == 1)
				{
					stringBuilder.AppendLine("AbandonTip".Translate());
				}
				else if (abandonSpecificCount)
				{
					stringBuilder.AppendLine("AbandonSpecificCountTip".Translate());
				}
				else
				{
					stringBuilder.AppendLine("AbandonAllTip".Translate());
				}
				stringBuilder.AppendLine();
				stringBuilder.Append("AbandonItemTipExtraText".Translate());
			}
			return stringBuilder.ToString();
		}

		private static void HealIfPossible(Pawn p)
		{
			CaravanPawnsAndItemsAbandonUtility.tmpHediffs.Clear();
			CaravanPawnsAndItemsAbandonUtility.tmpHediffs.AddRange(p.health.hediffSet.hediffs);
			for (int i = 0; i < CaravanPawnsAndItemsAbandonUtility.tmpHediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury = CaravanPawnsAndItemsAbandonUtility.tmpHediffs[i] as Hediff_Injury;
				if (hediff_Injury != null && !hediff_Injury.IsOld())
				{
					p.health.RemoveHediff(hediff_Injury);
				}
				else
				{
					ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(CaravanPawnsAndItemsAbandonUtility.tmpHediffs[i].def);
					if (immunityRecord != null)
					{
						immunityRecord.immunity = 1f;
					}
				}
			}
		}

		private static string GetAbandonPawnDialogText(Pawn abandonedPawn, Caravan caravan)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = CaravanPawnsAndItemsAbandonUtility.WouldBeLeftToDie(abandonedPawn, caravan);
			stringBuilder.Append("ConfirmAbandonPawnDialog".Translate(new object[]
			{
				abandonedPawn.Label
			}));
			if (flag)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ConfirmAbandonPawnDialog_LeftToDie".Translate(new object[]
				{
					abandonedPawn.LabelShort
				}).CapitalizeFirst());
			}
			List<ThingWithComps> list = (abandonedPawn.equipment == null) ? null : abandonedPawn.equipment.AllEquipmentListForReading;
			List<Apparel> list2 = (abandonedPawn.apparel == null) ? null : abandonedPawn.apparel.WornApparel;
			if (!list.NullOrEmpty<ThingWithComps>() || !list2.NullOrEmpty<Apparel>())
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("ConfirmAbandonPawnDialog_EquipmentAndApparel".Translate(new object[]
				{
					abandonedPawn.LabelShort
				}).CapitalizeFirst().AdjustedFor(abandonedPawn));
				stringBuilder.AppendLine();
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						stringBuilder.AppendLine();
						stringBuilder.Append("  - " + list[i].LabelCap);
					}
				}
				if (list2 != null)
				{
					for (int j = 0; j < list2.Count; j++)
					{
						stringBuilder.AppendLine();
						stringBuilder.Append("  - " + list2[j].LabelCap);
					}
				}
			}
			PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(abandonedPawn, null, (!flag) ? PawnDiedOrDownedThoughtsKind.Abandoned : PawnDiedOrDownedThoughtsKind.AbandonedToDie, stringBuilder, "\n\n" + "ConfirmAbandonPawnDialog_IndividualThoughts".Translate(new object[]
			{
				abandonedPawn.LabelShort
			}), "\n\n" + "ConfirmAbandonPawnDialog_AllColonistsThoughts".Translate());
			return stringBuilder.ToString();
		}

		private static bool WouldBeLeftToDie(Pawn p, Caravan caravan)
		{
			if (p.Downed)
			{
				return true;
			}
			if (p.health.hediffSet.BleedRateTotal > 0.4f)
			{
				return true;
			}
			float f = GenTemperature.AverageTemperatureAtTileForTwelfth(caravan.Tile, GenLocalDate.Twelfth(caravan.Tile));
			if (!p.ComfortableTemperatureRange().Includes(f))
			{
				return true;
			}
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffStage curStage = hediffs[i].CurStage;
				if (curStage != null && curStage.lifeThreatening)
				{
					return true;
				}
			}
			return false;
		}
	}
}
