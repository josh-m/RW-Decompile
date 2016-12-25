using RimWorld;
using System;

namespace Verse
{
	public class MentalBreakDef : Def
	{
		public Type workerClass = typeof(MentalBreakWorker);

		public MentalStateDef mentalState;

		public float baseCommonality;

		public MentalBreakIntensity intensity;

		public TraitDef requiredTrait;

		private MentalBreakWorker workerInt;

		public MentalBreakWorker Worker
		{
			get
			{
				if (this.workerInt == null && this.workerClass != null)
				{
					this.workerInt = (MentalBreakWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
