using System;
using UnityEngine;

namespace Verse
{
	internal sealed class DebugCell
	{
		public IntVec3 c;

		public string displayString;

		public float colorPct;

		public int ticksLeft;

		public Material customMat;

		public void Draw()
		{
			if (this.customMat != null)
			{
				CellRenderer.RenderCell(this.c, this.customMat);
			}
			else
			{
				CellRenderer.RenderCell(this.c, this.colorPct);
			}
		}

		public void OnGUI()
		{
			Vector2 vector = this.c.ToScreenPosition();
			Rect rect = new Rect(vector.x - 20f, vector.y - 20f, 40f, 40f);
			if (this.displayString != null)
			{
				Widgets.Label(rect, this.displayString);
			}
		}
	}
}
