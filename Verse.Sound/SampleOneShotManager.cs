using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Sound
{
	public class SampleOneShotManager
	{
		private List<SampleOneShot> samples = new List<SampleOneShot>();

		private List<SampleOneShot> cleanupList = new List<SampleOneShot>();

		public IEnumerable<SampleOneShot> PlayingOneShots
		{
			get
			{
				return this.samples;
			}
		}

		private float CameraDistanceSquaredOf(SoundInfo info)
		{
			return (Find.CameraDriver.MapPosition - info.Maker.Cell).LengthHorizontalSquared;
		}

		private float ImportanceOf(SampleOneShot sample)
		{
			return this.ImportanceOf(sample.subDef.parentDef, sample.info, sample.AgeRealTime);
		}

		private float ImportanceOf(SoundDef def, SoundInfo info, float ageRealTime)
		{
			if (def.priorityMode == VoicePriorityMode.PrioritizeNearest)
			{
				return 1f / (this.CameraDistanceSquaredOf(info) + 1f);
			}
			if (def.priorityMode == VoicePriorityMode.PrioritizeNewest)
			{
				return 1f / (ageRealTime + 1f);
			}
			throw new NotImplementedException();
		}

		public bool CanAddPlayingOneShot(SoundDef def, SoundInfo info)
		{
			if (!SoundDefHelper.CorrectContextNow(def, info.Maker.Map))
			{
				return false;
			}
			if ((from s in this.samples
			where s.subDef.parentDef == def && s.AgeRealTime < 0.05f
			select s).Count<SampleOneShot>() >= def.MaxSimultaneousSamples)
			{
				return false;
			}
			SampleOneShot sampleOneShot = this.LeastImportantOf(def);
			return sampleOneShot == null || this.ImportanceOf(def, info, 0f) >= this.ImportanceOf(sampleOneShot);
		}

		public void TryAddPlayingOneShot(SampleOneShot newSample)
		{
			int num = (from s in this.samples
			where s.subDef == newSample.subDef
			select s).Count<SampleOneShot>();
			if (num >= newSample.subDef.parentDef.maxVoices)
			{
				SampleOneShot sampleOneShot = this.LeastImportantOf(newSample.subDef.parentDef);
				sampleOneShot.source.Stop();
				this.samples.Remove(sampleOneShot);
			}
			this.samples.Add(newSample);
		}

		private SampleOneShot LeastImportantOf(SoundDef def)
		{
			SampleOneShot sampleOneShot = null;
			for (int i = 0; i < this.samples.Count; i++)
			{
				SampleOneShot sampleOneShot2 = this.samples[i];
				if (sampleOneShot2.subDef.parentDef == def && (sampleOneShot == null || this.ImportanceOf(sampleOneShot2) < this.ImportanceOf(sampleOneShot)))
				{
					sampleOneShot = sampleOneShot2;
				}
			}
			return sampleOneShot;
		}

		public void SampleOneShotManagerUpdate()
		{
			for (int i = 0; i < this.samples.Count; i++)
			{
				this.samples[i].ApplyMappedParameters();
			}
			this.cleanupList.Clear();
			for (int j = 0; j < this.samples.Count; j++)
			{
				SampleOneShot sampleOneShot = this.samples[j];
				if (sampleOneShot.source == null || !sampleOneShot.source.isPlaying || !SoundDefHelper.CorrectContextNow(sampleOneShot.subDef.parentDef, sampleOneShot.Map))
				{
					if (sampleOneShot.source != null && sampleOneShot.source.isPlaying)
					{
						sampleOneShot.source.Stop();
					}
					sampleOneShot.SampleCleanup();
					this.cleanupList.Add(sampleOneShot);
				}
			}
			if (this.cleanupList.Count > 0)
			{
				this.samples.RemoveAll((SampleOneShot s) => this.cleanupList.Contains(s));
			}
		}
	}
}
