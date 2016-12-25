using System;
using UnityEngine;

namespace Verse.Sound
{
	public abstract class SoundFilter
	{
		public abstract void SetupOn(AudioSource source);

		protected static T GetOrMakeFilterOn<T>(AudioSource source) where T : Behaviour
		{
			T t = source.gameObject.GetComponent<T>();
			if (t != null)
			{
				t.enabled = true;
			}
			else
			{
				t = source.gameObject.AddComponent<T>();
			}
			return t;
		}
	}
}
