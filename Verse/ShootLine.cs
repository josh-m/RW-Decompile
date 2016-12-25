using System;

namespace Verse
{
	public struct ShootLine
	{
		private IntVec3 source;

		private IntVec3 dest;

		public IntVec3 Source
		{
			get
			{
				return this.source;
			}
		}

		public IntVec3 Dest
		{
			get
			{
				return this.dest;
			}
		}

		public ShootLine(IntVec3 source, IntVec3 dest)
		{
			this.source = source;
			this.dest = dest;
		}

		public void ChangeDestToMissWild()
		{
			if ((double)(this.dest - this.source).LengthHorizontal < 2.5)
			{
				IntVec3 b = IntVec3.FromVector3((this.dest - this.source).ToVector3().normalized * 2f);
				this.dest += b;
			}
			this.dest = this.dest.RandomAdjacentCell8Way();
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.source,
				"->",
				this.dest,
				")"
			});
		}
	}
}
