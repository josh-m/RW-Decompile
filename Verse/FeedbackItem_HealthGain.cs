using System;
using UnityEngine;

namespace Verse
{
	public class FeedbackItem_HealthGain : FeedbackItem
	{
		protected Pawn Healer;

		protected int Amount;

		public FeedbackItem_HealthGain(Vector2 ScreenPos, int Amount, Pawn Healer) : base(ScreenPos)
		{
			this.Amount = Amount;
			this.Healer = Healer;
		}

		public override void FeedbackOnGUI()
		{
			string text = string.Empty;
			if (this.Amount >= 0)
			{
				text = "+";
			}
			else
			{
				text = "-";
			}
			text += this.Amount;
			base.DrawFloatingText(text, Color.red);
		}
	}
}
