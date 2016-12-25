using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Storyteller : IExposable
	{
		public const int IntervalsPerDay = 60;

		public const int CheckInterval = 1000;

		public StorytellerDef def;

		public DifficultyDef difficulty;

		public List<StorytellerComp> storytellerComps;

		public WeatherDecider weatherDecider = new WeatherDecider();

		public IncidentQueue incidentQueue = new IncidentQueue();

		public StoryIntender_Population intenderPopulation;

		public static readonly Vector2 PortraitSizeTiny = new Vector2(116f, 124f);

		public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

		public Storyteller()
		{
		}

		public Storyteller(StorytellerDef def, DifficultyDef difficulty)
		{
			this.def = def;
			this.difficulty = difficulty;
			this.intenderPopulation = new StoryIntender_Population(this);
			this.InitializeStorytellerComps();
		}

		private void InitializeStorytellerComps()
		{
			this.storytellerComps = new List<StorytellerComp>();
			for (int i = 0; i < this.def.comps.Count; i++)
			{
				StorytellerComp storytellerComp = (StorytellerComp)Activator.CreateInstance(this.def.comps[i].compClass);
				storytellerComp.props = this.def.comps[i];
				this.storytellerComps.Add(storytellerComp);
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<StorytellerDef>(ref this.def, "def");
			Scribe_Defs.LookDef<DifficultyDef>(ref this.difficulty, "difficulty");
			Scribe_Deep.LookDeep<WeatherDecider>(ref this.weatherDecider, "weatherDecider", new object[0]);
			Scribe_Deep.LookDeep<IncidentQueue>(ref this.incidentQueue, "incidentQueue", new object[0]);
			Scribe_Deep.LookDeep<StoryIntender_Population>(ref this.intenderPopulation, "intenderPopulation", new object[]
			{
				this
			});
			if (this.difficulty == null)
			{
				Log.Error("Loaded storyteller without difficulty");
				this.difficulty = DefDatabase<DifficultyDef>.AllDefsListForReading[3];
			}
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				this.InitializeStorytellerComps();
			}
		}

		public void StorytellerTick()
		{
			this.weatherDecider.WeatherDeciderTick();
			this.incidentQueue.IncidentQueueTick();
			if (Find.TickManager.TicksGame % 1000 == 0)
			{
				if (!DebugSettings.enableStoryteller)
				{
					return;
				}
				foreach (FiringIncident current in this.MakeIncidentsForInterval())
				{
					this.TryFire(current);
				}
			}
		}

		public void TryFire(FiringIncident fi)
		{
			if (fi.def.Worker.CanFireNow())
			{
				fi.def.Worker.TryExecute(fi.parms);
				Find.StoryWatcher.storyState.Notify_IncidentFired(fi);
			}
		}

		[DebuggerHidden]
		public IEnumerable<FiringIncident> MakeIncidentsForInterval()
		{
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				StorytellerComp c = this.storytellerComps[i];
				if (GenDate.DaysPassedFloat > c.props.minDaysPassed)
				{
					foreach (FiringIncident fi in this.storytellerComps[i].MakeIntervalIncidents())
					{
						yield return fi;
					}
				}
			}
		}

		public void Notify_DefChanged()
		{
			this.InitializeStorytellerComps();
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Storyteller : " + this.def.label);
			stringBuilder.AppendLine(this.intenderPopulation.DebugReadout);
			stringBuilder.AppendLine("Wealth: " + Find.StoryWatcher.watcherWealth.WealthTotal);
			stringBuilder.AppendLine("DaysSinceSeriousDamage: " + Find.StoryWatcher.watcherDamage.DaysSinceSeriousDamage);
			stringBuilder.AppendLine("numRaidsEnemy: " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
			stringBuilder.AppendLine("LastThreatBigQueueTick: " + Find.StoryWatcher.storyState.LastThreatBigTick.ToStringTicksToPeriod(true));
			stringBuilder.AppendLine("TotalThreatFactor: " + Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor.ToString("F5"));
			stringBuilder.AppendLine("   ShortFactor: " + Find.StoryWatcher.watcherRampUp.ShortTermFactor.ToString("F5"));
			stringBuilder.AppendLine("   LongFactor: " + Find.StoryWatcher.watcherRampUp.LongTermFactor.ToString("F5"));
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				IncidentParms incidentParms = this.storytellerComps[i].GenerateParms(IncidentCategory.ThreatBig);
				stringBuilder.AppendLine("Current default threat params (" + this.storytellerComps[i].GetType() + "):");
				stringBuilder.AppendLine("    ThreatBig points: " + incidentParms.points);
			}
			return stringBuilder.ToString();
		}
	}
}
