using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class PawnCapacitiesHandler
	{
		private Pawn pawn;

		private DefMap<PawnCapacityDef, float> cachedCapacitiesEfficiency;

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
			this.cachedCapacitiesEfficiency = null;
		}

		public float GetEfficiency(PawnCapacityDef capacity)
		{
			if (this.pawn.health.Dead)
			{
				return 0f;
			}
			if (this.cachedCapacitiesEfficiency == null)
			{
				this.Notify_CapacityEfficienciesDirty();
			}
			return this.cachedCapacitiesEfficiency[capacity];
		}

		public bool CapableOf(PawnCapacityDef capacity)
		{
			return this.GetEfficiency(capacity) > capacity.minForCapable;
		}

		public void Notify_CapacityEfficienciesDirty()
		{
			if (this.cachedCapacitiesEfficiency == null)
			{
				this.cachedCapacitiesEfficiency = new DefMap<PawnCapacityDef, float>();
			}
			this.cachedCapacitiesEfficiency.SetAll(0f);
			List<PawnCapacityDef> pawnCapacityDefsListInProcessingOrder = PawnCapacityUtility.PawnCapacityDefsListInProcessingOrder;
			for (int i = 0; i < pawnCapacityDefsListInProcessingOrder.Count; i++)
			{
				this.cachedCapacitiesEfficiency[pawnCapacityDefsListInProcessingOrder[i]] = PawnCapacityUtility.CalculateEfficiency(this.pawn.health.hediffSet, pawnCapacityDefsListInProcessingOrder[i]);
			}
		}
	}
}
