using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class AudioSourcePoolWorld
	{
		private const int NumSourcesWorld = 32;

		private List<AudioSource> sourcesWorld = new List<AudioSource>();

		public AudioSourcePoolWorld()
		{
			GameObject gameObject = new GameObject("OneShotSourcesWorldContainer");
			gameObject.transform.position = Vector3.zero;
			for (int i = 0; i < 32; i++)
			{
				GameObject gameObject2 = new GameObject("OneShotSource_" + i.ToString());
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localPosition = Vector3.zero;
				this.sourcesWorld.Add(AudioSourceMaker.NewAudioSourceOn(gameObject2));
			}
		}

		public AudioSource GetSourceWorld()
		{
			foreach (AudioSource current in this.sourcesWorld)
			{
				if (!current.isPlaying)
				{
					SoundFilterUtility.DisableAllFiltersOn(current);
					return current;
				}
			}
			return null;
		}
	}
}
