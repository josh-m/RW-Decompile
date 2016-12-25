using System;
using System.Collections.Generic;

namespace Verse.Sound
{
	public class SoundParams
	{
		private Dictionary<string, float> storedParams = new Dictionary<string, float>();

		public SoundSizeAggregator sizeAggregator;

		public float this[string key]
		{
			get
			{
				return this.storedParams[key];
			}
			set
			{
				this.storedParams[key] = value;
			}
		}

		public bool TryGetValue(string key, out float val)
		{
			return this.storedParams.TryGetValue(key, out val);
		}
	}
}
