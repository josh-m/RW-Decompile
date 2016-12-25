using System;
using UnityEngine;

namespace Verse
{
	public class FeedbackItem_FoodGain : FeedbackItem
	{
		protected int Amount;

		public FeedbackItem_FoodGain(Vector2 ScreenPos, int Amount) : base(ScreenPos)
		{
			this.Amount = Amount;
		}

		public override void FeedbackOnGUI()
		{
			string str = this.Amount + " food";
			base.DrawFloatingText(str, Color.yellow);
		}
	}
}
