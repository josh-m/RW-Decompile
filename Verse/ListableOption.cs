using System;
using UnityEngine;

namespace Verse
{
	public class ListableOption
	{
		public string label;

		public Action action;

		private string uiHighlightTag;

		public float minHeight = 45f;

		public ListableOption(string label, Action action, string uiHighlightTag = null)
		{
			this.label = label;
			this.action = action;
			this.uiHighlightTag = uiHighlightTag;
		}

		public virtual float DrawOption(Vector2 pos, float width)
		{
			float b = Text.CalcHeight(this.label, width);
			float num = Mathf.Max(this.minHeight, b);
			Rect rect = new Rect(pos.x, pos.y, width, num);
			if (Widgets.ButtonText(rect, this.label, true, true, true))
			{
				this.action();
			}
			if (this.uiHighlightTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, this.uiHighlightTag);
			}
			return num;
		}
	}
}
