using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabWindow_Menu : MainTabWindow
	{
		private bool anyGameFiles;

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(450f, 390f);
			}
		}

		public override MainTabWindowAnchor Anchor
		{
			get
			{
				return MainTabWindowAnchor.Right;
			}
		}

		public MainTabWindow_Menu()
		{
			this.forcePause = true;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			PlayerKnowledgeDatabase.Save();
			ShipCountdown.CancelCountdown();
			this.anyGameFiles = GenFilePaths.AllSavedGameFiles.Any<FileInfo>();
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			VersionControl.DrawInfoInCorner();
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DoWindowContents(rect);
			MainMenuDrawer.DoMainMenuControls(rect, this.anyGameFiles);
		}
	}
}
