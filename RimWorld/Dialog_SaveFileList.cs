using System;
using System.IO;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_SaveFileList : Dialog_FileList
	{
		private static readonly Color AutosaveTextColor = new Color(0.75f, 0.75f, 0.75f);

		protected override Color FileNameColor(SaveFileInfo sfi)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sfi.FileInfo.Name);
			if (SaveGameFilesUtility.IsAutoSave(fileNameWithoutExtension))
			{
				GUI.color = Dialog_SaveFileList.AutosaveTextColor;
			}
			return base.FileNameColor(sfi);
		}

		protected override void ReloadFiles()
		{
			this.files.Clear();
			foreach (FileInfo current in GenFilePaths.AllSavedGameFiles)
			{
				try
				{
					this.files.Add(new SaveFileInfo(current));
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + current.Name + ": " + ex.ToString());
				}
			}
		}

		public override void PostClose()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.MainTabsRoot.SetCurrentTab(MainTabDefOf.Menu, true);
			}
		}
	}
}
