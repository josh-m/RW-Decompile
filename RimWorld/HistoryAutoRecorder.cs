using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorder : IExposable
	{
		public HistoryAutoRecorderDef def;

		public List<float> records;

		public HistoryAutoRecorder()
		{
			this.records = new List<float>();
		}

		public void Tick()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame % this.def.recordTicksFrequency == 0 || !this.records.Any<float>())
			{
				float item = this.def.Worker.PullRecord();
				this.records.Add(item);
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<HistoryAutoRecorderDef>(ref this.def, "def");
			byte[] array = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				array = new byte[this.records.Count * 4];
				for (int i = 0; i < this.records.Count; i++)
				{
					byte[] bytes = BitConverter.GetBytes(this.records[i]);
					for (int j = 0; j < 4; j++)
					{
						array[i * 4 + j] = bytes[j];
					}
				}
			}
			ArrayExposeUtility.ExposeByteArray(ref array, "records");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				int num = array.Length / 4;
				this.records.Clear();
				for (int k = 0; k < num; k++)
				{
					float item = BitConverter.ToSingle(array, k * 4);
					this.records.Add(item);
				}
			}
		}
	}
}
