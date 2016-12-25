using RimWorld;
using System;

namespace Verse
{
	public class SubEffecter_InteractSymbol : SubEffecter
	{
		private Mote interactMote;

		public SubEffecter_InteractSymbol(SubEffecterDef def) : base(def)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (this.interactMote == null)
			{
				this.interactMote = MoteMaker.MakeInteractionOverlay(this.def.moteDef, A, B);
			}
		}

		public override void SubCleanup()
		{
			if (this.interactMote != null && !this.interactMote.Destroyed)
			{
				this.interactMote.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
