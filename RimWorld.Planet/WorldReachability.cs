using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldReachability
	{
		private int[] fields;

		private int nextFieldID;

		private int impassableFieldID;

		private int minValidFieldID;

		public WorldReachability()
		{
			this.fields = new int[Find.WorldGrid.TilesCount];
			this.nextFieldID = 1;
			this.InvalidateAllFields();
		}

		public void ClearCache()
		{
			this.InvalidateAllFields();
		}

		public bool CanReach(Caravan c, int tile)
		{
			return this.CanReach(c.Tile, tile);
		}

		public bool CanReach(int startTile, int destTile)
		{
			if (startTile < 0 || startTile >= this.fields.Length || destTile < 0 || destTile >= this.fields.Length)
			{
				return false;
			}
			if (this.fields[startTile] == this.impassableFieldID || this.fields[destTile] == this.impassableFieldID)
			{
				return false;
			}
			if (this.IsValidField(this.fields[startTile]) || this.IsValidField(this.fields[destTile]))
			{
				return this.fields[startTile] == this.fields[destTile];
			}
			this.FloodFillAt(startTile);
			return this.fields[startTile] != this.impassableFieldID && this.fields[startTile] == this.fields[destTile];
		}

		private void InvalidateAllFields()
		{
			if (this.nextFieldID == 2147483646)
			{
				this.nextFieldID = 1;
			}
			this.minValidFieldID = this.nextFieldID;
			this.impassableFieldID = this.nextFieldID;
			this.nextFieldID++;
		}

		private bool IsValidField(int fieldID)
		{
			return fieldID >= this.minValidFieldID;
		}

		private void FloodFillAt(int tile)
		{
			World world = Find.World;
			if (world.Impassable(tile))
			{
				this.fields[tile] = this.impassableFieldID;
				return;
			}
			WorldFloodFiller.FloodFill(tile, (int x) => !world.Impassable(x), delegate(int x)
			{
				this.fields[x] = this.nextFieldID;
			}, 2147483647);
			this.nextFieldID++;
		}
	}
}
