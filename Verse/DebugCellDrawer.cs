using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public sealed class DebugCellDrawer
	{
		private const int DefaultLifespanTicks = 50;

		private List<DebugCell> debugCells = new List<DebugCell>();

		private List<DebugLine> debugLines = new List<DebugLine>();

		public void FlashCell(IntVec3 c, float colorPct = 0f, string text = null)
		{
			DebugCell debugCell = new DebugCell();
			debugCell.c = c;
			debugCell.displayString = text;
			debugCell.colorPct = colorPct;
			debugCell.ticksLeft = 50;
			this.debugCells.Add(debugCell);
		}

		public void FlashCell(IntVec3 c, Material mat, string text = null)
		{
			DebugCell debugCell = new DebugCell();
			debugCell.c = c;
			debugCell.displayString = text;
			debugCell.customMat = mat;
			debugCell.ticksLeft = 50;
			this.debugCells.Add(debugCell);
		}

		public void FlashLine(IntVec3 a, IntVec3 b)
		{
			DebugLine item = new DebugLine(a.ToVector3Shifted(), b.ToVector3Shifted());
			item.TicksLeft = 50;
			this.debugLines.Add(item);
		}

		public void DebugDrawerUpdate()
		{
			for (int i = 0; i < this.debugCells.Count; i++)
			{
				this.debugCells[i].Draw();
			}
			for (int j = 0; j < this.debugLines.Count; j++)
			{
				this.debugLines[j].Draw();
			}
		}

		public void DebugDrawerTick()
		{
			for (int i = this.debugCells.Count - 1; i >= 0; i--)
			{
				DebugCell debugCell = this.debugCells[i];
				debugCell.ticksLeft--;
				if (debugCell.ticksLeft <= 0)
				{
					this.debugCells.RemoveAt(i);
				}
			}
			for (int j = this.debugLines.Count - 1; j >= 0; j--)
			{
				this.debugLines[j] = new DebugLine(this.debugLines[j].a, this.debugLines[j].b, this.debugLines[j].TicksLeft - 1);
				if (this.debugLines[j].TicksLeft <= 0)
				{
					this.debugLines.RemoveAt(j);
				}
			}
		}

		public void DebugDrawerOnGUI()
		{
			if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
			{
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleCenter;
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				for (int i = 0; i < this.debugCells.Count; i++)
				{
					this.debugCells[i].OnGUI();
				}
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}
}
