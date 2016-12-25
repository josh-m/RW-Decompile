using System;

namespace Verse
{
	public class MentalBreakWorker
	{
		public MentalBreakDef def;

		public virtual float CommonalityFor(Pawn pawn)
		{
			return this.def.baseCommonality;
		}
	}
}
