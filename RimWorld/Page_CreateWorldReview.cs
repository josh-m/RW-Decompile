using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_CreateWorldReview : Page
	{
		private WorldInterface worldInterface = new WorldInterface();

		public override string PageTitle
		{
			get
			{
				return "ReviewWorld".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.worldInterface.Reset();
		}

		public override void PostOpen()
		{
			base.PostOpen();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.WorldCameraMovement, OpportunityType.Important);
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			this.worldInterface.Draw(base.GetMainRect(rect, 0f, false), false);
			base.DoBottomButtons(rect, "WorldSaveAndFinish".Translate(), null, null, true);
		}

		protected override void DoBack()
		{
			Current.Game = null;
			base.DoBack();
		}

		protected override void DoNext()
		{
			GameDataSaveLoader.SaveWorld(Find.World, Find.World.info.FileNameNoExtension);
			Current.Game = null;
			base.DoNext();
		}
	}
}
