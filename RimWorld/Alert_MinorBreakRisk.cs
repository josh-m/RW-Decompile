using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_MinorBreakRisk : Alert_High
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
				if (BreakRiskAlertUtility.PawnsAtRiskExtreme.Any<Pawn>() || BreakRiskAlertUtility.PawnsAtRiskMajor.Any<Pawn>())
				{
					return false;
				}
				Pawn pawn = BreakRiskAlertUtility.PawnsAtRiskMinor.FirstOrDefault<Pawn>();
				if (pawn != null)
				{
					return pawn;
				}
				return false;
			}
		}
	}
}
