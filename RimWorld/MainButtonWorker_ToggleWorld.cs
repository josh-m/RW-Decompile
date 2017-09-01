using RimWorld.Planet;
using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainButtonWorker_ToggleWorld : MainButtonWorker
	{
		public bool resetViewNextTime = true;

		public override void Activate()
		{
			if (Find.World.renderer.wantedMode == WorldRenderMode.None)
			{
				Find.World.renderer.wantedMode = WorldRenderMode.Planet;
				if (this.resetViewNextTime)
				{
					this.resetViewNextTime = false;
					Find.World.UI.Reset();
				}
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.FormCaravan, OpportunityType.Important);
				Find.MainTabsRoot.EscapeCurrentTab(false);
				SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
			}
			else if (Find.MainTabsRoot.OpenTab != null && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect)
			{
				Find.MainTabsRoot.EscapeCurrentTab(false);
				SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
			}
			else
			{
				Find.World.renderer.wantedMode = WorldRenderMode.None;
				SoundDefOf.TabClose.PlayOneShotOnCamera(null);
			}
		}
	}
}
