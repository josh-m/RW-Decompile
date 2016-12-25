using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Verse.Sound
{
	public class AudioGrain_Clip : AudioGrain
	{
		public string clipPath = string.Empty;

		[DebuggerHidden]
		public override IEnumerable<ResolvedGrain> GetResolvedGrains()
		{
			AudioClip clip = ContentFinder<AudioClip>.Get(this.clipPath, true);
			if (clip != null)
			{
				yield return new ResolvedGrain_Clip(clip);
			}
			else
			{
				Log.Error("Grain couldn't resolve: Clip not found at " + this.clipPath);
			}
		}
	}
}
