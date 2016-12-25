using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RecordDef : Def
	{
		public RecordType type;

		public Type workerClass = typeof(RecordWorker);

		public List<JobDef> measuredTimeJobs;

		[Unsaved]
		private RecordWorker workerInt;

		public RecordWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (RecordWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
