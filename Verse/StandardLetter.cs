using System;
using System.Collections.Generic;

namespace Verse
{
	public class StandardLetter : ChoiceLetter
	{
		protected override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.OK;
				if (this.lookTarget.IsValid)
				{
					yield return base.JumpToLocation;
				}
			}
		}
	}
}
