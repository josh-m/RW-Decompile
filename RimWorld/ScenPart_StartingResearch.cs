using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_StartingResearch : ScenPart
	{
		private ResearchProjectDef project;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			if (Widgets.ButtonText(scenPartRect, this.project.LabelCap, true, false, true))
			{
				FloatMenuUtility.MakeMenu<ResearchProjectDef>(this.NonRedundantResearchProjects(), (ResearchProjectDef d) => d.LabelCap, (ResearchProjectDef d) => delegate
				{
					this.project = d;
				});
			}
		}

		public override void Randomize()
		{
			this.project = this.NonRedundantResearchProjects().RandomElement<ResearchProjectDef>();
		}

		private IEnumerable<ResearchProjectDef> NonRedundantResearchProjects()
		{
			return DefDatabase<ResearchProjectDef>.AllDefs.Where(delegate(ResearchProjectDef d)
			{
				if (d.tags == null || Find.Scenario.playerFaction.factionDef.startingResearchTags == null)
				{
					return true;
				}
				return !d.tags.Any((string tag) => Find.Scenario.playerFaction.factionDef.startingResearchTags.Contains(tag));
			});
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<ResearchProjectDef>(ref this.project, "project");
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_StartingResearchFinished".Translate(new object[]
			{
				this.project.LabelCap
			});
		}

		public override void PostGameStart()
		{
			Find.ResearchManager.InstantFinish(this.project, false);
		}
	}
}
