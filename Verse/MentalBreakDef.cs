using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

namespace Verse
{
	public class MentalBreakDef : Def
	{
		public Type workerClass = typeof(MentalBreakWorker);

		public MentalStateDef mentalState;

		public float baseCommonality;

		public SimpleCurve commonalityFactorPerPopulationCurve;

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

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.intensity == MentalBreakIntensity.None)
			{
				yield return "intensity not set";
			}
		}
	}
}
