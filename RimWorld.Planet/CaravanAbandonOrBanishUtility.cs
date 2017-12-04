using System;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanAbandonOrBanishUtility
	{
		public static void TryAbandonOrBanishViaInterface(Thing t, Caravan caravan)
		{
			Pawn p = t as Pawn;
			if (p != null)
			{
				if (!caravan.PawnsListForReading.Any((Pawn x) => x != p && caravan.IsOwner(x)))
				{
					Messages.Message("MessageCantBanishLastColonist".Translate(), caravan, MessageTypeDefOf.RejectInput);
					return;
				}
				PawnBanishUtility.ShowBanishPawnConfirmationDialog(p);
			}
			else
			{
				Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("ConfirmAbandonItemDialog".Translate(new object[]
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
				Find.WindowStack.Add(window);
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

		public static string GetAbandonOrBanishButtonTooltip(Thing t, Caravan caravan, bool abandonSpecificCount)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				return PawnBanishUtility.GetBanishButtonTip(pawn);
			}
			StringBuilder stringBuilder = new StringBuilder();
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
			return stringBuilder.ToString();
		}
	}
}
