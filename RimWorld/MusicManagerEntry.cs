using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MusicManagerEntry
	{
		private const string SourceGameObjectName = "MusicAudioSourceDummy";

		private AudioSource audioSource;

		private float CurVolume
		{
			get
			{
				return Prefs.VolumeMusic * SongDefOf.EntrySong.volume;
			}
		}

		public void MusicManagerEntryUpdate()
		{
			if (this.audioSource == null || !this.audioSource.isPlaying)
			{
				this.StartPlaying();
			}
			this.audioSource.volume = this.CurVolume;
		}

		private void StartPlaying()
		{
			if (this.audioSource != null && !this.audioSource.isPlaying)
			{
				this.audioSource.Play();
				return;
			}
			if (GameObject.Find("MusicAudioSourceDummy") != null)
			{
				Log.Error("MusicManagerEntry did StartPlaying but there is already a music source GameObject.");
				return;
			}
			this.audioSource = new GameObject("MusicAudioSourceDummy")
			{
				transform = 
				{
					parent = Camera.main.transform
				}
			}.AddComponent<AudioSource>();
			this.audioSource.bypassEffects = true;
			this.audioSource.bypassListenerEffects = true;
			this.audioSource.bypassReverbZones = true;
			this.audioSource.priority = 0;
			this.audioSource.clip = SongDefOf.EntrySong.clip;
			this.audioSource.volume = this.CurVolume;
			this.audioSource.loop = true;
			this.audioSource.spatialBlend = 0f;
			this.audioSource.Play();
		}
	}
}
