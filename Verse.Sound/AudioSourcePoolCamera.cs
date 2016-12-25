using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class AudioSourcePoolCamera
	{
		private const int NumSourcesCamera = 16;

		public GameObject cameraSourcesContainer;

		private List<AudioSource> sourcesCamera = new List<AudioSource>();

		public AudioSourcePoolCamera()
		{
			this.cameraSourcesContainer = new GameObject("OneShotSourcesCameraContainer");
			this.cameraSourcesContainer.transform.parent = Find.Camera.transform;
			this.cameraSourcesContainer.transform.localPosition = Vector3.zero;
			for (int i = 0; i < 16; i++)
			{
				AudioSource audioSource = AudioSourceMaker.NewAudioSourceOn(new GameObject("OneShotSourceCamera_" + i.ToString())
				{
					transform = 
					{
						parent = this.cameraSourcesContainer.transform,
						localPosition = Vector3.zero
					}
				});
				audioSource.bypassReverbZones = true;
				this.sourcesCamera.Add(audioSource);
			}
		}

		public AudioSource GetSourceCamera()
		{
			for (int i = 0; i < this.sourcesCamera.Count; i++)
			{
				AudioSource audioSource = this.sourcesCamera[i];
				if (!audioSource.isPlaying)
				{
					audioSource.clip = null;
					SoundFilterUtility.DisableAllFiltersOn(audioSource);
					return audioSource;
				}
			}
			return null;
		}
	}
}
