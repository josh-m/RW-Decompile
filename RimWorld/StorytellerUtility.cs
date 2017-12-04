using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class StorytellerUtility
	{
		private const float WealthBase = 2000f;

		private const float PointsPer1000Wealth = 10f;

		private const float PointsPerColonist = 42f;

		private const float PointsPerColonistFactorInContainer = 0.3f;

		private const float PointsPerColonistHealthSummaryLerpAmount = 0.5f;

		private const float MinMaxSquadCost = 50f;

		public const float BuildingWealthFactor = 0.5f;

		public const float CaravanWealthFactor = 0.5f;

		private const float HalveLimitLo = 1000f;

		private const float HalveLimitHi = 2000f;

		private static Dictionary<IIncidentTarget, StoryState> tmpOldStoryStates = new Dictionary<IIncidentTarget, StoryState>();

		public static IncidentParms DefaultParmsNow(StorytellerDef tellerDef, IncidentCategory incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = target;
			if (incCat == IncidentCategory.ThreatSmall || incCat == IncidentCategory.ThreatBig || incCat == IncidentCategory.RaidBeacon)
			{
				float num = target.PlayerWealthForStoryteller;
				num -= 2000f;
				num = Mathf.Max(num, 0f);
				float num2 = num / 1000f * 10f;
				float num3 = 0f;
				foreach (Pawn current in target.FreeColonistsForStoryteller)
				{
					float num4 = 1f;
					if (current.ParentHolder != null && current.ParentHolder is Building_CryptosleepCasket)
					{
						num4 *= 0.3f;
					}
					num4 = Mathf.Lerp(num4, num4 * current.health.summaryHealth.SummaryHealthPercent, 0.5f);
					num3 += 42f * num4;
				}
				incidentParms.points = num2 + num3;
				incidentParms.points *= Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor;
				incidentParms.points *= Find.Storyteller.difficulty.threatScale;
				incidentParms.points *= target.IncidentPointsRandomFactorRange.RandomInRange;
				switch (Find.StoryWatcher.statsRecord.numThreatBigs)
				{
				case 0:
					incidentParms.points = 35f;
					incidentParms.raidForceOneIncap = true;
					incidentParms.raidNeverFleeIndividual = true;
					break;
				case 1:
					incidentParms.points *= 0.5f;
					break;
				case 2:
					incidentParms.points *= 0.7f;
					break;
				case 3:
					incidentParms.points *= 0.8f;
					break;
				case 4:
					incidentParms.points *= 0.9f;
					break;
				default:
					incidentParms.points *= 1f;
					break;
				}
				if (incidentParms.points < 0f)
				{
					incidentParms.points = 0f;
				}
				if (incidentParms.points > 1000f)
				{
					if (incidentParms.points > 2000f)
					{
						incidentParms.points = 2000f + (incidentParms.points - 2000f) * 0.5f;
					}
					incidentParms.points = 1000f + (incidentParms.points - 1000f) * 0.5f;
				}
			}
			return incidentParms;
		}

		public static float AllyIncidentMTBMultiplier()
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (!allFactionsListForReading[i].def.hidden && !allFactionsListForReading[i].IsPlayer)
				{
					if (allFactionsListForReading[i].def.CanEverBeNonHostile)
					{
						num2++;
					}
					if (!allFactionsListForReading[i].HostileTo(Faction.OfPlayer))
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				return -1f;
			}
			float num3 = (float)num / Mathf.Max((float)num2, 1f);
			return 1f / num3;
		}

		public static void DebugLogTestFutureIncidents(bool visibleMapOnly)
		{
			int ticksGame = Find.TickManager.TicksGame;
			IncidentQueue incidentQueue = Find.Storyteller.incidentQueue;
			List<IIncidentTarget> allIncidentTargets = Find.Storyteller.AllIncidentTargets;
			StorytellerUtility.tmpOldStoryStates.Clear();
			for (int i = 0; i < allIncidentTargets.Count; i++)
			{
				IIncidentTarget incidentTarget = allIncidentTargets[i];
				StorytellerUtility.tmpOldStoryStates.Add(incidentTarget, incidentTarget.StoryState);
				new StoryState(incidentTarget).CopyTo(incidentTarget.StoryState);
			}
			Find.Storyteller.incidentQueue = new IncidentQueue();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Test future incidents for " + Find.Storyteller.def + ":");
			int[] array = new int[Find.Storyteller.storytellerComps.Count];
			Dictionary<IIncidentTarget, int> dictionary = new Dictionary<IIncidentTarget, int>();
			int num = 0;
			for (int j = 0; j < 6000; j++)
			{
				foreach (FiringIncident current in Find.Storyteller.MakeIncidentsForInterval())
				{
					if (!visibleMapOnly || current.parms.target == Find.VisibleMap)
					{
						if (!dictionary.ContainsKey(current.parms.target))
						{
							dictionary[current.parms.target] = 0;
						}
						Dictionary<IIncidentTarget, int> dictionary2;
						IIncidentTarget target;
						(dictionary2 = dictionary)[target = current.parms.target] = dictionary2[target] + 1;
						string text = "  ";
						if (current.def.category == IncidentCategory.ThreatBig || current.def.category == IncidentCategory.RaidBeacon)
						{
							num++;
							text = "T";
						}
						int num2 = Find.Storyteller.storytellerComps.IndexOf(current.source);
						array[num2]++;
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							"M",
							num2,
							" ",
							text,
							" ",
							Find.TickManager.TicksGame.TicksToDays().ToString("F1"),
							"d      ",
							current
						}));
						current.parms.target.StoryState.Notify_IncidentFired(current);
					}
				}
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1000);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Target totals:");
			foreach (KeyValuePair<IIncidentTarget, int> current2 in from kvp in dictionary
			orderby kvp.Value
			select kvp)
			{
				stringBuilder.AppendLine(string.Format("  {0}: {1}", current2.Value, current2.Key));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident totals:");
			for (int k = 0; k < array.Length; k++)
			{
				float f = (float)array[k] / (float)array.Sum();
				float num3 = (float)array[k] / 100f;
				float num4 = 1f / num3;
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"   M",
					k,
					": ",
					array[k],
					"  (",
					f.ToStringPercent("F2"),
					" of total, avg ",
					num3.ToString("F2"),
					" per day, avg interval ",
					num4,
					")"
				}));
			}
			stringBuilder.AppendLine("Total threats: " + num);
			stringBuilder.AppendLine("Total threats avg per day: " + ((float)num / 100f).ToString("F2"));
			stringBuilder.AppendLine("Overall: " + array.Sum());
			stringBuilder.AppendLine("Overall avg per day: " + ((float)array.Sum() / 100f).ToString("F2"));
			Log.Message(stringBuilder.ToString());
			Find.TickManager.DebugSetTicksGame(ticksGame);
			Find.Storyteller.incidentQueue = incidentQueue;
			for (int l = 0; l < allIncidentTargets.Count; l++)
			{
				StorytellerUtility.tmpOldStoryStates[allIncidentTargets[l]].CopyTo(allIncidentTargets[l].StoryState);
			}
			StorytellerUtility.tmpOldStoryStates.Clear();
		}

		public static void DebugLogTestIncidentTargets()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Available incident targets:\n");
			foreach (IIncidentTarget current in Find.Storyteller.AllIncidentTargets)
			{
				stringBuilder.AppendLine(current.ToString());
				foreach (IncidentTargetTypeDef current2 in current.AcceptedTypes())
				{
					stringBuilder.AppendLine("  " + current2);
				}
				stringBuilder.AppendLine(string.Empty);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
