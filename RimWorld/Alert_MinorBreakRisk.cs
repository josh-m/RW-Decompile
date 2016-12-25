using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_MinorBreakRisk : Alert
	{
		public Alert_MinorBreakRisk()
		{
			this.defaultPriority = AlertPriority.High;
		}

		public override string GetLabel()
		{
			return BreakRiskAlertUtility.AlertLabel;
		}

		public override string GetExplanation()
		{
			return BreakRiskAlertUtility.AlertExplanation;
		}

		public override AlertReport GetReport()
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
