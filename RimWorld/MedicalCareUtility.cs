using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class MedicalCareUtility
	{
		public const float CareSetterHeight = 28f;

		public const float CareSetterWidth = 140f;

		private static Texture2D[] careTextures;

		public static void Reset()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				MedicalCareUtility.careTextures = new Texture2D[5];
				MedicalCareUtility.careTextures[0] = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare", true);
				MedicalCareUtility.careTextures[1] = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoMeds", true);
				MedicalCareUtility.careTextures[2] = ThingDefOf.HerbalMedicine.uiIcon;
				MedicalCareUtility.careTextures[3] = ThingDefOf.Medicine.uiIcon;
				MedicalCareUtility.careTextures[4] = ThingDefOf.GlitterworldMedicine.uiIcon;
			});
		}

		public static void MedicalCareSetter(Rect rect, ref MedicalCareCategory medCare)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width / 5f, rect.height);
			for (int i = 0; i < 5; i++)
			{
				MedicalCareCategory mc = (MedicalCareCategory)i;
				Widgets.DrawHighlightIfMouseover(rect2);
				GUI.DrawTexture(rect2, MedicalCareUtility.careTextures[i]);
				if (Widgets.ButtonInvisible(rect2, false))
				{
					medCare = mc;
					SoundDefOf.TickHigh.PlayOneShotOnCamera();
				}
				if (medCare == mc)
				{
					Widgets.DrawBox(rect2, 3);
				}
				TooltipHandler.TipRegion(rect2, () => mc.GetLabel(), 632165 + i * 17);
				rect2.x += rect2.width;
			}
		}

		public static string GetLabel(this MedicalCareCategory cat)
		{
			return ("MedicalCareCategory_" + cat).Translate();
		}

		public static bool AllowsMedicine(this MedicalCareCategory cat, ThingDef meds)
		{
			switch (cat)
			{
			case MedicalCareCategory.NoCare:
				return false;
			case MedicalCareCategory.NoMeds:
				return false;
			case MedicalCareCategory.HerbalOrWorse:
				return meds.GetStatValueAbstract(StatDefOf.MedicalPotency, null) <= ThingDefOf.HerbalMedicine.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
			case MedicalCareCategory.NormalOrWorse:
				return meds.GetStatValueAbstract(StatDefOf.MedicalPotency, null) <= ThingDefOf.Medicine.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
			case MedicalCareCategory.Best:
				return true;
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
