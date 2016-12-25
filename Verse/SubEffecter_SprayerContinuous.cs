using System;

namespace Verse
{
	public class SubEffecter_SprayerContinuous : SubEffecter_Sprayer
	{
		private int ticksUntilMote;

		public SubEffecter_SprayerContinuous(SubEffecterDef def) : base(def)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			this.ticksUntilMote--;
			if (this.ticksUntilMote <= 0)
			{
				base.MakeMote(A, B);
				this.ticksUntilMote = this.def.ticksBetweenMotes;
			}
		}
	}
}
