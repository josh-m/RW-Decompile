using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompUseEffect_Artifact : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(usedBy.MapHeld);
			usedBy.records.Increment(RecordDefOf.ArtifactsActivated);
		}
	}
}
