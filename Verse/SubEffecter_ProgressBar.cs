using RimWorld;
using System;

namespace Verse
{
	public class SubEffecter_ProgressBar : SubEffecter
	{
		private const float Width = 0.68f;

		private const float Height = 0.12f;

		public MoteProgressBar mote;

		public SubEffecter_ProgressBar(SubEffecterDef def) : base(def)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (this.mote == null)
			{
				this.mote = (MoteProgressBar)MoteMaker.MakeInteractionOverlay(this.def.moteDef, A, B);
				this.mote.exactScale.x = 0.68f;
				this.mote.exactScale.z = 0.12f;
			}
		}

		public override void SubCleanup()
		{
			if (this.mote != null && !this.mote.Destroyed)
			{
				this.mote.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
