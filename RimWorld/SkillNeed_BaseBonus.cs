using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class SkillNeed_BaseBonus : SkillNeed
	{
		private float baseValue = 0.5f;

		private float bonusPerLevel = 0.05f;

		public override float ValueFor(Pawn pawn)
		{
			if (pawn.skills == null)
			{
				return 1f;
			}
			int level = pawn.skills.GetSkill(this.skill).Level;
			return this.ValueAtLevel(level);
		}

		private float ValueAtLevel(int level)
		{
			return this.baseValue + this.bonusPerLevel * (float)level;
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string error in base.ConfigErrors())
			{
				yield return error;
			}
			for (int i = 1; i <= 20; i++)
			{
				float factor = this.ValueAtLevel(i);
				if (factor <= 0f)
				{
					yield return "SkillNeed yields factor < 0 at skill level " + i;
				}
			}
		}
	}
}
