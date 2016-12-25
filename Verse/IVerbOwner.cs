using System;
using System.Collections.Generic;

namespace Verse
{
	public interface IVerbOwner
	{
		VerbTracker VerbTracker
		{
			get;
		}

		List<VerbProperties> VerbProperties
		{
			get;
		}
	}
}
