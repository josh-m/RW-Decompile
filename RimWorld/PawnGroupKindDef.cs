using System;
using Verse;

namespace RimWorld
{
	public class PawnGroupKindDef : Def
	{
		public Type workerClass;

		[Unsaved]
		private PawnGroupKindWorker workerInt;

		public PawnGroupKindWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (PawnGroupKindWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
