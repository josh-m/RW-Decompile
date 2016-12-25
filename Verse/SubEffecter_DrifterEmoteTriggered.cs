using System;

namespace Verse
{
	public class SubEffecter_DrifterEmoteTriggered : SubEffecter_DrifterEmote
	{
		public SubEffecter_DrifterEmoteTriggered(SubEffecterDef def) : base(def)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B)
		{
			base.MakeMote(A);
		}
	}
}
