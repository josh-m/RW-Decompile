using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderDef : Def
	{
		public Type workerClass;

		public int recordTicksFrequency = 60000;

		public Color graphColor = Color.green;

		[Unsaved]
		private HistoryAutoRecorderWorker workerInt;

		public HistoryAutoRecorderWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (HistoryAutoRecorderWorker)Activator.CreateInstance(this.workerClass);
				}
				return this.workerInt;
			}
		}

		public static HistoryAutoRecorderDef Named(string defName)
		{
			return DefDatabase<HistoryAutoRecorderDef>.GetNamed(defName, true);
		}
	}
}
