using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class PawnCapacitiesHandler
	{
		private Pawn pawn;

		private DefMap<PawnCapacityDef, float> cachedActivitiesEfficiency;

		public bool CanBeAwake
		{
			get
			{
				return this.GetEfficiency(PawnCapacityDefOf.Consciousness) >= 0.3f;
			}
		}

		public PawnCapacitiesHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void Clear()
		{
			this.cachedActivitiesEfficiency = null;
		}

		public float GetEfficiency(PawnCapacityDef activity)
		{
			if (this.pawn.health.Dead)
			{
				return 0f;
			}
			if (this.cachedActivitiesEfficiency == null)
			{
				this.Notify_ActivitiesEfficiencyDirty();
			}
			return this.cachedActivitiesEfficiency[activity];
		}

		public bool CapableOf(PawnCapacityDef activity)
		{
			return this.GetEfficiency(activity) > activity.minForCapable;
		}

		public void Notify_ActivitiesEfficiencyDirty()
		{
			if (this.cachedActivitiesEfficiency == null)
			{
				this.cachedActivitiesEfficiency = new DefMap<PawnCapacityDef, float>();
			}
			ProfilerThreadCheck.BeginSample("Calculate and cache activity efficiencies");
			this.cachedActivitiesEfficiency.SetAll(0f);
			List<PawnCapacityDef> pawnCapacityDefsListInProcessingOrder = PawnCapacityUtility.PawnCapacityDefsListInProcessingOrder;
			for (int i = 0; i < pawnCapacityDefsListInProcessingOrder.Count; i++)
			{
				this.cachedActivitiesEfficiency[pawnCapacityDefsListInProcessingOrder[i]] = PawnCapacityUtility.CalculateEfficiency(this.pawn.health.hediffSet, pawnCapacityDefsListInProcessingOrder[i]);
			}
			ProfilerThreadCheck.EndSample();
		}
	}
}
