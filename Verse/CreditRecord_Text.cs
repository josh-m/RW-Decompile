using System;
using UnityEngine;

namespace Verse
{
	public class CreditRecord_Text : CreditsEntry
	{
		public string text;

		public TextAnchor anchor;

		public CreditRecord_Text()
		{
		}

		public CreditRecord_Text(string text, TextAnchor anchor = TextAnchor.UpperLeft)
		{
			this.text = text;
			this.anchor = anchor;
		}

		public override float DrawHeight(float width)
		{
			return Text.CalcHeight(this.text, width);
		}

		public override void Draw(Rect r)
		{
			Text.Anchor = this.anchor;
			Widgets.Label(r, this.text);
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
