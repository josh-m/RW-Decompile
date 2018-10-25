using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_FileList : Window
	{
		protected string interactButLabel = "Error";

		protected float bottomAreaHeight;

		protected List<SaveFileInfo> files = new List<SaveFileInfo>();

		protected Vector2 scrollPosition = Vector2.zero;

		protected string typingName = string.Empty;

		private bool focusedNameArea;

		protected const float EntryHeight = 40f;

		protected const float FileNameLeftMargin = 8f;

		protected const float FileNameRightMargin = 4f;

		protected const float FileInfoWidth = 94f;

		protected const float InteractButWidth = 100f;

		protected const float InteractButHeight = 36f;

		protected const float DeleteButSize = 36f;

		private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);

		protected const float NameTextFieldWidth = 400f;

		protected const float NameTextFieldHeight = 35f;

		protected const float NameTextFieldButtonSpace = 20f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(620f, 700f);
			}
		}

		protected virtual bool ShouldDoTypeInField
		{
			get
			{
				return false;
			}
		}

		public Dialog_FileList()
		{
			this.doCloseButton = true;
			this.doCloseX = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnAccept = false;
			this.ReloadFiles();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Vector2 vector = new Vector2(inRect.width - 16f, 40f);
			inRect.height -= 45f;
			float y = vector.y;
			float height = (float)this.files.Count * y;
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
			Rect outRect = new Rect(inRect.AtZero());
			outRect.height -= this.bottomAreaHeight;
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
			float num = 0f;
			int num2 = 0;
			foreach (SaveFileInfo current in this.files)
			{
				if (num + vector.y >= this.scrollPosition.y && num <= this.scrollPosition.y + outRect.height)
				{
					Rect rect = new Rect(0f, num, vector.x, vector.y);
					if (num2 % 2 == 0)
					{
						Widgets.DrawAltRect(rect);
					}
					GUI.BeginGroup(rect);
					Rect rect2 = new Rect(rect.width - 36f, (rect.height - 36f) / 2f, 36f, 36f);
					if (Widgets.ButtonImage(rect2, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
					{
						FileInfo localFile = current.FileInfo;
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(localFile.Name), delegate
						{
							localFile.Delete();
							this.ReloadFiles();
						}, true, null));
					}
					TooltipHandler.TipRegion(rect2, "DeleteThisSavegame".Translate());
					Text.Font = GameFont.Small;
					Rect rect3 = new Rect(rect2.x - 100f, (rect.height - 36f) / 2f, 100f, 36f);
					if (Widgets.ButtonText(rect3, this.interactButLabel, true, false, true))
					{
						this.DoFileInteraction(Path.GetFileNameWithoutExtension(current.FileInfo.Name));
					}
					Rect rect4 = new Rect(rect3.x - 94f, 0f, 94f, rect.height);
					Dialog_FileList.DrawDateAndVersion(current, rect4);
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = this.FileNameColor(current);
					Rect rect5 = new Rect(8f, 0f, rect4.x - 8f - 4f, rect.height);
					Text.Anchor = TextAnchor.MiddleLeft;
					Text.Font = GameFont.Small;
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current.FileInfo.Name);
					Widgets.Label(rect5, fileNameWithoutExtension.Truncate(rect5.width * 1.8f, null));
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.EndGroup();
				}
				num += vector.y;
				num2++;
			}
			Widgets.EndScrollView();
			if (this.ShouldDoTypeInField)
			{
				this.DoTypeInField(inRect.AtZero());
			}
		}

		protected abstract void DoFileInteraction(string fileName);

		protected abstract void ReloadFiles();

		protected virtual void DoTypeInField(Rect rect)
		{
			GUI.BeginGroup(rect);
			bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
			float y = rect.height - 52f;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.SetNextControlName("MapNameField");
			Rect rect2 = new Rect(5f, y, 400f, 35f);
			string str = Widgets.TextField(rect2, this.typingName);
			if (GenText.IsValidFilename(str))
			{
				this.typingName = str;
			}
			if (!this.focusedNameArea)
			{
				UI.FocusControl("MapNameField", this);
				this.focusedNameArea = true;
			}
			Rect rect3 = new Rect(420f, y, rect.width - 400f - 20f, 35f);
			if (Widgets.ButtonText(rect3, "SaveGameButton".Translate(), true, false, true) || flag)
			{
				if (this.typingName.NullOrEmpty())
				{
					Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput, false);
				}
				else
				{
					this.DoFileInteraction(this.typingName);
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
		}

		protected virtual Color FileNameColor(SaveFileInfo sfi)
		{
			return Dialog_FileList.DefaultFileTextColor;
		}

		public static void DrawDateAndVersion(SaveFileInfo sfi, Rect rect)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(0f, 2f, rect.width, rect.height / 2f);
			GUI.color = SaveFileInfo.UnimportantTextColor;
			Widgets.Label(rect2, sfi.FileInfo.LastWriteTime.ToString("g"));
			Rect rect3 = new Rect(0f, rect2.yMax, rect.width, rect.height / 2f);
			GUI.color = sfi.VersionColor;
			Widgets.Label(rect3, sfi.GameVersion);
			TooltipHandler.TipRegion(rect3, sfi.CompatibilityTip);
			GUI.EndGroup();
		}
	}
}
