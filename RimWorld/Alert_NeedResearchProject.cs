using System;
using Verse;

namespace RimWorld
{
	public class Alert_NeedResearchProject : Alert_Medium
	{
		public override AlertReport Report
		{
			get
			{
				return Find.ResearchManager.currentProj == null && Find.ListerBuildings.ColonistsHaveResearchBench() && Find.ResearchManager.AnyProjectIsAvailable;
			}
		}

		public Alert_NeedResearchProject()
		{
			this.baseLabel = "NeedResearchProject".Translate();
			this.baseExplanation = "NeedResearchProjectDesc".Translate();
		}
	}
}
