using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_Negotiation : Dialog_NodeTree
	{
		protected Pawn negotiator;

		protected ICommunicable commTarget;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600f, 500f);
			}
		}

		public Dialog_Negotiation(Pawn negotiator, ICommunicable commTarget, DiaNode startNode, bool radioMode) : base(startNode, radioMode, false)
		{
			this.negotiator = negotiator;
			this.commTarget = commTarget;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect position = new Rect(0f, 0f, inRect.width, 100f);
			GUI.BeginGroup(position);
			Rect rect = new Rect(0f, 0f, position.width / 2f, 35f);
			Rect rect2 = new Rect(position.width / 2f, 0f, position.width / 2f, 35f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, this.negotiator.LabelCap);
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(rect2, this.commTarget.GetCallLabel());
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect3 = new Rect(0f, rect.yMax, rect.width, 999f);
			Rect rect4 = new Rect(position.width / 2f, rect.yMax, rect.width, 999f);
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 1f, 1f, 0.7f);
			Widgets.Label(rect3, "SocialSkillIs".Translate(new object[]
			{
				this.negotiator.skills.GetSkill(SkillDefOf.Social).level
			}));
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(rect4, this.commTarget.GetInfoText());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			GUI.EndGroup();
			Rect rect5 = new Rect(0f, 100f, inRect.width, inRect.height - 100f);
			base.DrawNode(rect5);
		}
	}
}
