using System;
using Verse;

namespace RimWorld
{
	public class Dialog_SaveFileList_Save : Dialog_SaveFileList
	{
		protected override bool ShouldDoTypeInField
		{
			get
			{
				return true;
			}
		}

		public Dialog_SaveFileList_Save()
		{
			this.interactButLabel = "OverwriteButton".Translate();
			this.bottomAreaHeight = 85f;
			if (Faction.OfPlayer.HasName)
			{
				this.typingName = Faction.OfPlayer.Name;
			}
			else
			{
				this.typingName = SaveGameFilesUtility.UnusedDefaultFileName(Faction.OfPlayer.def.LabelCap);
			}
		}

		protected override void DoFileInteraction(string mapName)
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveGame(mapName);
			}, "SavingLongEvent", false, null);
			Messages.Message("SavedAs".Translate(new object[]
			{
				mapName
			}), MessageSound.Silent);
			PlayerKnowledgeDatabase.Save();
			this.Close(true);
		}
	}
}
