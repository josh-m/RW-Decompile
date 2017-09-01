using System;

namespace Verse
{
	public class HediffGiver_Event : HediffGiver
	{
		private float chance = 1f;

		public bool EventOccurred(Pawn pawn)
		{
			return Rand.Value < this.chance && base.TryApply(pawn, null);
		}
	}
}
