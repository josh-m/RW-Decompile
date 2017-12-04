using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPawnGC : IExposable
	{
		private int lastSuccessfulGCTick;

		private int currentGCRate = 1;

		private const float PctOfHumanlikesAlwaysKept = 0.1f;

		private const float PctOfUnnamedColonyAnimalsAlwaysKept = 0.05f;

		private const int AdditionalStoryRelevantPawns = 20;

		private const int GCUpdateInterval = 15000;

		private IEnumerator activeGCProcess;

		private StringBuilder logDotgraph;

		private HashSet<string> logDotgraphUniqueLinks;

		public void WorldPawnGCTick()
		{
			if (this.lastSuccessfulGCTick < Find.TickManager.TicksGame / 15000 * 15000)
			{
				if (this.activeGCProcess == null)
				{
					this.activeGCProcess = this.PawnGCPass().GetEnumerator();
					if (DebugViewSettings.logWorldPawnGC)
					{
						Log.Message(string.Format("World pawn GC started at rate {0}", this.currentGCRate));
					}
				}
				if (this.activeGCProcess != null)
				{
					bool flag = false;
					int num = 0;
					while (num < this.currentGCRate && !flag)
					{
						flag = !this.activeGCProcess.MoveNext();
						num++;
					}
					if (flag)
					{
						this.lastSuccessfulGCTick = Find.TickManager.TicksGame;
						this.currentGCRate = 1;
						this.activeGCProcess = null;
						if (DebugViewSettings.logWorldPawnGC)
						{
							Log.Message("World pawn GC complete");
						}
					}
				}
			}
		}

		public void CancelGCPass()
		{
			if (this.activeGCProcess != null)
			{
				this.activeGCProcess = null;
				this.currentGCRate = Mathf.Min(this.currentGCRate * 2, 16777216);
				if (DebugViewSettings.logWorldPawnGC)
				{
					Log.Message("World pawn GC cancelled");
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable AccumulatePawnGCData(Dictionary<Pawn, string> keptPawns)
		{
			foreach (Pawn current in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				string criticalPawnReason = this.GetCriticalPawnReason(current);
				if (!criticalPawnReason.NullOrEmpty())
				{
					keptPawns[current] = criticalPawnReason;
					if (this.logDotgraph != null)
					{
						this.logDotgraph.AppendLine(string.Format("{0} [label=<{0}<br/><font point-size=\"10\">{1}</font>> color=\"{2}\" shape=\"{3}\"];", new object[]
						{
							WorldPawnGC.DotgraphIdentifier(current),
							criticalPawnReason,
							(current.relations == null || !current.relations.everSeenByPlayer) ? "grey" : "black",
							(!current.RaceProps.Humanlike) ? "box" : "oval"
						}));
					}
				}
				else if (this.logDotgraph != null)
				{
					this.logDotgraph.AppendLine(string.Format("{0} [color=\"{1}\" shape=\"{2}\"];", WorldPawnGC.DotgraphIdentifier(current), (current.relations == null || !current.relations.everSeenByPlayer) ? "grey" : "black", (!current.RaceProps.Humanlike) ? "box" : "oval"));
				}
			}
			foreach (Pawn current2 in (from pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive
			where this.$this.AllowedAsStoryPawn(pawn) && !keptPawns.ContainsKey(pawn)
			orderby pawn.records.StoryRelevance descending
			select pawn).Take(20))
			{
				keptPawns[current2] = "StoryRelevant";
			}
			Pawn[] criticalPawns = keptPawns.Keys.ToArray<Pawn>();
			Pawn[] array = criticalPawns;
			for (int i = 0; i < array.Length; i++)
			{
				Pawn pawn3 = array[i];
				this.AddAllRelationships(pawn3, keptPawns);
				yield return null;
			}
			Pawn[] array2 = criticalPawns;
			for (int j = 0; j < array2.Length; j++)
			{
				Pawn pawn2 = array2[j];
				this.AddAllMemories(pawn2, keptPawns);
			}
		}

		private Dictionary<Pawn, string> AccumulatePawnGCDataImmediate()
		{
			Dictionary<Pawn, string> dictionary = new Dictionary<Pawn, string>();
			this.AccumulatePawnGCData(dictionary).ExecuteEnumerable();
			return dictionary;
		}

		public string PawnGCDebugResults()
		{
			Dictionary<Pawn, string> dictionary = this.AccumulatePawnGCDataImmediate();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			foreach (Pawn current in Find.WorldPawns.AllPawnsAlive)
			{
				string text = "Discarded";
				if (dictionary.ContainsKey(current))
				{
					text = dictionary[current];
				}
				if (!dictionary2.ContainsKey(text))
				{
					dictionary2[text] = 0;
				}
				Dictionary<string, int> dictionary3;
				string key;
				(dictionary3 = dictionary2)[key = text] = dictionary3[key] + 1;
			}
			return GenText.ToTextList(from kvp in dictionary2
			orderby kvp.Value descending
			select string.Format("{0}: {1}", kvp.Value, kvp.Key), "\n");
		}

		[DebuggerHidden]
		public IEnumerable PawnGCPass()
		{
			Dictionary<Pawn, string> keptPawns = new Dictionary<Pawn, string>();
			Pawn[] worldPawnsSnapshot = Find.WorldPawns.AllPawnsAliveOrDead.ToArray<Pawn>();
			foreach (object _ in this.AccumulatePawnGCData(keptPawns))
			{
				yield return null;
			}
			for (int i = 0; i < worldPawnsSnapshot.Length; i++)
			{
				Pawn pawn = worldPawnsSnapshot[i];
				if (pawn.IsWorldPawn() && !keptPawns.ContainsKey(pawn))
				{
					Find.WorldPawns.RemoveAndDiscardPawnViaGC(pawn);
				}
			}
		}

		private string GetCriticalPawnReason(Pawn pawn)
		{
			if (pawn.Discarded)
			{
				return null;
			}
			if (PawnUtility.EverBeenColonistOrTameAnimal(pawn) && pawn.RaceProps.Humanlike)
			{
				return "Colonist";
			}
			if (PawnGenerator.IsBeingGenerated(pawn))
			{
				return "Generating";
			}
			if (PawnUtility.IsFactionLeader(pawn))
			{
				return "FactionLeader";
			}
			if (PawnUtility.IsKidnappedPawn(pawn))
			{
				return "Kidnapped";
			}
			if (pawn.IsCaravanMember())
			{
				return "CaravanMember";
			}
			if (PawnUtility.IsTravelingInTransportPodWorldObject(pawn))
			{
				return "TransportPod";
			}
			if (PawnUtility.ForSaleBySettlement(pawn))
			{
				return "ForSale";
			}
			if (Find.WorldPawns.ForcefullyKeptPawns.Contains(pawn))
			{
				return "ForceKept";
			}
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				return "Spawned";
			}
			if (!pawn.Corpse.DestroyedOrNull())
			{
				return "CorpseExists";
			}
			if (pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing)
			{
				if (Find.PlayLog.AnyEntryConcerns(pawn))
				{
					return "InPlayLog";
				}
				if (Find.BattleLog.AnyEntryConcerns(pawn))
				{
					return "InBattleLog";
				}
			}
			if (Current.ProgramState == ProgramState.Playing && Find.TaleManager.AnyActiveTaleConcerns(pawn))
			{
				return "InActiveTale";
			}
			return null;
		}

		private bool AllowedAsStoryPawn(Pawn pawn)
		{
			return pawn.RaceProps.Humanlike;
		}

		public void AddAllRelationships(Pawn pawn, Dictionary<Pawn, string> keptPawns)
		{
			if (pawn.relations == null)
			{
				return;
			}
			foreach (Pawn current in pawn.relations.RelatedPawns)
			{
				if (this.logDotgraph != null)
				{
					string text = string.Format("{0}->{1} [label=<{2}> color=\"purple\"];", WorldPawnGC.DotgraphIdentifier(pawn), WorldPawnGC.DotgraphIdentifier(current), pawn.GetRelations(current).FirstOrDefault<PawnRelationDef>().ToString());
					if (!this.logDotgraphUniqueLinks.Contains(text))
					{
						this.logDotgraphUniqueLinks.Add(text);
						this.logDotgraph.AppendLine(text);
					}
				}
				if (!keptPawns.ContainsKey(current))
				{
					keptPawns[current] = "Relationship";
				}
			}
		}

		public void AddAllMemories(Pawn pawn, Dictionary<Pawn, string> keptPawns)
		{
			if (pawn.needs == null || pawn.needs.mood == null || pawn.needs.mood.thoughts == null || pawn.needs.mood.thoughts.memories == null)
			{
				return;
			}
			foreach (Thought_Memory current in pawn.needs.mood.thoughts.memories.Memories)
			{
				if (current.otherPawn != null)
				{
					if (this.logDotgraph != null)
					{
						string text = string.Format("{0}->{1} [label=<{2}> color=\"orange\"];", WorldPawnGC.DotgraphIdentifier(pawn), WorldPawnGC.DotgraphIdentifier(current.otherPawn), current.def);
						if (!this.logDotgraphUniqueLinks.Contains(text))
						{
							this.logDotgraphUniqueLinks.Add(text);
							this.logDotgraph.AppendLine(text);
						}
					}
					if (!keptPawns.ContainsKey(current.otherPawn))
					{
						keptPawns[current.otherPawn] = "Memory";
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.lastSuccessfulGCTick, "lastSuccessfulGCTick", 0, false);
			Scribe_Values.Look<int>(ref this.currentGCRate, "nextGCRate", 1, false);
		}

		public void LogGC()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= GC =======");
			stringBuilder.AppendLine(this.PawnGCDebugResults());
			Log.Message(stringBuilder.ToString());
		}

		public void RunGC()
		{
			this.CancelGCPass();
			PerfLogger.Reset();
			foreach (object current in this.PawnGCPass())
			{
			}
			float num = PerfLogger.Duration() * 1000f;
			PerfLogger.Flush();
			Log.Message(string.Format("World pawn GC run complete in {0} ms", num));
		}

		public void LogDotgraph()
		{
			this.logDotgraph = new StringBuilder();
			this.logDotgraphUniqueLinks = new HashSet<string>();
			this.logDotgraph.AppendLine("digraph { rankdir=LR;");
			this.AccumulatePawnGCDataImmediate();
			this.logDotgraph.AppendLine("}");
			GUIUtility.systemCopyBuffer = this.logDotgraph.ToString();
			Log.Message("Dotgraph copied to clipboard");
			this.logDotgraph = null;
			this.logDotgraphUniqueLinks = null;
		}

		public static string DotgraphIdentifier(Pawn pawn)
		{
			return new string(pawn.NameStringShort.Where(new Func<char, bool>(char.IsLetter)).ToArray<char>()) + "_" + pawn.thingIDNumber.ToString();
		}
	}
}
