using System;
using System.Collections.Generic;

namespace Verse.Sound
{
	public class SoundSizeAggregator
	{
		private List<ISizeReporter> reporters = new List<ISizeReporter>();

		private float testSize;

		public float AggregateSize
		{
			get
			{
				if (this.reporters.Count == 0)
				{
					return this.testSize;
				}
				float num = 0f;
				foreach (ISizeReporter current in this.reporters)
				{
					num += current.CurrentSize();
				}
				return num;
			}
		}

		public SoundSizeAggregator()
		{
			this.testSize = Rand.Value * 3f;
			this.testSize *= this.testSize;
		}

		public void RegisterReporter(ISizeReporter newRep)
		{
			this.reporters.Add(newRep);
		}

		public void RemoveReporter(ISizeReporter oldRep)
		{
			this.reporters.Remove(oldRep);
		}
	}
}
