using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_FactionDuringLanding : Window
	{
		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1010f, 684f);
			}
		}

		public Dialog_FactionDuringLanding()
		{
			this.doCloseButton = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			FactionUIUtility.DoWindowContents(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - this.CloseButSize.y), ref this.scrollPosition, ref this.scrollViewHeight);
		}
	}
}
