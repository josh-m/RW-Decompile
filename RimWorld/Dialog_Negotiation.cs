using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_Negotiation : Dialog_NodeTree
	{
		private const float TitleHeight = 70f;

		private const float InfoHeight = 60f;

		protected Pawn negotiator;

		protected ICommunicable commTarget;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(720f, 600f);
			}
		}

		public Dialog_Negotiation(Pawn negotiator, ICommunicable commTarget, DiaNode startNode, bool radioMode) : base(startNode, radioMode, false)
		{
			this.negotiator = negotiator;
			this.commTarget = commTarget;
		}

		public override void DoWindowContents(Rect inRect)
		{
			GUI.BeginGroup(inRect);
			Rect rect = new Rect(0f, 0f, inRect.width / 2f, 70f);
			Rect rect2 = new Rect(0f, rect.yMax, rect.width, 60f);
			Rect rect3 = new Rect(inRect.width / 2f, 0f, inRect.width / 2f, 70f);
			Rect rect4 = new Rect(inRect.width / 2f, rect.yMax, rect.width, 60f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, this.negotiator.LabelCap);
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(rect3, this.commTarget.GetCallLabel());
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 1f, 1f, 0.7f);
			Widgets.Label(rect2, "SocialSkillIs".Translate(new object[]
			{
				this.negotiator.skills.GetSkill(SkillDefOf.Social).Level
			}));
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(rect4, this.commTarget.GetInfoText());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			GUI.EndGroup();
			float num = 147f;
			Rect rect5 = new Rect(0f, num, inRect.width, inRect.height - num);
			base.DrawNode(rect5);
		}
	}
}
