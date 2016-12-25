using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDragBox
	{
		private const float DragBoxMinDiagonal = 7f;

		public bool active;

		public Vector2 start;

		public float LeftX
		{
			get
			{
				return Math.Min(this.start.x, UI.MousePositionOnUIInverted.x);
			}
		}

		public float RightX
		{
			get
			{
				return Math.Max(this.start.x, UI.MousePositionOnUIInverted.x);
			}
		}

		public float BotZ
		{
			get
			{
				return Math.Min(this.start.y, UI.MousePositionOnUIInverted.y);
			}
		}

		public float TopZ
		{
			get
			{
				return Math.Max(this.start.y, UI.MousePositionOnUIInverted.y);
			}
		}

		public Rect ScreenRect
		{
			get
			{
				return new Rect(this.LeftX, this.BotZ, this.RightX - this.LeftX, this.TopZ - this.BotZ);
			}
		}

		public float Diagonal
		{
			get
			{
				return (this.start - new Vector2(UI.MousePositionOnUIInverted.x, UI.MousePositionOnUIInverted.y)).magnitude;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.Diagonal > 7f;
			}
		}

		public bool IsValidAndActive
		{
			get
			{
				return this.active && this.IsValid;
			}
		}

		public void DragBoxOnGUI()
		{
			if (this.IsValidAndActive)
			{
				Widgets.DrawBox(this.ScreenRect, 2);
			}
		}

		public bool Contains(WorldObject o)
		{
			return this.Contains(o.ScreenPos());
		}

		public bool Contains(Vector2 screenPoint)
		{
			return screenPoint.x + 0.5f > this.LeftX && screenPoint.x - 0.5f < this.RightX && screenPoint.y + 0.5f > this.BotZ && screenPoint.y - 0.5f < this.TopZ;
		}
	}
}
