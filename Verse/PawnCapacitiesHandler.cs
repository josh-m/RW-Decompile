using RimWorld;
using System;

namespace Verse
{
	public class PawnCapacitiesHandler
	{
		private enum CacheStatus
		{
			Uncached,
			Caching,
			Cached
		}

		private class CacheElement
		{
			public PawnCapacitiesHandler.CacheStatus status;

			public float value;
		}

		private Pawn pawn;

		private DefMap<PawnCapacityDef, PawnCapacitiesHandler.CacheElement> cachedCapacityLevels;

		public bool CanBeAwake
		{
			get
			{
				return this.GetLevel(PawnCapacityDefOf.Consciousness) >= 0.3f;
			}
		}

		public PawnCapacitiesHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void Clear()
		{
			this.cachedCapacityLevels = null;
		}

		public float GetLevel(PawnCapacityDef capacity)
		{
			if (this.pawn.health.Dead)
			{
				return 0f;
			}
			if (this.cachedCapacityLevels == null)
			{
				this.Notify_CapacityLevelsDirty();
			}
			PawnCapacitiesHandler.CacheElement cacheElement = this.cachedCapacityLevels[capacity];
			if (cacheElement.status == PawnCapacitiesHandler.CacheStatus.Caching)
			{
				Log.Error(string.Format("Detected infinite stat recursion when evaluating {0}", capacity));
				return 0f;
			}
			if (cacheElement.status == PawnCapacitiesHandler.CacheStatus.Uncached)
			{
				cacheElement.status = PawnCapacitiesHandler.CacheStatus.Caching;
				cacheElement.value = PawnCapacityUtility.CalculateCapacityLevel(this.pawn.health.hediffSet, capacity, null);
				cacheElement.status = PawnCapacitiesHandler.CacheStatus.Cached;
			}
			return cacheElement.value;
		}

		public bool CapableOf(PawnCapacityDef capacity)
		{
			return this.GetLevel(capacity) > capacity.minForCapable;
		}

		public void Notify_CapacityLevelsDirty()
		{
			if (this.cachedCapacityLevels == null)
			{
				this.cachedCapacityLevels = new DefMap<PawnCapacityDef, PawnCapacitiesHandler.CacheElement>();
			}
			for (int i = 0; i < this.cachedCapacityLevels.Count; i++)
			{
				this.cachedCapacityLevels[i].status = PawnCapacitiesHandler.CacheStatus.Uncached;
			}
		}
	}
}
