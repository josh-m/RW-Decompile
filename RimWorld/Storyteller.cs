using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Storyteller : IExposable
	{
		public StorytellerDef def;

		public DifficultyDef difficulty;

		public List<StorytellerComp> storytellerComps;

		public IncidentQueue incidentQueue = new IncidentQueue();

		public StoryIntender_Population intenderPopulation;

		public static readonly Vector2 PortraitSizeTiny = new Vector2(116f, 124f);

		public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

		public const int IntervalsPerDay = 60;

		public const int CheckInterval = 1000;

		private static List<IIncidentTarget> tmpAllIncidentTargets = new List<IIncidentTarget>();

		public List<IIncidentTarget> AllIncidentTargets
		{
			get
			{
				Storyteller.tmpAllIncidentTargets.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Storyteller.tmpAllIncidentTargets.Add(maps[i]);
				}
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled)
					{
						Storyteller.tmpAllIncidentTargets.Add(caravans[j]);
					}
				}
				Storyteller.tmpAllIncidentTargets.Add(Find.World);
				return Storyteller.tmpAllIncidentTargets;
			}
		}

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
			Scribe_Defs.Look<StorytellerDef>(ref this.def, "def");
			Scribe_Defs.Look<DifficultyDef>(ref this.difficulty, "difficulty");
			Scribe_Deep.Look<IncidentQueue>(ref this.incidentQueue, "incidentQueue", new object[0]);
			Scribe_Deep.Look<StoryIntender_Population>(ref this.intenderPopulation, "intenderPopulation", new object[]
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
			if ((fi.parms.forced || fi.def.Worker.CanFireNow(fi.parms.target)) && fi.def.Worker.TryExecute(fi.parms))
			{
				fi.parms.target.StoryState.Notify_IncidentFired(fi);
			}
		}

		[DebuggerHidden]
		public IEnumerable<FiringIncident> MakeIncidentsForInterval()
		{
			List<IIncidentTarget> targets = this.AllIncidentTargets;
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				StorytellerComp c = this.storytellerComps[i];
				if (GenDate.DaysPassedFloat > c.props.minDaysPassed)
				{
					for (int j = 0; j < targets.Count; j++)
					{
						IIncidentTarget targ = targets[j];
						if (c.props.allowedTargetTypes == null || c.props.allowedTargetTypes.Count == 0 || c.props.allowedTargetTypes.Intersect(targ.AcceptedTypes()).Any<IncidentTargetTypeDef>())
						{
							foreach (FiringIncident fi in c.MakeIntervalIncidents(targ))
							{
								if (Find.Storyteller.difficulty.allowBigThreats || (fi.def.category != IncidentCategory.ThreatBig && fi.def.category != IncidentCategory.RaidBeacon))
								{
									yield return fi;
								}
							}
						}
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
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(this.intenderPopulation.DebugReadout);
			stringBuilder.AppendLine("Global stats:");
			stringBuilder.AppendLine("   numRaidsEnemy: " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
			stringBuilder.AppendLine("   TotalThreatFactor: " + Find.StoryWatcher.watcherRampUp.TotalThreatPointsFactor.ToString("F5"));
			stringBuilder.AppendLine("      ShortFactor: " + Find.StoryWatcher.watcherRampUp.ShortTermFactor.ToString("F5"));
			stringBuilder.AppendLine("      LongFactor: " + Find.StoryWatcher.watcherRampUp.LongTermFactor.ToString("F5"));
			stringBuilder.AppendLine("   Current default ThreatBig parms points:");
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				IncidentParms incidentParms = this.storytellerComps[i].GenerateParms(IncidentCategory.ThreatBig, Find.VisibleMap);
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"      ",
					this.storytellerComps[i].GetType(),
					": ",
					incidentParms.points
				}));
			}
			if (Find.VisibleMap != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("VisibleMap stats:");
				stringBuilder.AppendLine("   Wealth: " + Find.VisibleMap.wealthWatcher.WealthTotal);
				stringBuilder.AppendLine("   DaysSinceSeriousDamage: " + Find.VisibleMap.damageWatcher.DaysSinceSeriousDamage.ToString("F1"));
				stringBuilder.AppendLine("   LastThreatBigQueueTick: " + Find.VisibleMap.storyState.LastThreatBigTick.ToStringTicksToPeriod(true, false, true));
				stringBuilder.AppendLine("   FireDanger: " + Find.VisibleMap.fireWatcher.FireDanger.ToString("F2"));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident targets:");
			for (int j = 0; j < this.AllIncidentTargets.Count; j++)
			{
				stringBuilder.AppendLine("   " + this.AllIncidentTargets[j].ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
