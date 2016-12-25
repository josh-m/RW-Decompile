using System;
using Verse;

namespace RimWorld
{
	public class CompUseEffect_LearnSkill : CompUseEffect
	{
		private const float XPGainAmount = 50000f;

		public override float OrderPriority
		{
			get
			{
				return -1000f;
			}
		}

		public override void DoEffect(Pawn user)
		{
			base.DoEffect(user);
			SkillDef skill = this.parent.GetComp<CompNeurotrainer>().skill;
			int level = user.skills.GetSkill(skill).Level;
			user.skills.Learn(skill, 50000f, true);
			int level2 = user.skills.GetSkill(skill).Level;
			if (PawnUtility.ShouldSendNotificationAbout(user))
			{
				Messages.Message("NeurotrainerUsed".Translate(new object[]
				{
					user.LabelShort,
					skill.label,
					level,
					level2
				}), user, MessageSound.Benefit);
			}
			this.parent.Destroy(DestroyMode.Vanish);
		}
	}
}
