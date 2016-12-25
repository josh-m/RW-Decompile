using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_ScenarioList : Dialog_FileList
	{
		protected override void ReloadFiles()
		{
			this.files.Clear();
			foreach (FileInfo current in GenFilePaths.AllCustomScenarioFiles)
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
	}
}
