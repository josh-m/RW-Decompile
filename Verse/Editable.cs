using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class Editable
	{
		public virtual void ResolveReferences()
		{
		}

		public virtual void PostLoad()
		{
		}

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors()
		{
		}
	}
}
