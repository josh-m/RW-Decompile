using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_GameStartDialog : ScenPart
	{
		private string text;

		private string textKey;

		private SoundDef closeSound;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 5f);
			this.text = Widgets.TextArea(scenPartRect, this.text);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<string>(ref this.text, "text", null, false);
			Scribe_Values.LookValue<string>(ref this.textKey, "textKey", null, false);
			Scribe_Defs.LookDef<SoundDef>(ref this.closeSound, "closeSound");
		}

		public override void PostGameStart()
		{
			if (Find.GameInitData.startedFromEntry)
			{
				Find.MusicManagerPlay.disabled = true;
				Find.WindowStack.Notify_GameStartDialogOpened();
				DiaNode diaNode = new DiaNode((!this.text.NullOrEmpty()) ? this.text : this.textKey.Translate());
				DiaOption diaOption = new DiaOption();
				diaOption.resolveTree = true;
				diaOption.clickSound = null;
				diaNode.options.Add(diaOption);
				Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, false, false);
				dialog_NodeTree.soundClose = ((this.closeSound == null) ? SoundDefOf.GameStartSting : this.closeSound);
				dialog_NodeTree.closeAction = delegate
				{
					Find.MusicManagerPlay.ForceSilenceFor(7f);
					Find.MusicManagerPlay.disabled = false;
					Find.WindowStack.Notify_GameStartDialogClosed();
					Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
					TutorSystem.Notify_Event("GameStartDialogClosed");
				};
				Find.WindowStack.Add(dialog_NodeTree);
			}
		}
	}
}
