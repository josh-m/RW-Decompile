using System;
using UnityEngine;

namespace Verse.Sound
{
	public class ResolvedGrain_Clip : ResolvedGrain
	{
		public AudioClip clip;

		public ResolvedGrain_Clip(AudioClip clip)
		{
			this.clip = clip;
			this.duration = clip.length;
		}

		public override string ToString()
		{
			return "Clip:" + this.clip.name;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ResolvedGrain_Clip resolvedGrain_Clip = obj as ResolvedGrain_Clip;
			return resolvedGrain_Clip != null && resolvedGrain_Clip.clip == this.clip;
		}

		public override int GetHashCode()
		{
			if (this.clip == null)
			{
				return 0;
			}
			return this.clip.GetHashCode();
		}
	}
}
