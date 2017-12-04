using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SkillNeed_Direct : SkillNeed
	{
		public List<float> valuesPerLevel = new List<float>();

		public override float ValueFor(Pawn pawn)
		{
			if (pawn.skills == null)
			{
				return 1f;
			}
			int level = pawn.skills.GetSkill(this.skill).Level;
			if (this.valuesPerLevel.Count > level)
			{
				return this.valuesPerLevel[level];
			}
			if (this.valuesPerLevel.Count > 0)
			{
				return this.valuesPerLevel[this.valuesPerLevel.Count - 1];
			}
			return 1f;
		}
	}
}
