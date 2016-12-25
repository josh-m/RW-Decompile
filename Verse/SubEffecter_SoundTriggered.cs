using System;
using Verse.Sound;

namespace Verse
{
	public class SubEffecter_SoundTriggered : SubEffecter
	{
		public SubEffecter_SoundTriggered(SubEffecterDef def) : base(def)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B)
		{
			this.def.soundDef.PlayOneShot(new TargetInfo(A.Cell, A.Map, false));
		}
	}
}
