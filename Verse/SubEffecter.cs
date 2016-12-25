using System;

namespace Verse
{
	public class SubEffecter
	{
		public SubEffecterDef def;

		public SubEffecter(SubEffecterDef subDef)
		{
			this.def = subDef;
		}

		public virtual void SubEffectTick(TargetInfo A, TargetInfo B)
		{
		}

		public virtual void SubTrigger(TargetInfo A, TargetInfo B)
		{
		}

		public virtual void SubCleanup()
		{
		}
	}
}
