using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.Sound
{
	public abstract class Sample
	{
		public SubSoundDef subDef;

		public AudioSource source;

		public float startRealTime;

		public int startTick;

		public float resolvedVolume;

		public float resolvedPitch;

		private bool mappingsApplied;

		private Dictionary<SoundParamTarget, float> volumeInMappings = new Dictionary<SoundParamTarget, float>();

		public float AgeRealTime
		{
			get
			{
				return Time.realtimeSinceStartup - this.startRealTime;
			}
		}

		public int AgeTicks
		{
			get
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					return Find.TickManager.TicksGame - this.startTick;
				}
				return (int)(this.AgeRealTime * 60f);
			}
		}

		public abstract float ParentStartRealTime
		{
			get;
		}

		public abstract float ParentStartTick
		{
			get;
		}

		public abstract float ParentHashCode
		{
			get;
		}

		public abstract SoundParams ExternalParams
		{
			get;
		}

		public abstract SoundInfo Info
		{
			get;
		}

		public Map Map
		{
			get
			{
				return this.Info.Maker.Map;
			}
		}

		protected bool TestPlaying
		{
			get
			{
				return this.Info.testPlay;
			}
		}

		protected float MappedVolumeMultiplier
		{
			get
			{
				float num = 1f;
				foreach (float num2 in this.volumeInMappings.Values)
				{
					num *= num2;
				}
				return num;
			}
		}

		protected float ContextVolumeMultiplier
		{
			get
			{
				if (SoundDefHelper.CorrectContextNow(this.subDef.parentDef, this.Map))
				{
					return 1f;
				}
				return 0f;
			}
		}

		protected virtual float Volume
		{
			get
			{
				if (this.subDef.muteWhenPaused && Current.ProgramState == ProgramState.Playing && Find.TickManager.Paused && !this.TestPlaying)
				{
					return 0f;
				}
				return this.resolvedVolume * this.Info.volumeFactor * this.MappedVolumeMultiplier * this.ContextVolumeMultiplier;
			}
		}

		public float SanitizedVolume
		{
			get
			{
				return AudioSourceUtility.GetSanitizedVolume(this.Volume, this.subDef.parentDef);
			}
		}

		protected virtual float Pitch
		{
			get
			{
				float num = this.resolvedPitch * this.Info.pitchFactor;
				if (this.subDef.tempoAffectedByGameSpeed && Current.ProgramState == ProgramState.Playing && !this.TestPlaying && !Find.TickManager.Paused)
				{
					num *= Find.TickManager.TickRateMultiplier;
				}
				return num;
			}
		}

		public float SanitizedPitch
		{
			get
			{
				return AudioSourceUtility.GetSanitizedPitch(this.Pitch, this.subDef.parentDef);
			}
		}

		public Sample(SubSoundDef def)
		{
			this.subDef = def;
			this.resolvedVolume = def.RandomizedVolume();
			this.resolvedPitch = def.pitchRange.RandomInRange;
			this.startRealTime = Time.realtimeSinceStartup;
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.startTick = Find.TickManager.TicksGame;
			}
			else
			{
				this.startTick = 0;
			}
			foreach (SoundParamTarget_Volume current in (from m in this.subDef.paramMappings
			select m.outParam).OfType<SoundParamTarget_Volume>())
			{
				this.volumeInMappings.Add(current, 0f);
			}
		}

		public virtual void Update()
		{
			this.source.pitch = this.SanitizedPitch;
			this.ApplyMappedParameters();
			this.source.volume = this.SanitizedVolume;
			if (this.source.volume < 0.001f)
			{
				this.source.mute = true;
			}
			else
			{
				this.source.mute = false;
			}
			if (this.subDef.tempoAffectedByGameSpeed && !this.TestPlaying)
			{
				if (Current.ProgramState == ProgramState.Playing && Find.TickManager.Paused)
				{
					if (this.source.isPlaying)
					{
						this.source.Pause();
					}
				}
				else if (!this.source.isPlaying)
				{
					this.source.UnPause();
				}
			}
		}

		public void ApplyMappedParameters()
		{
			for (int i = 0; i < this.subDef.paramMappings.Count; i++)
			{
				SoundParameterMapping soundParameterMapping = this.subDef.paramMappings[i];
				if (soundParameterMapping.paramUpdateMode != SoundParamUpdateMode.OncePerSample || !this.mappingsApplied)
				{
					soundParameterMapping.Apply(this);
				}
			}
			this.mappingsApplied = true;
		}

		public void SignalMappedVolume(float value, SoundParamTarget sourceParam)
		{
			this.volumeInMappings[sourceParam] = value;
		}

		public virtual void SampleCleanup()
		{
			for (int i = 0; i < this.subDef.paramMappings.Count; i++)
			{
				SoundParameterMapping soundParameterMapping = this.subDef.paramMappings[i];
				if (soundParameterMapping.curve.HasView)
				{
					soundParameterMapping.curve.View.ClearDebugInputFrom(this);
				}
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Sample_",
				this.subDef.name,
				" volume=",
				this.source.volume,
				" at ",
				this.source.transform.position.ToIntVec3()
			});
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine<SubSoundDef>(this.startRealTime.GetHashCode(), this.subDef);
		}
	}
}
