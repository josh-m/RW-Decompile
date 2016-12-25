using RimWorld.Planet;
using System;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class StorytellerUtility
	{
		private const float WealthBase = 2000f;

		private const float PointsPer1000Wealth = 11f;

		private const float PointsPerColonist = 40f;

		private const float MinMaxSquadCost = 50f;

		private const float BuildingWealthFactor = 0.5f;

		private const float HalveLimitLo = 1000f;

		private const float HalveLimitHi = 2000f;

		public static IncidentParms DefaultParmsNow(StorytellerDef tellerDef, IncidentCategory incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = target;
			if (incCat == IncidentCategory.ThreatSmall || incCat == IncidentCategory.ThreatBig)
			{
				Map map = target as Map;
				float num = 0f;
				if (map != null)
				{
					num = map.wealthWatcher.WealthItems + map.wealthWatcher.WealthBuildings * 0.5f;
				}
				num -= 2000f;
				if (num < 0f)
				{
					num = 0f;
				}
				float num2 = num / 1000f * 11f;
				float num3 = 0f;
				if (map != null)
				{
					num3 = (float)map.mapPawns.FreeColonistsCount * 40f;
				}
				else
				{
					Caravan caravan = target as Caravan;
					if (caravan != null)
					{
						num3 = (float)caravan.PawnsListForReading.Count((Pawn x) => x.IsColonist && x.HostFaction == null) * 40f;
					}
				}
				incidentParms.points = num2 + num3;
				incidentParms.points *= Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor;
				incidentParms.points *= Find.Storyteller.difficulty.threatScale;
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

		public static void DebugLogTestFutureIncidents()
		{
			int ticksGame = Find.TickManager.TicksGame;
			StoryState storyState = Find.StoryWatcher.storyState;
			IncidentQueue incidentQueue = Find.Storyteller.incidentQueue;
			Find.StoryWatcher.storyState = new StoryState();
			Find.Storyteller.incidentQueue = new IncidentQueue();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Test future incidents:");
			int[] array = new int[Find.Storyteller.storytellerComps.Count];
			for (int i = 0; i < 6000; i++)
			{
				foreach (FiringIncident current in Find.Storyteller.MakeIncidentsForInterval())
				{
					int num = Find.Storyteller.storytellerComps.IndexOf(current.source);
					array[num]++;
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"M",
						num,
						"  ",
						Find.TickManager.TicksGame.TicksToDays().ToString("F1"),
						"d    ",
						current
					}));
					Find.StoryWatcher.storyState.Notify_IncidentFired(current);
				}
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1000);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident totals:");
			for (int j = 0; j < array.Length; j++)
			{
				float f = (float)array[j] / (float)array.Sum();
				float num2 = (float)array[j] / 100f;
				float num3 = 1f / num2;
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"   M",
					j,
					": ",
					array[j],
					"  (",
					f.ToStringPercent("F2"),
					" of total, avg ",
					num2.ToString("F2"),
					" per day, avg interval ",
					num3,
					")"
				}));
			}
			stringBuilder.AppendLine("Overall: " + array.Sum());
			stringBuilder.AppendLine("Overall avg per day: " + ((float)array.Sum() / 100f).ToString("F2"));
			Log.Message(stringBuilder.ToString());
			Find.TickManager.DebugSetTicksGame(ticksGame);
			Find.StoryWatcher.storyState = storyState;
			Find.Storyteller.incidentQueue = incidentQueue;
		}
	}
}
