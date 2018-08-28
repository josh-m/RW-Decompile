using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class SampleOneShot : Sample
	{
		public SoundInfo info;

		private SoundParams externalParams = new SoundParams();

		public override float ParentStartRealTime
		{
			get
			{
				return this.startRealTime;
			}
		}

		public override float ParentStartTick
		{
			get
			{
				return (float)this.startTick;
			}
		}

		public override float ParentHashCode
		{
			get
			{
				return (float)this.GetHashCode();
			}
		}

		public override SoundParams ExternalParams
		{
			get
			{
				return this.externalParams;
			}
		}

		public override SoundInfo Info
		{
			get
			{
				return this.info;
			}
		}

		private SampleOneShot(SubSoundDef def) : base(def)
		{
		}

		public static SampleOneShot TryMakeAndPlay(SubSoundDef def, AudioClip clip, SoundInfo info)
		{
			if ((double)info.pitchFactor <= 0.0001)
			{
				Log.ErrorOnce(string.Concat(new object[]
				{
					"Played sound with pitchFactor ",
					info.pitchFactor,
					": ",
					def,
					", ",
					info
				}), 632321, false);
				return null;
			}
			SampleOneShot sampleOneShot = new SampleOneShot(def);
			sampleOneShot.info = info;
			sampleOneShot.source = Find.SoundRoot.sourcePool.GetSource(def.onCamera);
			if (sampleOneShot.source == null)
			{
				return null;
			}
			sampleOneShot.source.clip = clip;
			sampleOneShot.source.volume = sampleOneShot.SanitizedVolume;
			sampleOneShot.source.pitch = sampleOneShot.SanitizedPitch;
			sampleOneShot.source.minDistance = sampleOneShot.subDef.distRange.TrueMin;
			sampleOneShot.source.maxDistance = sampleOneShot.subDef.distRange.TrueMax;
			if (!def.onCamera)
			{
				sampleOneShot.source.gameObject.transform.position = info.Maker.Cell.ToVector3ShiftedWithAltitude(0f);
				sampleOneShot.source.minDistance = def.distRange.TrueMin;
				sampleOneShot.source.maxDistance = def.distRange.TrueMax;
				sampleOneShot.source.spatialBlend = 1f;
			}
			else
			{
				sampleOneShot.source.spatialBlend = 0f;
			}
			for (int i = 0; i < def.filters.Count; i++)
			{
				def.filters[i].SetupOn(sampleOneShot.source);
			}
			foreach (KeyValuePair<string, float> current in info.DefinedParameters)
			{
				sampleOneShot.externalParams[current.Key] = current.Value;
			}
			sampleOneShot.Update();
			sampleOneShot.source.Play();
			Find.SoundRoot.oneShotManager.TryAddPlayingOneShot(sampleOneShot);
			return sampleOneShot;
		}
	}
}
