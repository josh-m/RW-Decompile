using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDebugDrawer
	{
		private const int DefaultLifespanTicks = 50;

		private const float MaxDistToCameraToDisplayLabel = 39f;

		private List<DebugTile> debugTiles = new List<DebugTile>();

		private List<DebugWorldLine> debugLines = new List<DebugWorldLine>();

		public void FlashTile(int tile, float colorPct = 0f, string text = null)
		{
			DebugTile debugTile = new DebugTile();
			debugTile.tile = tile;
			debugTile.displayString = text;
			debugTile.colorPct = colorPct;
			debugTile.ticksLeft = 50;
			this.debugTiles.Add(debugTile);
		}

		public void FlashTile(int tile, Material mat, string text = null)
		{
			DebugTile debugTile = new DebugTile();
			debugTile.tile = tile;
			debugTile.displayString = text;
			debugTile.customMat = mat;
			debugTile.ticksLeft = 50;
			this.debugTiles.Add(debugTile);
		}

		public void FlashLine(Vector3 a, Vector3 b, bool onPlanetSurface = false)
		{
			DebugWorldLine debugWorldLine = new DebugWorldLine(a, b, onPlanetSurface);
			debugWorldLine.TicksLeft = 50;
			this.debugLines.Add(debugWorldLine);
		}

		public void FlashLine(int tileA, int tileB)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			Vector3 tileCenter = worldGrid.GetTileCenter(tileA);
			Vector3 tileCenter2 = worldGrid.GetTileCenter(tileB);
			DebugWorldLine debugWorldLine = new DebugWorldLine(tileCenter, tileCenter2, true);
			debugWorldLine.TicksLeft = 50;
			this.debugLines.Add(debugWorldLine);
		}

		public void WorldDebugDrawerUpdate()
		{
			for (int i = 0; i < this.debugTiles.Count; i++)
			{
				this.debugTiles[i].Draw();
			}
			for (int j = 0; j < this.debugLines.Count; j++)
			{
				this.debugLines[j].Draw();
			}
		}

		public void WorldDebugDrawerTick()
		{
			for (int i = this.debugTiles.Count - 1; i >= 0; i--)
			{
				DebugTile debugTile = this.debugTiles[i];
				debugTile.ticksLeft--;
				if (debugTile.ticksLeft <= 0)
				{
					this.debugTiles.RemoveAt(i);
				}
			}
			for (int j = this.debugLines.Count - 1; j >= 0; j--)
			{
				DebugWorldLine debugWorldLine = this.debugLines[j];
				debugWorldLine.ticksLeft--;
				if (debugWorldLine.ticksLeft <= 0)
				{
					this.debugLines.RemoveAt(j);
				}
			}
		}

		public void WorldDebugDrawerOnGUI()
		{
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			for (int i = 0; i < this.debugTiles.Count; i++)
			{
				if (this.debugTiles[i].DistanceToCamera <= 39f)
				{
					this.debugTiles[i].OnGUI();
				}
			}
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
