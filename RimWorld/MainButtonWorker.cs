using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class MainButtonWorker
	{
		public MainButtonDef def;

		public virtual float ButtonBarPercent
		{
			get
			{
				return 0f;
			}
		}

		public abstract void Activate();

		public virtual void InterfaceTryActivate()
		{
			if (TutorSystem.TutorialMode && this.def.canBeTutorDenied && Find.MainTabsRoot.OpenTab != this.def && !TutorSystem.AllowAction("MainTab-" + this.def.defName + "-Open"))
			{
				return;
			}
			this.Activate();
		}

		public virtual void DoButton(Rect rect)
		{
			Text.Font = GameFont.Small;
			string labelCap = this.def.LabelCap;
			if ((!this.def.validWithoutMap || this.def == MainButtonDefOf.World) && Find.VisibleMap == null)
			{
				Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
				{
					Event.current.Use();
				}
			}
			else
			{
				SoundDef mouseoverCategory = SoundDefOf.MouseoverCategory;
				if (Widgets.ButtonTextSubtle(rect, labelCap, this.ButtonBarPercent, -1f, mouseoverCategory))
				{
					this.InterfaceTryActivate();
				}
				if (Find.MainTabsRoot.OpenTab != this.def && !Find.WindowStack.NonImmediateDialogWindowOpen)
				{
					UIHighlighter.HighlightOpportunity(rect, this.def.cachedHighlightTagClosed);
				}
				if (!this.def.description.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect, this.def.description);
				}
			}
		}
	}
}
