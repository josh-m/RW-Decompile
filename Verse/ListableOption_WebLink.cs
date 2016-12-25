using System;
using UnityEngine;

namespace Verse
{
	public class ListableOption_WebLink : ListableOption
	{
		public Texture2D image;

		public string url;

		private static readonly Vector2 Imagesize = new Vector2(24f, 18f);

		public ListableOption_WebLink(string label, Texture2D image) : base(label, null, null)
		{
			this.minHeight = 24f;
			this.image = image;
		}

		public ListableOption_WebLink(string label, string url, Texture2D image) : this(label, image)
		{
			this.url = url;
		}

		public ListableOption_WebLink(string label, Action action, Texture2D image) : this(label, image)
		{
			this.action = action;
		}

		public override float DrawOption(Vector2 pos, float width)
		{
			float num = width - ListableOption_WebLink.Imagesize.x - 3f;
			float num2 = Text.CalcHeight(this.label, num);
			float num3 = Mathf.Max(this.minHeight, num2);
			Rect rect = new Rect(pos.x, pos.y, width, num3);
			GUI.color = Color.white;
			if (this.image != null)
			{
				Rect position = new Rect(pos.x, pos.y + num3 / 2f - ListableOption_WebLink.Imagesize.y / 2f, ListableOption_WebLink.Imagesize.x, ListableOption_WebLink.Imagesize.y);
				if (Mouse.IsOver(rect))
				{
					GUI.color = Widgets.MouseoverOptionColor;
				}
				GUI.DrawTexture(position, this.image);
			}
			Rect rect2 = new Rect(rect.xMax - num, pos.y, num, num2);
			Widgets.Label(rect2, this.label);
			GUI.color = Color.white;
			if (Widgets.ButtonInvisible(rect, true))
			{
				if (this.action != null)
				{
					this.action();
				}
				else
				{
					Application.OpenURL(this.url);
				}
			}
			return num3;
		}
	}
}
