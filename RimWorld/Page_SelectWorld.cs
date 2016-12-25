using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_SelectWorld : Page
	{
		private const float BoxMargin = 20f;

		private const float EntrySpacing = 8f;

		private const float EntryMargin = 6f;

		private const float WorldNameExtraLeftMargin = 15f;

		private const float DateExtraLeftMargin = 400f;

		private const float DeleteButtonSpace = 5f;

		private List<SaveFileInfo> worldFiles = new List<SaveFileInfo>();

		private Vector2 scrollPosition = Vector2.zero;

		private Vector2 mapEntrySize;

		private Vector2 interactButSize;

		public override string PageTitle
		{
			get
			{
				return "SelectWorld".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Current.Game.World = null;
			this.ScanWorldFiles();
		}

		private void ScanWorldFiles()
		{
			this.worldFiles.Clear();
			foreach (FileInfo current in GenFilePaths.AllWorldFiles)
			{
				try
				{
					this.worldFiles.Add(new SaveFileInfo(current));
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + current.Name + ": " + ex.ToString());
				}
			}
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			Rect mainRect = base.GetMainRect(rect, 0f, false);
			GUI.BeginGroup(mainRect);
			this.mapEntrySize = new Vector2(mainRect.width - 16f, 48f);
			this.interactButSize = new Vector2(100f, this.mapEntrySize.y - 12f);
			float num = this.mapEntrySize.y + 8f;
			float height = (float)this.worldFiles.Count * num;
			Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, height);
			Rect outRect = new Rect(mainRect.AtZero());
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
			float num2 = 0f;
			foreach (SaveFileInfo current in this.worldFiles)
			{
				this.DrawWorldFileEntry(current, num2);
				num2 += this.mapEntrySize.y + 8f;
			}
			if (this.worldFiles.Count == 0)
			{
				Rect rect2 = new Rect(0f, num2, this.mapEntrySize.x, this.mapEntrySize.y);
				Text.Font = GameFont.Small;
				Widgets.Label(rect2, "NoWorldsFilesFound".Translate());
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			base.DoBottomButtons(rect, null, null, null, false);
		}

		private void DrawWorldFileEntry(SaveFileInfo wfi, float curY)
		{
			Rect rect = new Rect(0f, curY, this.mapEntrySize.x, this.mapEntrySize.y);
			Widgets.DrawMenuSection(rect, true);
			Rect position = rect.ContractedBy(6f);
			GUI.BeginGroup(position);
			Rect rect2 = new Rect(15f, 0f, position.width, position.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, Path.GetFileNameWithoutExtension(wfi.FileInfo.Name));
			GUI.color = Color.white;
			Rect rect3 = new Rect(400f, 0f, position.width, position.height);
			Dialog_FileList.DrawDateAndVersion(wfi, rect3);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			float num = this.mapEntrySize.x - 12f - this.interactButSize.x - this.interactButSize.y;
			Rect rect4 = new Rect(num, 0f, this.interactButSize.x, this.interactButSize.y);
			if (Widgets.ButtonText(rect4, "WorldChooseButton".Translate(), true, false, true))
			{
				this.SelectWorldFile(wfi.FileInfo);
			}
			Rect rect5 = new Rect(num + this.interactButSize.x + 5f, 0f, this.interactButSize.y, this.interactButSize.y);
			if (Widgets.ButtonImage(rect5, TexButton.DeleteX))
			{
				FileInfo localFile = wfi.FileInfo;
				Find.WindowStack.Add(new Dialog_Confirm("ConfirmDelete".Translate(new object[]
				{
					localFile.Name
				}), delegate
				{
					localFile.Delete();
					this.ScanWorldFiles();
				}, true, null, true));
			}
			TooltipHandler.TipRegion(rect5, "DeleteThisSavegame".Translate());
			GUI.EndGroup();
		}

		private void SelectWorldFile(FileInfo worldFile)
		{
			PreLoadUtility.CheckVersionAndLoad(worldFile.ToString(), ScribeMetaHeaderUtility.ScribeHeaderMode.World, delegate
			{
				Find.GameInitData.ResetWorldRelatedMapInitData();
				try
				{
					GameDataSaveLoader.LoadWorldFromFileIntoGame(worldFile.ToString());
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading world from " + worldFile.Name + ":\n" + ex.ToString());
					Current.Game.World = null;
				}
				Find.Scenario.PostWorldLoad();
				if (this.CanDoNext())
				{
					this.DoNext();
				}
			});
		}

		protected override bool CanDoNext()
		{
			return base.CanDoNext() && Find.World != null;
		}
	}
}
