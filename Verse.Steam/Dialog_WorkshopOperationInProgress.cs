using Steamworks;
using System;
using UnityEngine;

namespace Verse.Steam
{
	internal class Dialog_WorkshopOperationInProgress : Window
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600f, 400f);
			}
		}

		public Dialog_WorkshopOperationInProgress()
		{
			this.forcePause = true;
			this.closeOnEscapeKey = false;
			this.absorbInputAroundWindow = true;
			this.preventDrawTutor = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			EItemUpdateStatus eItemUpdateStatus;
			float num;
			Workshop.GetUpdateStatus(out eItemUpdateStatus, out num);
			WorkshopInteractStage curStage = Workshop.CurStage;
			if (curStage == WorkshopInteractStage.None && eItemUpdateStatus == EItemUpdateStatus.k_EItemUpdateStatusInvalid)
			{
				this.Close(true);
				return;
			}
			string text = string.Empty;
			if (curStage != WorkshopInteractStage.None)
			{
				text += curStage.GetLabel();
				text += "\n\n";
			}
			if (eItemUpdateStatus != EItemUpdateStatus.k_EItemUpdateStatusInvalid)
			{
				text += eItemUpdateStatus.GetLabel();
				if (num > 0f)
				{
					text = text + " (" + num.ToStringPercent() + ")";
				}
				text += GenText.MarchingEllipsis(0f);
			}
			Widgets.Label(inRect, text);
		}

		public static void CloseAll()
		{
			Dialog_WorkshopOperationInProgress dialog_WorkshopOperationInProgress = Find.WindowStack.WindowOfType<Dialog_WorkshopOperationInProgress>();
			if (dialog_WorkshopOperationInProgress != null)
			{
				dialog_WorkshopOperationInProgress.Close(true);
			}
		}
	}
}
