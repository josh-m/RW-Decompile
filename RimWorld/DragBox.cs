using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class DragBox
	{
		private const float DragBoxMinDiagonal = 0.5f;

		public bool active;

		public Vector3 start;

		public float LeftX
		{
			get
			{
				return Math.Min(this.start.x, UI.MouseMapPosition().x);
			}
		}

		public float RightX
		{
			get
			{
				return Math.Max(this.start.x, UI.MouseMapPosition().x);
			}
		}

		public float BotZ
		{
			get
			{
				return Math.Min(this.start.z, UI.MouseMapPosition().z);
			}
		}

		public float TopZ
		{
			get
			{
				return Math.Max(this.start.z, UI.MouseMapPosition().z);
			}
		}

		public Rect ScreenRect
		{
			get
			{
				Vector2 vector = this.start.MapToUIPosition();
				Vector2 mousePosition = Event.current.mousePosition;
				if (mousePosition.x < vector.x)
				{
					float x = mousePosition.x;
					mousePosition.x = vector.x;
					vector.x = x;
				}
				if (mousePosition.y < vector.y)
				{
					float y = mousePosition.y;
					mousePosition.y = vector.y;
					vector.y = y;
				}
				return new Rect
				{
					xMin = vector.x,
					xMax = mousePosition.x,
					yMin = vector.y,
					yMax = mousePosition.y
				};
			}
		}

		public bool IsValid
		{
			get
			{
				return (this.start - UI.MouseMapPosition()).magnitude > 0.5f;
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

		public bool Contains(Thing t)
		{
			if (t is Pawn)
			{
				return this.Contains((t as Pawn).Drawer.DrawPos);
			}
			CellRect.CellRectIterator iterator = t.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (this.Contains(current.ToVector3Shifted()))
				{
					return true;
				}
				iterator.MoveNext();
			}
			return false;
		}

		public bool Contains(Vector3 v)
		{
			return v.x + 0.5f > this.LeftX && v.x - 0.5f < this.RightX && v.z + 0.5f > this.BotZ && v.z - 0.5f < this.TopZ;
		}
	}
}
