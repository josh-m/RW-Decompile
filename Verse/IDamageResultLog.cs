using System;
using System.Collections.Generic;

namespace Verse
{
	public interface IDamageResultLog
	{
		void FillTargets(List<BodyPartDef> recipientParts, List<bool> recipientPartsDestroyed);
	}
}
