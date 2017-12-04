using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class PawnUIOverlay
	{
		private Pawn pawn;

		private const float PawnLabelOffsetY = -0.6f;

		private const int PawnStatBarWidth = 32;

		private const float ActivityIconSize = 13f;

		private const float ActivityIconOffsetY = 12f;

		public PawnUIOverlay(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void DrawPawnGUIOverlay()
		{
			if (!this.pawn.Spawned || this.pawn.Map.fogGrid.IsFogged(this.pawn.Position))
			{
				return;
			}
			if (!this.pawn.RaceProps.Humanlike)
			{
				AnimalNameDisplayMode animalNameMode = Prefs.AnimalNameMode;
				if (animalNameMode == AnimalNameDisplayMode.None)
				{
					return;
				}
				if (animalNameMode != AnimalNameDisplayMode.TameAll)
				{
					if (animalNameMode == AnimalNameDisplayMode.TameNamed)
					{
						if (this.pawn.Name == null || this.pawn.Name.Numerical)
						{
							return;
						}
					}
				}
				else if (this.pawn.Name == null)
				{
					return;
				}
			}
			Vector2 pos = GenMapUI.LabelDrawPosFor(this.pawn, -0.6f);
			GenMapUI.DrawPawnLabel(this.pawn, pos, 1f, 9999f, null, GameFont.Tiny, true, true);
			if (this.pawn.CanTradeNow)
			{
				this.pawn.Map.overlayDrawer.DrawOverlay(this.pawn, OverlayTypes.QuestionMark);
			}
		}
	}
}
