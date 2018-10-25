using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld
{
	public class Page_ModsConfig : Page
	{
		public ModMetaData selectedMod;

		private Vector2 modListScrollPosition = Vector2.zero;

		private Vector2 modDescriptionScrollPosition = Vector2.zero;

		private int activeModsWhenOpenedHash = -1;

		private Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();

		protected string filter = string.Empty;

		private const float ModListAreaWidth = 350f;

		private const float ModsListButtonHeight = 30f;

		private const float ModsFolderButHeight = 30f;

		private const float ButtonsGap = 4f;

		private const float UploadRowHeight = 40f;

		private const float PreviewMaxHeight = 300f;

		private const float VersionWidth = 30f;

		private const float ModRowHeight = 26f;

		public Page_ModsConfig()
		{
			this.doCloseButton = true;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			ModLister.RebuildModList();
			this.selectedMod = this.ModsInListOrder().FirstOrDefault<ModMetaData>();
			this.activeModsWhenOpenedHash = ModLister.InstalledModsListHash(true);
		}

		[DebuggerHidden]
		private IEnumerable<ModMetaData> ModsInListOrder()
		{
			foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder)
			{
				yield return mod;
			}
			foreach (ModMetaData mod2 in from x in ModLister.AllInstalledMods
			where !x.Active
			select x into m
			orderby m.VersionCompatible descending
			select m)
			{
				yield return mod2;
			}
		}

		public override void DoWindowContents(Rect rect)
		{
			Rect mainRect = base.GetMainRect(rect, 0f, true);
			GUI.BeginGroup(mainRect);
			Text.Font = GameFont.Small;
			float num = 0f;
			Rect rect2 = new Rect(17f, num, 316f, 30f);
			if (Widgets.ButtonText(rect2, "OpenSteamWorkshop".Translate(), true, false, true))
			{
				SteamUtility.OpenSteamWorkshopPage();
			}
			num += 30f;
			Rect rect3 = new Rect(17f, num, 316f, 30f);
			if (Widgets.ButtonText(rect3, "GetModsFromForum".Translate(), true, false, true))
			{
				Application.OpenURL("http://rimworldgame.com/getmods");
			}
			num += 30f;
			num += 17f;
			this.filter = Widgets.TextField(new Rect(0f, num, 350f, 30f), this.filter);
			num += 30f;
			num += 10f;
			Rect rect4 = new Rect(0f, num, 350f, mainRect.height - num);
			Widgets.DrawMenuSection(rect4);
			float height = (float)ModLister.AllInstalledMods.Count<ModMetaData>() * 26f + 8f;
			Rect rect5 = new Rect(0f, 0f, rect4.width - 16f, height);
			Widgets.BeginScrollView(rect4, ref this.modListScrollPosition, rect5, true);
			Rect rect6 = rect5.ContractedBy(4f);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = rect6.width;
			listing_Standard.Begin(rect6);
			int reorderableGroup = ReorderableWidget.NewGroup(delegate(int from, int to)
			{
				ModsConfig.Reorder(from, to);
			}, ReorderableDirection.Vertical, -1f, null);
			int num2 = 0;
			foreach (ModMetaData current in this.ModsInListOrder())
			{
				this.DoModRow(listing_Standard, current, num2, reorderableGroup);
				num2++;
			}
			int downloadingItemsCount = WorkshopItems.DownloadingItemsCount;
			for (int i = 0; i < downloadingItemsCount; i++)
			{
				this.DoModRowDownloading(listing_Standard, num2);
				num2++;
			}
			listing_Standard.End();
			Widgets.EndScrollView();
			Rect position = new Rect(rect4.xMax + 17f, 0f, mainRect.width - rect4.width - 17f, mainRect.height);
			GUI.BeginGroup(position);
			if (this.selectedMod != null)
			{
				Text.Font = GameFont.Medium;
				Rect rect7 = new Rect(0f, 0f, position.width, 40f);
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect7, this.selectedMod.Name.Truncate(rect7.width, null));
				Text.Anchor = TextAnchor.UpperLeft;
				if (!this.selectedMod.IsCoreMod)
				{
					Rect rect8 = rect7;
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.LowerRight;
					if (!this.selectedMod.VersionCompatible)
					{
						GUI.color = Color.red;
					}
					Widgets.Label(rect8, "ModTargetVersion".Translate(this.selectedMod.TargetVersion));
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					Text.Font = GameFont.Small;
				}
				Rect position2 = new Rect(0f, rect7.yMax, 0f, 20f);
				if (this.selectedMod.previewImage != null)
				{
					position2.width = Mathf.Min((float)this.selectedMod.previewImage.width, position.width);
					position2.height = (float)this.selectedMod.previewImage.height * (position2.width / (float)this.selectedMod.previewImage.width);
					if (position2.height > 300f)
					{
						position2.width *= 300f / position2.height;
						position2.height = 300f;
					}
					position2.x = position.width / 2f - position2.width / 2f;
					GUI.DrawTexture(position2, this.selectedMod.previewImage, ScaleMode.ScaleToFit);
				}
				Text.Font = GameFont.Small;
				float num3 = position2.yMax + 10f;
				if (!this.selectedMod.Author.NullOrEmpty())
				{
					Rect rect9 = new Rect(0f, num3, position.width / 2f, 25f);
					Widgets.Label(rect9, "Author".Translate() + ": " + this.selectedMod.Author);
				}
				if (!this.selectedMod.Url.NullOrEmpty())
				{
					float num4 = Mathf.Min(position.width / 2f, Text.CalcSize(this.selectedMod.Url).x);
					Rect rect10 = new Rect(position.width - num4, num3, num4, 25f);
					Text.WordWrap = false;
					if (Widgets.ButtonText(rect10, this.selectedMod.Url, false, false, true))
					{
						Application.OpenURL(this.selectedMod.Url);
					}
					Text.WordWrap = true;
				}
				WidgetRow widgetRow = new WidgetRow(position.width, num3 + 25f, UIDirection.LeftThenUp, 99999f, 4f);
				if (SteamManager.Initialized && this.selectedMod.OnSteamWorkshop)
				{
					if (widgetRow.ButtonText("Unsubscribe", null, true, false))
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnsubscribe".Translate(this.selectedMod.Name), delegate
						{
							this.selectedMod.enabled = false;
							Workshop.Unsubscribe(this.selectedMod);
							this.Notify_SteamItemUnsubscribed(this.selectedMod.GetPublishedFileId());
						}, true, null));
					}
					if (widgetRow.ButtonText("WorkshopPage".Translate(), null, true, false))
					{
						SteamUtility.OpenWorkshopPage(this.selectedMod.GetPublishedFileId());
					}
				}
				float num5 = num3 + 25f + 24f;
				Rect outRect = new Rect(0f, num5, position.width, position.height - num5 - 40f);
				float width = outRect.width - 16f;
				Rect rect11 = new Rect(0f, 0f, width, Text.CalcHeight(this.selectedMod.Description, width));
				Widgets.BeginScrollView(outRect, ref this.modDescriptionScrollPosition, rect11, true);
				Widgets.Label(rect11, this.selectedMod.Description);
				Widgets.EndScrollView();
				if (Prefs.DevMode && SteamManager.Initialized && this.selectedMod.CanToUploadToWorkshop())
				{
					Rect rect12 = new Rect(0f, position.yMax - 40f, 200f, 40f);
					if (Widgets.ButtonText(rect12, Workshop.UploadButtonLabel(this.selectedMod.GetPublishedFileId()), true, false, true))
					{
						if (!VersionControl.IsWellFormattedVersionString(this.selectedMod.TargetVersion))
						{
							Messages.Message("MessageModNeedsWellFormattedTargetVersion".Translate(VersionControl.CurrentVersionString), MessageTypeDefOf.RejectInput, false);
						}
						else
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSteamWorkshopUpload".Translate(), delegate
							{
								SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
								Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmContentAuthor".Translate(), delegate
								{
									SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
									Workshop.Upload(this.selectedMod);
								}, true, null);
								dialog_MessageBox.buttonAText = "Yes".Translate();
								dialog_MessageBox.buttonBText = "No".Translate();
								dialog_MessageBox.interactionDelay = 6f;
								Find.WindowStack.Add(dialog_MessageBox);
							}, true, null));
						}
					}
				}
			}
			GUI.EndGroup();
			GUI.EndGroup();
		}

		private void DoModRow(Listing_Standard listing, ModMetaData mod, int index, int reorderableGroup)
		{
			Rect rect = listing.GetRect(26f);
			if (mod.Active)
			{
				ReorderableWidget.Reorderable(reorderableGroup, rect, false);
			}
			Action clickAction = null;
			if (mod.Source == ContentSource.SteamWorkshop)
			{
				clickAction = delegate
				{
					SteamUtility.OpenWorkshopPage(mod.GetPublishedFileId());
				};
			}
			ContentSourceUtility.DrawContentSource(rect, mod.Source, clickAction);
			rect.xMin += 28f;
			bool flag = mod == this.selectedMod;
			bool active = mod.Active;
			Rect rect2 = rect;
			if (mod.enabled)
			{
				string text = string.Empty;
				if (mod.Active)
				{
					text = text + "DragToReorder".Translate() + ".\n\n";
				}
				if (!mod.VersionCompatible)
				{
					GUI.color = Color.red;
					if (mod.MadeForNewerVersion)
					{
						text += "ModNotMadeForThisVersion_Newer".Translate();
					}
					else
					{
						text += "ModNotMadeForThisVersion".Translate();
					}
				}
				GUI.color = this.FilteredColor(GUI.color, mod.Name);
				if (!text.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, new TipSignal(text, mod.GetHashCode() * 3311));
				}
				float num = rect2.width - 24f;
				if (mod.Active)
				{
					Rect position = new Rect(rect2.xMax - 48f + 2f, rect2.y, 24f, 24f);
					GUI.DrawTexture(position, TexButton.DragHash);
					num -= 24f;
				}
				Text.Font = GameFont.Small;
				string label = mod.Name.Truncate(num, this.truncatedModNamesCache);
				if (Widgets.CheckboxLabeledSelectable(rect2, label, ref flag, ref active))
				{
					this.selectedMod = mod;
				}
				if (mod.Active && !active && mod.IsCoreMod)
				{
					ModMetaData coreMod = mod;
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisableCoreMod".Translate(), delegate
					{
						coreMod.Active = false;
						this.truncatedModNamesCache.Clear();
					}, false, null));
				}
				else
				{
					mod.Active = active;
					this.truncatedModNamesCache.Clear();
				}
			}
			else
			{
				GUI.color = this.FilteredColor(Color.gray, mod.Name);
				Widgets.Label(rect2, mod.Name);
			}
			GUI.color = Color.white;
		}

		private void DoModRowDownloading(Listing_Standard listing, int index)
		{
			Rect rect = listing.GetRect(26f);
			ContentSourceUtility.DrawContentSource(rect, ContentSource.SteamWorkshop, null);
			rect.xMin += 28f;
			Widgets.Label(rect, "Downloading".Translate() + GenText.MarchingEllipsis(0f));
		}

		public void Notify_ModsListChanged()
		{
			string selModId = this.selectedMod.Identifier;
			this.selectedMod = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData m) => m.Identifier == selModId);
		}

		internal void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid)
		{
			if (this.selectedMod != null && this.selectedMod.Identifier == pfid.ToString())
			{
				this.selectedMod = null;
			}
		}

		public override void PostClose()
		{
			ModsConfig.Save();
			if (this.activeModsWhenOpenedHash != ModLister.InstalledModsListHash(true))
			{
				ModsConfig.RestartFromChangedMods();
			}
		}

		private Color FilteredColor(Color color, string label)
		{
			if (this.filter.NullOrEmpty())
			{
				return color;
			}
			if (label.IndexOf(this.filter, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return color;
			}
			return color * new Color(1f, 1f, 1f, 0.3f);
		}
	}
}
