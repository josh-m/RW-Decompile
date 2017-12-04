using RimWorld;
using System;

namespace Verse
{
	public abstract class DeathActionWorker
	{
		public virtual RulePackDef DeathRules
		{
			get
			{
				return RulePackDefOf.Transition_Died;
			}
		}

		public abstract void PawnDied(Corpse corpse);
	}
}
