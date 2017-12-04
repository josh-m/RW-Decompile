using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public abstract class DefModExtension
	{
		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors()
		{
		}
	}
}
