using System;
using UnityEngine;

namespace Verse
{
	public class CreditRecord_Role : CreditsEntry
	{
		public string roleKey;

		public string creditee;

		public string extra;

		public CreditRecord_Role()
		{
		}

		public CreditRecord_Role(string roleKey, string creditee, string extra = null)
		{
			this.roleKey = roleKey;
			this.creditee = creditee;
			this.extra = extra;
		}

		public override float DrawHeight(float width)
		{
			return 50f;
		}

		public override void Draw(Rect rect)
		{
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = rect;
			rect2.width /= 2f;
			Widgets.Label(rect2, this.roleKey.Translate());
			Rect rect3 = rect;
			rect3.xMin = rect2.xMax;
			Widgets.Label(rect3, this.creditee);
			if (!this.extra.NullOrEmpty())
			{
				Rect rect4 = rect3;
				rect4.yMin += 28f;
				Text.Font = GameFont.Tiny;
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				Widgets.Label(rect4, this.extra);
				GUI.color = Color.white;
			}
		}
	}
}
