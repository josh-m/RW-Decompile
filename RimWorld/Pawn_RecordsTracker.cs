using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_RecordsTracker : IExposable
	{
		public Pawn pawn;

		private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();

		private double storyRelevance;

		private Battle battleActive;

		private int battleExitTick;

		private float storyRelevanceBonus;

		private const int UpdateTimeRecordsIntervalTicks = 80;

		private const float StoryRelevanceBonusRange = 100f;

		private const float StoryRelevanceMultiplierPerYear = 0.2f;

		public float StoryRelevance
		{
			get
			{
				return (float)this.storyRelevance + this.storyRelevanceBonus;
			}
		}

		public Battle BattleActive
		{
			get
			{
				return (this.battleExitTick < Find.TickManager.TicksGame) ? null : this.battleActive;
			}
		}

		public Pawn_RecordsTracker(Pawn pawn)
		{
			this.pawn = pawn;
			Rand.PushState();
			Rand.Seed = pawn.thingIDNumber * 681;
			this.storyRelevanceBonus = Rand.Range(0f, 100f);
			Rand.PopState();
		}

		public void RecordsTick()
		{
			if (this.pawn.Dead)
			{
				return;
			}
			if (this.pawn.IsHashIntervalTick(80))
			{
				this.RecordsTickUpdate(80);
				this.battleActive = this.BattleActive;
			}
		}

		public void RecordsTickMothballed(int interval)
		{
			this.RecordsTickUpdate(interval);
		}

		private void RecordsTickUpdate(int interval)
		{
			List<RecordDef> allDefsListForReading = DefDatabase<RecordDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].type == RecordType.Time && allDefsListForReading[i].Worker.ShouldMeasureTimeNow(this.pawn))
				{
					DefMap<RecordDef, float> defMap;
					RecordDef def;
					(defMap = this.records)[def = allDefsListForReading[i]] = defMap[def] + (float)interval;
				}
			}
			this.storyRelevance *= Math.Pow(0.20000000298023224, (double)(0 * interval));
		}

		public void Increment(RecordDef def)
		{
			if (def.type != RecordType.Int)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to increment record \"",
					def.defName,
					"\" whose record type is \"",
					def.type,
					"\"."
				}));
				return;
			}
			this.records[def] = Mathf.Round(this.records[def] + 1f);
		}

		public void AddTo(RecordDef def, float value)
		{
			if (def.type == RecordType.Int)
			{
				this.records[def] = Mathf.Round(this.records[def] + Mathf.Round(value));
			}
			else
			{
				if (def.type != RecordType.Float)
				{
					Log.Error(string.Concat(new object[]
					{
						"Tried to add value to record \"",
						def.defName,
						"\" whose record type is \"",
						def.type,
						"\"."
					}));
					return;
				}
				DefMap<RecordDef, float> defMap;
				(defMap = this.records)[def] = defMap[def] + value;
			}
		}

		public float GetValue(RecordDef def)
		{
			float num = this.records[def];
			if (def.type == RecordType.Int || def.type == RecordType.Time)
			{
				return Mathf.Round(num);
			}
			return num;
		}

		public int GetAsInt(RecordDef def)
		{
			return Mathf.RoundToInt(this.records[def]);
		}

		public void AccumulateStoryEvent(StoryEventDef def)
		{
			this.storyRelevance += (double)def.importance;
		}

		public void EnterBattle(Battle battle)
		{
			this.battleActive = battle;
			this.battleExitTick = Find.TickManager.TicksGame + 5000;
			Log.Message(string.Format("Pawn {0} entering battle {1}", this.pawn, battle));
		}

		public void ExposeData()
		{
			this.battleActive = this.BattleActive;
			Scribe_Deep.Look<DefMap<RecordDef, float>>(ref this.records, "records", new object[0]);
			Scribe_Values.Look<double>(ref this.storyRelevance, "storyRelevance", 0.0, false);
			Scribe_References.Look<Battle>(ref this.battleActive, "battleActive", false);
			Scribe_Values.Look<int>(ref this.battleExitTick, "battleExitTick", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.RecordsTrackerPostLoadInit(this);
			}
		}
	}
}
