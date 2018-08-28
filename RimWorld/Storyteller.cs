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

		public static readonly Vector2 PortraitSizeTiny = new Vector2(116f, 124f);

		public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

		public const int IntervalsPerDay = 60;

		public const int CheckInterval = 1000;

		private static List<IIncidentTarget> tmpAllIncidentTargets = new List<IIncidentTarget>();

		private string debugStringCached = "Generating data...";

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
			this.InitializeStorytellerComps();
		}

		public static void StorytellerStaticUpdate()
		{
			Storyteller.tmpAllIncidentTargets.Clear();
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
			if (this.difficulty == null)
			{
				Log.Error("Loaded storyteller without difficulty", false);
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

		public bool TryFire(FiringIncident fi)
		{
			if (fi.def.Worker.CanFireNow(fi.parms, false) && fi.def.Worker.TryExecute(fi.parms))
			{
				fi.parms.target.StoryState.Notify_IncidentFired(fi);
				return true;
			}
			return false;
		}

		[DebuggerHidden]
		public IEnumerable<FiringIncident> MakeIncidentsForInterval()
		{
			List<IIncidentTarget> targets = this.AllIncidentTargets;
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				foreach (FiringIncident incident in this.MakeIncidentsForInterval(this.storytellerComps[i], targets))
				{
					yield return incident;
				}
			}
		}

		[DebuggerHidden]
		public IEnumerable<FiringIncident> MakeIncidentsForInterval(StorytellerComp comp, List<IIncidentTarget> targets)
		{
			if (GenDate.DaysPassedFloat > comp.props.minDaysPassed)
			{
				for (int i = 0; i < targets.Count; i++)
				{
					IIncidentTarget targ = targets[i];
					bool flag = false;
					bool flag2 = comp.props.allowedTargetTags.NullOrEmpty<IncidentTargetTagDef>();
					foreach (IncidentTargetTagDef current in targ.IncidentTargetTags())
					{
						if (!comp.props.disallowedTargetTags.NullOrEmpty<IncidentTargetTagDef>() && comp.props.disallowedTargetTags.Contains(current))
						{
							flag = true;
							break;
						}
						if (!flag2 && comp.props.allowedTargetTags.Contains(current))
						{
							flag2 = true;
						}
					}
					if (!flag && flag2)
					{
						foreach (FiringIncident fi in comp.MakeIntervalIncidents(targ))
						{
							if (Find.Storyteller.difficulty.allowBigThreats || (fi.def.category != IncidentCategoryDefOf.ThreatBig && fi.def.category != IncidentCategoryDefOf.RaidBeacon))
							{
								yield return fi;
							}
						}
					}
				}
			}
		}

		public void Notify_PawnEvent(Pawn pawn, AdaptationEvent ev, DamageInfo? dinfo = null)
		{
			Find.StoryWatcher.watcherAdaptation.Notify_PawnEvent(pawn, ev, dinfo);
			for (int i = 0; i < this.storytellerComps.Count; i++)
			{
				StorytellerComp storytellerComp = this.storytellerComps[i];
				storytellerComp.Notify_PawnEvent(pawn, ev, dinfo);
			}
		}

		public void Notify_DefChanged()
		{
			this.InitializeStorytellerComps();
		}

		public string DebugString()
		{
			if (Time.frameCount % 60 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Storyteller : " + this.def.label);
				stringBuilder.AppendLine("------------- Global threats data ---------------");
				stringBuilder.AppendLine("   Adaptation days: " + Find.StoryWatcher.watcherAdaptation.AdaptDays.ToString("F1"));
				stringBuilder.AppendLine("   Adapt points factor: " + Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor.ToString("F2"));
				stringBuilder.AppendLine("   Time points factor: " + Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassed).ToString("F2"));
				stringBuilder.AppendLine("   Num raids enemy: " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
				stringBuilder.AppendLine("   Ally incident fraction (neutral or ally): " + StorytellerUtility.AllyIncidentFraction(false).ToString("F2"));
				stringBuilder.AppendLine("   Ally incident fraction (ally only): " + StorytellerUtility.AllyIncidentFraction(true).ToString("F2"));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("-------------- Global population data --------------");
				stringBuilder.AppendLine(StorytellerUtilityPopulation.DebugReadout().TrimEndNewlines());
				stringBuilder.AppendLine("   Greatest population: " + Find.StoryWatcher.statsRecord.greatestPopulation);
				stringBuilder.AppendLine("------------- All incident targets --------------");
				for (int i = 0; i < this.AllIncidentTargets.Count; i++)
				{
					stringBuilder.AppendLine("   " + this.AllIncidentTargets[i].ToString());
				}
				IIncidentTarget incidentTarget = Find.WorldSelector.SingleSelectedObject as IIncidentTarget;
				if (incidentTarget == null)
				{
					incidentTarget = Find.CurrentMap;
				}
				if (incidentTarget != null)
				{
					Map map = incidentTarget as Map;
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("---------- Selected: " + incidentTarget + " --------");
					stringBuilder.AppendLine("   Wealth: " + incidentTarget.PlayerWealthForStoryteller.ToString("F0"));
					if (map != null)
					{
						stringBuilder.AppendLine(string.Concat(new string[]
						{
							"   (Items: ",
							map.wealthWatcher.WealthItems.ToString("F0"),
							" Buildings: ",
							map.wealthWatcher.WealthBuildings.ToString("F0"),
							" (Floors: ",
							map.wealthWatcher.WealthFloorsOnly.ToString("F0"),
							") Pawns: ",
							map.wealthWatcher.WealthPawns.ToString("F0"),
							")"
						}));
					}
					stringBuilder.AppendLine("   IncidentPointsRandomFactorRange: " + incidentTarget.IncidentPointsRandomFactorRange);
					stringBuilder.AppendLine("   Pawns-Humanlikes: " + (from p in incidentTarget.PlayerPawnsForStoryteller
					where p.def.race.Humanlike
					select p).Count<Pawn>());
					stringBuilder.AppendLine("   Pawns-Animals: " + (from p in incidentTarget.PlayerPawnsForStoryteller
					where p.def.race.Animal
					select p).Count<Pawn>());
					if (map != null)
					{
						stringBuilder.AppendLine("   StoryDanger: " + map.dangerWatcher.DangerRating);
						stringBuilder.AppendLine("   FireDanger: " + map.fireWatcher.FireDanger.ToString("F2"));
						stringBuilder.AppendLine("   LastThreatBigTick days ago: " + (Find.TickManager.TicksGame - map.storyState.LastThreatBigTick).ToStringTicksToDays("F1"));
					}
					stringBuilder.AppendLine("   Current points (ignoring early raid factors): " + StorytellerUtility.DefaultThreatPointsNow(incidentTarget).ToString("F0"));
					stringBuilder.AppendLine("   Current points for specific IncidentMakers:");
					for (int j = 0; j < this.storytellerComps.Count; j++)
					{
						IncidentParms incidentParms = this.storytellerComps[j].GenerateParms(IncidentCategoryDefOf.ThreatBig, incidentTarget);
						stringBuilder.AppendLine("      " + this.storytellerComps[j].GetType().ToString().Substring(23) + ": " + incidentParms.points.ToString("F0"));
					}
				}
				this.debugStringCached = stringBuilder.ToString();
			}
			return this.debugStringCached;
		}
	}
}
