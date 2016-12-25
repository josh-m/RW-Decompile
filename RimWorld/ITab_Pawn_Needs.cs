using System;
using UnityEngine;

namespace RimWorld
{
	public class ITab_Pawn_Needs : ITab
	{
		private Vector2 thoughtScrollPosition;

		public override bool IsVisible
		{
			get
			{
				return base.SelPawn.needs != null && base.SelPawn.needs.AllNeeds.Count > 0;
			}
		}

		public ITab_Pawn_Needs()
		{
			this.labelKey = "TabNeeds";
			this.tutorTag = "Needs";
		}

		public override void OnOpen()
		{
			this.thoughtScrollPosition = default(Vector2);
		}

		protected override void FillTab()
		{
			NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, this.size.x, this.size.y), base.SelPawn, ref this.thoughtScrollPosition);
		}

		protected override void UpdateSize()
		{
			this.size = NeedsCardUtility.GetSize(base.SelPawn);
		}
	}
}
