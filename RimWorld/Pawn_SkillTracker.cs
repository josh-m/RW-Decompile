using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_SkillTracker : IExposable
	{
		private Pawn pawn;

		public List<SkillRecord> skills = new List<SkillRecord>();

		public Pawn_SkillTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
			foreach (SkillDef current in DefDatabase<SkillDef>.AllDefs)
			{
				this.skills.Add(new SkillRecord(this.pawn, current));
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<SkillRecord>(ref this.skills, "skills", LookMode.Deep, new object[]
			{
				this.pawn
			});
		}

		public SkillRecord GetSkill(SkillDef skillDef)
		{
			for (int i = 0; i < this.skills.Count; i++)
			{
				if (this.skills[i].def == skillDef)
				{
					return this.skills[i];
				}
			}
			Log.Error(string.Concat(new object[]
			{
				"Did not find skill of def ",
				skillDef,
				", returning ",
				this.skills[0]
			}));
			return this.skills[0];
		}

		public void SkillsTick()
		{
			if ((Find.TickManager.TicksGame + this.pawn.thingIDNumber) % 200 == 0)
			{
				for (int i = 0; i < this.skills.Count; i++)
				{
					this.skills[i].Interval();
				}
			}
		}

		public void Learn(SkillDef sDef, float xp, bool direct = false)
		{
			this.GetSkill(sDef).Learn(xp, direct);
		}

		public float AverageOfRelevantSkillsFor(WorkTypeDef workDef)
		{
			if (workDef.relevantSkills.Count == 0)
			{
				return 3f;
			}
			float num = 0f;
			for (int i = 0; i < workDef.relevantSkills.Count; i++)
			{
				num += (float)this.GetSkill(workDef.relevantSkills[i]).Level;
			}
			return num / (float)workDef.relevantSkills.Count;
		}

		public Passion MaxPassionOfRelevantSkillsFor(WorkTypeDef workDef)
		{
			if (workDef.relevantSkills.Count == 0)
			{
				return Passion.None;
			}
			Passion passion = Passion.None;
			for (int i = 0; i < workDef.relevantSkills.Count; i++)
			{
				Passion passion2 = this.GetSkill(workDef.relevantSkills[i]).passion;
				if (passion2 > passion)
				{
					passion = passion2;
				}
			}
			return passion;
		}

		public void Notify_SkillDisablesChanged()
		{
			for (int i = 0; i < this.skills.Count; i++)
			{
				this.skills[i].Notify_SkillDisablesChanged();
			}
		}
	}
}
