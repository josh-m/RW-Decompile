using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_LifeStage : PawnColumnWorker_Icon
	{
		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn.ageTracker.CurLifeStageRace.GetIcon(pawn);
		}

		protected override string GetIconTip(Pawn pawn)
		{
			return pawn.ageTracker.CurLifeStage.LabelCap;
		}

		protected override Vector2 GetIconSize(Pawn pawn)
		{
			return new Vector2(24f, 24f);
		}
	}
}
