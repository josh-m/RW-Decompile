using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SkillNeed_Direct : SkillNeed
	{
		public List<float> factorsPerLevel = new List<float>();

		public override float FactorFor(Pawn pawn)
		{
			if (pawn.skills == null)
			{
				return 1f;
			}
			int level = pawn.skills.GetSkill(this.skill).Level;
			if (this.factorsPerLevel.Count > level)
			{
				return this.factorsPerLevel[level];
			}
			if (this.factorsPerLevel.Count > 0)
			{
				return this.factorsPerLevel[this.factorsPerLevel.Count - 1];
			}
			return 1f;
		}
	}
}
