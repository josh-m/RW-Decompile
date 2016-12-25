using System;
using UnityEngine;

namespace Verse
{
	public sealed class MapInfo : IExposable
	{
		private IntVec3 sizeInt = default(IntVec3);

		public IntVec2 worldCoords = IntVec2.Invalid;

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

		public int PowerOfTwoOverMapSize
		{
			get
			{
				int num = Mathf.Max(this.sizeInt.x, this.sizeInt.z);
				int i;
				for (i = 1; i <= num; i *= 2)
				{
				}
				return i;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.sizeInt, "size", default(IntVec3), false);
			Scribe_Values.LookValue<IntVec2>(ref this.worldCoords, "worldCoords", default(IntVec2), false);
		}
	}
}
