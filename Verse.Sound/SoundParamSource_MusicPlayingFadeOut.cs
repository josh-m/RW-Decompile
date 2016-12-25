using System;

namespace Verse.Sound
{
	public class SoundParamSource_MusicPlayingFadeOut : SoundParamSource
	{
		public override string Label
		{
			get
			{
				return "Music playing";
			}
		}

		public override float ValueFor(Sample samp)
		{
			if (Current.ProgramState != ProgramState.MapPlaying || Find.Map == null || Find.MusicManagerMap == null)
			{
				return 1f;
			}
			return Find.MusicManagerMap.subtleAmbienceSoundVolumeMultiplier;
		}
	}
}
