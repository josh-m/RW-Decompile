using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class PawnNameColorUtility
	{
		private const int ColorShiftCount = 10;

		private static readonly List<Color> ColorsNeutral;

		private static readonly List<Color> ColorsHostile;

		private static readonly List<Color> ColorsPrisoner;

		private static readonly Color ColorBaseNeutral;

		private static readonly Color ColorBaseHostile;

		private static readonly Color ColorBasePrisoner;

		private static readonly Color ColorColony;

		private static readonly List<Color> ColorShifts;

		static PawnNameColorUtility()
		{
			PawnNameColorUtility.ColorsNeutral = new List<Color>();
			PawnNameColorUtility.ColorsHostile = new List<Color>();
			PawnNameColorUtility.ColorsPrisoner = new List<Color>();
			PawnNameColorUtility.ColorBaseNeutral = new Color(0.4f, 0.85f, 0.9f);
			PawnNameColorUtility.ColorBaseHostile = new Color(0.9f, 0.2f, 0.2f);
			PawnNameColorUtility.ColorBasePrisoner = new Color(1f, 0.85f, 0.5f);
			PawnNameColorUtility.ColorColony = new Color(0.9f, 0.9f, 0.9f);
			PawnNameColorUtility.ColorShifts = new List<Color>
			{
				new Color(1f, 1f, 1f),
				new Color(0.8f, 1f, 1f),
				new Color(0.8f, 0.8f, 1f),
				new Color(0.8f, 0.8f, 0.8f),
				new Color(1.2f, 1f, 1f),
				new Color(0.8f, 1.2f, 1f),
				new Color(0.8f, 1.2f, 1.2f),
				new Color(1.2f, 1.2f, 1.2f),
				new Color(1f, 1.2f, 1f),
				new Color(1.2f, 1f, 0.8f)
			};
			for (int i = 0; i < 10; i++)
			{
				PawnNameColorUtility.ColorsNeutral.Add(PawnNameColorUtility.RandomShiftOf(PawnNameColorUtility.ColorBaseNeutral, i));
				PawnNameColorUtility.ColorsHostile.Add(PawnNameColorUtility.RandomShiftOf(PawnNameColorUtility.ColorBaseHostile, i));
				PawnNameColorUtility.ColorsPrisoner.Add(PawnNameColorUtility.RandomShiftOf(PawnNameColorUtility.ColorBasePrisoner, i));
			}
		}

		private static Color RandomShiftOf(Color color, int i)
		{
			return new Color(Mathf.Clamp01(color.r * PawnNameColorUtility.ColorShifts[i].r), Mathf.Clamp01(color.g * PawnNameColorUtility.ColorShifts[i].g), Mathf.Clamp01(color.b * PawnNameColorUtility.ColorShifts[i].b), color.a);
		}

		public static Color PawnNameColorOf(Pawn pawn)
		{
			if (pawn.MentalStateDef != null)
			{
				return pawn.MentalStateDef.nameColor;
			}
			int index;
			if (pawn.Faction == null)
			{
				index = 0;
			}
			else
			{
				index = pawn.Faction.randomKey % 10;
			}
			if (pawn.IsPrisoner)
			{
				return PawnNameColorUtility.ColorsPrisoner[index];
			}
			if (pawn.Faction == null)
			{
				return PawnNameColorUtility.ColorsNeutral[index];
			}
			if (pawn.Faction == Faction.OfPlayer)
			{
				return PawnNameColorUtility.ColorColony;
			}
			if (pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				return PawnNameColorUtility.ColorsHostile[index];
			}
			return PawnNameColorUtility.ColorsNeutral[index];
		}
	}
}
