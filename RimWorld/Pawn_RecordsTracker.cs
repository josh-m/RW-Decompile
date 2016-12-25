using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_RecordsTracker : IExposable
	{
		private const int UpdateTimeRecordsIntervalTicks = 80;

		private Pawn pawn;

		private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();

		public Pawn_RecordsTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void RecordsTick()
		{
			if (this.pawn.Dead)
			{
				return;
			}
			if (this.pawn.IsHashIntervalTick(80))
			{
				List<RecordDef> allDefsListForReading = DefDatabase<RecordDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					if (allDefsListForReading[i].type == RecordType.Time && allDefsListForReading[i].Worker.ShouldMeasureTimeNow(this.pawn))
					{
						DefMap<RecordDef, float> defMap;
						DefMap<RecordDef, float> expr_63 = defMap = this.records;
						RecordDef def;
						RecordDef expr_6C = def = allDefsListForReading[i];
						float num = defMap[def];
						expr_63[expr_6C] = num + 80f;
					}
				}
			}
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
				DefMap<RecordDef, float> expr_47 = defMap = this.records;
				float num = defMap[def];
				expr_47[def] = num + value;
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

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<DefMap<RecordDef, float>>(ref this.records, "records", new object[0]);
		}
	}
}
