using System;
using Verse.Sound;

namespace Verse
{
	public class SubEffecter_Sustainer : SubEffecter
	{
		private int age;

		private Sustainer sustainer;

		public SubEffecter_Sustainer(SubEffecterDef def) : base(def)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			this.age++;
			if (this.age > this.def.ticksBeforeSustainerStart)
			{
				if (this.sustainer == null)
				{
					SoundInfo info = SoundInfo.InMap(A, MaintenanceType.PerTick);
					this.sustainer = this.def.soundDef.TrySpawnSustainer(info);
				}
				else
				{
					this.sustainer.Maintain();
				}
			}
		}
	}
}
