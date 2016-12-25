using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Training : ITab
	{
		public override bool IsVisible
		{
			get
			{
				return base.SelPawn.training != null && base.SelPawn.Faction == Faction.OfPlayer;
			}
		}

		public ITab_Pawn_Training()
		{
			this.size = new Vector2(300f, 450f);
			this.labelKey = "TabTraining";
			this.tutorTag = "Training";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(17f);
			rect.yMin += 10f;
			TrainingCardUtility.DrawTrainingCard(rect, base.SelPawn);
		}
	}
}
