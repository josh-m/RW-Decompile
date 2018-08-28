using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class ResearchManager : IExposable
	{
		public ResearchProjectDef currentProj;

		private Dictionary<ResearchProjectDef, float> progress = new Dictionary<ResearchProjectDef, float>();

		private float ResearchPointsPerWorkTick = 0.00825f;

		public bool AnyProjectIsAvailable
		{
			get
			{
				return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Find((ResearchProjectDef x) => x.CanStartNow) != null;
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look<ResearchProjectDef>(ref this.currentProj, "currentProj");
			Scribe_Collections.Look<ResearchProjectDef, float>(ref this.progress, "progress", LookMode.Def, LookMode.Value);
		}

		public float GetProgress(ResearchProjectDef proj)
		{
			float result;
			if (this.progress.TryGetValue(proj, out result))
			{
				return result;
			}
			this.progress.Add(proj, 0f);
			return 0f;
		}

		public void ResearchPerformed(float amount, Pawn researcher)
		{
			if (this.currentProj == null)
			{
				Log.Error("Researched without having an active project.", false);
				return;
			}
			amount *= this.ResearchPointsPerWorkTick;
			amount *= Find.Storyteller.difficulty.researchSpeedFactor;
			if (researcher != null && researcher.Faction != null)
			{
				amount /= this.currentProj.CostFactor(researcher.Faction.def.techLevel);
			}
			if (DebugSettings.fastResearch)
			{
				amount *= 500f;
			}
			if (researcher != null)
			{
				researcher.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
			}
			float num = this.GetProgress(this.currentProj);
			num += amount;
			this.progress[this.currentProj] = num;
			if (this.currentProj.IsFinished)
			{
				this.FinishProject(this.currentProj, true, researcher);
			}
		}

		public void ReapplyAllMods()
		{
			foreach (ResearchProjectDef current in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (current.IsFinished)
				{
					current.ReapplyAllMods();
				}
			}
		}

		public void FinishProject(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null)
		{
			if (proj.prerequisites != null)
			{
				for (int i = 0; i < proj.prerequisites.Count; i++)
				{
					if (!proj.prerequisites[i].IsFinished)
					{
						this.FinishProject(proj.prerequisites[i], false, null);
					}
				}
			}
			this.progress[proj] = proj.baseCost;
			if (researcher != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, new object[]
				{
					researcher,
					this.currentProj
				});
			}
			this.ReapplyAllMods();
			if (doCompletionDialog)
			{
				string text = "ResearchFinished".Translate(new object[]
				{
					this.currentProj.LabelCap
				}) + "\n\n" + this.currentProj.description;
				DiaNode diaNode = new DiaNode(text);
				diaNode.options.Add(DiaOption.DefaultOK);
				DiaOption diaOption = new DiaOption("ResearchScreen".Translate());
				diaOption.resolveTree = true;
				diaOption.action = delegate
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research, true);
				};
				diaNode.options.Add(diaOption);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, null));
			}
			if (!proj.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.difficulty >= proj.discoveredLetterMinDifficulty)
			{
				Find.LetterStack.ReceiveLetter(proj.discoveredLetterTitle, proj.discoveredLetterText, LetterDefOf.NeutralEvent, null);
			}
			if (proj.unlockExtremeDifficulty && Find.Storyteller.difficulty.difficulty >= DifficultyDefOf.Rough.difficulty)
			{
				Prefs.ExtremeDifficultyUnlocked = true;
				Prefs.Save();
			}
			if (this.currentProj == proj)
			{
				this.currentProj = null;
			}
		}

		public void DebugSetAllProjectsFinished()
		{
			this.progress.Clear();
			foreach (ResearchProjectDef current in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				this.progress.Add(current, current.baseCost);
			}
			this.ReapplyAllMods();
		}
	}
}
