using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Page_SelectStoryteller : Page
	{
		private StorytellerDef storyteller;

		private DifficultyDef difficulty;

		public override string PageTitle
		{
			get
			{
				return "ChooseAIStoryteller".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.storyteller = (from d in DefDatabase<StorytellerDef>.AllDefs
			where d.listVisible
			orderby d.listOrder
			select d).First<StorytellerDef>();
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			Rect mainRect = base.GetMainRect(rect, 0f, false);
			StorytellerUI.DrawStorytellerSelectionInterface(mainRect, ref this.storyteller, ref this.difficulty);
			base.DoBottomButtons(rect, null, null, null, true);
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (this.difficulty == null)
			{
				if (!Prefs.DevMode)
				{
					Messages.Message("MustChooseDifficulty".Translate(), MessageSound.RejectInput);
					return false;
				}
				Messages.Message("Difficulty has been automatically selected (debug mode only)", MessageSound.Silent);
				this.difficulty = DifficultyDefOf.Hard;
			}
			Current.Game.storyteller = new Storyteller(this.storyteller, this.difficulty);
			return true;
		}
	}
}
