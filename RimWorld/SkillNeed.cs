using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class SkillNeed
	{
		public SkillDef skill;

		public bool reportInverse;

		public virtual float FactorFor(Pawn pawn)
		{
			throw new NotImplementedException();
		}

		[DebuggerHidden]
		public virtual IEnumerable<string> ConfigErrors()
		{
		}
	}
}
