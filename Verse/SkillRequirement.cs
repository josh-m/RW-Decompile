using RimWorld;
using System;

namespace Verse
{
	public class SkillRequirement
	{
		public SkillDef skill;

		public int minLevel;

		public bool PawnSatisfies(Pawn pawn)
		{
			return pawn.skills != null && pawn.skills.GetSkill(this.skill).Level >= this.minLevel;
		}
	}
}
