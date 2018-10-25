using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Boredom : Alert
	{
		private const float JoyNeedThreshold = 0.24000001f;

		public Alert_Boredom()
		{
			this.defaultLabel = "Boredom".Translate();
			this.defaultPriority = AlertPriority.Medium;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(this.BoredPawns());
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Pawn pawn = null;
			foreach (Pawn current in this.BoredPawns())
			{
				stringBuilder.AppendLine("   " + current.Label);
				if (pawn == null)
				{
					pawn = current;
				}
			}
			string value = JoyUtility.JoyKindsOnMapString(pawn.Map);
			return "BoredomDesc".Translate(stringBuilder.ToString().TrimEndNewlines(), pawn.LabelShort, value, pawn.Named("PAWN"));
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> BoredPawns()
		{
			foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if ((p.needs.joy.CurLevelPercentage < 0.24000001f || p.GetTimeAssignment() == TimeAssignmentDefOf.Joy) && p.needs.joy.tolerances.BoredOfAllAvailableJoyKinds(p))
				{
					yield return p;
				}
			}
		}
	}
}
