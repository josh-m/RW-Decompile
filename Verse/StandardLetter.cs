using System;
using System.Collections.Generic;

namespace Verse
{
	public class StandardLetter : ChoiceLetter
	{
		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				if (this.lookTargets.IsValid())
				{
					yield return base.Option_JumpToLocation;
				}
			}
		}
	}
}
