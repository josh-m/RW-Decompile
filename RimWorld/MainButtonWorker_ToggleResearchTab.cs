using System;
using Verse;

namespace RimWorld
{
	public class MainButtonWorker_ToggleResearchTab : MainButtonWorker_ToggleTab
	{
		public override float ButtonBarPercent
		{
			get
			{
				ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
				if (currentProj == null)
				{
					return 0f;
				}
				return currentProj.ProgressPercent;
			}
		}
	}
}
