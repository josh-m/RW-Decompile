using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_World : MainTabWindow
	{
		public bool everOpened;

		public override Vector2 RequestedTabSize
		{
			get
			{
				return Vector2.zero;
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public MainTabWindow_World()
		{
			this.preventCameraMotion = true;
			this.wantsRenderedWorld = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			if (!this.everOpened)
			{
				this.everOpened = true;
				Find.World.UI.Reset();
			}
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.FormCaravan, OpportunityType.Important);
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			this.closeOnEscapeKey = (Find.VisibleMap != null);
		}
	}
}
