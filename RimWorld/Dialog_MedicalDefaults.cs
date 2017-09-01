using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_MedicalDefaults : Window
	{
		private const float MedicalCareStartX = 170f;

		private const float VerticalGap = 6f;

		private const float VerticalBigGap = 24f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(346f, 350f);
			}
		}

		public Dialog_MedicalDefaults()
		{
			this.forcePause = true;
			this.doCloseX = true;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, 170f, 28f);
			Rect rect2 = new Rect(170f, 0f, 140f, 28f);
			Widgets.Label(rect, "MedGroupColonist".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.World.settings.defaultCareForColonyHumanlike);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.Label(rect, "MedGroupImprisonedColonist".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.World.settings.defaultCareForColonyPrisoner);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.Label(rect, "MedGroupColonyAnimal".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.World.settings.defaultCareForColonyAnimal);
			rect.y += 52f;
			rect2.y += 52f;
			Widgets.Label(rect, "MedGroupNeutralAnimal".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.World.settings.defaultCareForNeutralAnimal);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.Label(rect, "MedGroupNeutralFaction".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.World.settings.defaultCareForNeutralFaction);
			rect.y += 52f;
			rect2.y += 52f;
			Widgets.Label(rect, "MedGroupHostileFaction".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.World.settings.defaultCareForHostileFaction);
		}
	}
}
