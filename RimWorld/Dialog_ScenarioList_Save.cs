using System;
using Verse;

namespace RimWorld
{
	public class Dialog_ScenarioList_Save : Dialog_ScenarioList
	{
		private Scenario savingScen;

		protected override bool ShouldDoTypeInField
		{
			get
			{
				return true;
			}
		}

		public Dialog_ScenarioList_Save(Scenario scen)
		{
			this.interactButLabel = "OverwriteButton".Translate();
			this.typingName = scen.name;
			this.savingScen = scen;
		}

		protected override void DoFileInteraction(string fileName)
		{
			string absPath = GenFilePaths.AbsPathForScenario(fileName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveScenario(this.savingScen, absPath);
			}, "SavingLongEvent", false, null);
			Messages.Message("SavedAs".Translate(new object[]
			{
				fileName
			}), MessageSound.Silent);
			this.Close(true);
		}
	}
}
