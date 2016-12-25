using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_MajorOrExtremeBreakRisk : Alert_Critical
	{
		public override string FullLabel
		{
			get
			{
				return BreakRiskAlertUtility.AlertLabel;
			}
		}

		public override string FullExplanation
		{
			get
			{
				return BreakRiskAlertUtility.AlertExplanation;
			}
		}

		public override AlertReport Report
		{
			get
			{
				Pawn pawn = BreakRiskAlertUtility.PawnsAtRiskExtreme.FirstOrDefault<Pawn>();
				if (pawn != null)
				{
					return pawn;
				}
				pawn = BreakRiskAlertUtility.PawnsAtRiskMajor.FirstOrDefault<Pawn>();
				if (pawn != null)
				{
					return pawn;
				}
				return false;
			}
		}
	}
}
