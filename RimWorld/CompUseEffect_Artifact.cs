using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompUseEffect_Artifact : CompUseEffect
	{
		public override float OrderPriority
		{
			get
			{
				return -1000f;
			}
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			if (usedBy.MapHeld == Find.VisibleMap || this.parent.MapHeld == Find.VisibleMap)
			{
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
			}
			usedBy.records.Increment(RecordDefOf.ArtifactsActivated);
			this.parent.Destroy(DestroyMode.Vanish);
		}
	}
}
