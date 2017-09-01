using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_Pregnant : PawnColumnWorker_Icon
	{
		private static readonly Texture2D Icon = Resources.Load<Texture2D>("Textures/UI/Icons/Pregnant");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			Hediff_Pregnant pregnantHediff = this.GetPregnantHediff(pawn);
			return (pregnantHediff == null) ? null : PawnColumnWorker_Pregnant.Icon;
		}

		protected override string GetIconTip(Pawn pawn)
		{
			Hediff_Pregnant pregnantHediff = this.GetPregnantHediff(pawn);
			float gestationProgress = pregnantHediff.GestationProgress;
			int num = (int)(pawn.RaceProps.gestationPeriodDays * 60000f);
			int numTicks = (int)(gestationProgress * (float)num);
			return "PregnantIconDesc".Translate(new object[]
			{
				numTicks.ToStringTicksToDays("F0"),
				num.ToStringTicksToDays("F0")
			});
		}

		private Hediff_Pregnant GetPregnantHediff(Pawn pawn)
		{
			return (Hediff_Pregnant)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant, true);
		}
	}
}
