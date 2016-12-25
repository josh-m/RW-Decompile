using System;

namespace Verse.Sound
{
	[EditorReplaceable, EditorShowClassName]
	public abstract class SoundParamSource
	{
		public abstract string Label
		{
			get;
		}

		public abstract float ValueFor(Sample samp);
	}
}
