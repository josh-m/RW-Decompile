using System;
using UnityEngine;

namespace Verse
{
	public abstract class ColorGenerator
	{
		public virtual Color ExemplaryColor
		{
			get
			{
				Rand.PushState(764543439);
				Color result = this.NewRandomizedColor();
				Rand.PopState();
				return result;
			}
		}

		public abstract Color NewRandomizedColor();
	}
}
