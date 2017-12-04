using RimWorld.Planet;
using System;

namespace Verse
{
	public sealed class MapInfo : IExposable
	{
		private IntVec3 sizeInt;

		public MapParent parent;

		public int Tile
		{
			get
			{
				return this.parent.Tile;
			}
		}

		public int NumCells
		{
			get
			{
				return this.Size.x * this.Size.y * this.Size.z;
			}
		}

		public IntVec3 Size
		{
			get
			{
				return this.sizeInt;
			}
			set
			{
				this.sizeInt = value;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<IntVec3>(ref this.sizeInt, "size", default(IntVec3), false);
			Scribe_References.Look<MapParent>(ref this.parent, "parent", false);
		}
	}
}
