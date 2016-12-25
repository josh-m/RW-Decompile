using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_AssignBuildingOwner : Window
	{
		private const float EntryHeight = 35f;

		private IAssignableBuilding assignable;

		private Vector2 scrollPosition;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(620f, 500f);
			}
		}

		public Dialog_AssignBuildingOwner(IAssignableBuilding assignable)
		{
			this.assignable = assignable;
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.doCloseX = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect);
			outRect.yMin += 20f;
			outRect.yMax -= 40f;
			outRect.width -= 16f;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)this.assignable.AssigningCandidates.Count<Pawn>() * 35f + 100f);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
			float num = 0f;
			bool flag = false;
			foreach (Pawn current in this.assignable.AssignedPawns)
			{
				flag = true;
				Rect rect = new Rect(0f, num, viewRect.width * 0.6f, 32f);
				Widgets.Label(rect, current.LabelCap);
				rect.x = rect.xMax;
				rect.width = viewRect.width * 0.4f;
				if (Widgets.ButtonText(rect, "BuildingUnassign".Translate(), true, false, true))
				{
					this.assignable.TryUnassignPawn(current);
					SoundDefOf.Click.PlayOneShotOnCamera();
					return;
				}
				num += 35f;
			}
			if (flag)
			{
				num += 15f;
			}
			foreach (Pawn current2 in this.assignable.AssigningCandidates)
			{
				if (!this.assignable.AssignedPawns.Contains(current2))
				{
					Rect rect2 = new Rect(0f, num, viewRect.width * 0.6f, 32f);
					Widgets.Label(rect2, current2.LabelCap);
					rect2.x = rect2.xMax;
					rect2.width = viewRect.width * 0.4f;
					if (Widgets.ButtonText(rect2, "BuildingAssign".Translate(), true, false, true))
					{
						this.assignable.TryAssignPawn(current2);
						if (this.assignable.MaxAssignedPawnsCount == 1)
						{
							this.Close(true);
						}
						else
						{
							SoundDefOf.Click.PlayOneShotOnCamera();
						}
						return;
					}
					num += 35f;
				}
			}
			Widgets.EndScrollView();
		}
	}
}
