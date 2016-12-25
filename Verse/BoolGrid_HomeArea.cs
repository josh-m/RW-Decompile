using RimWorld;
using System;

namespace Verse
{
	public class BoolGrid_HomeArea : BoolGrid
	{
		public BoolGrid_HomeArea(MapMeshFlag changeType) : base(changeType)
		{
		}

		public override void Set(IntVec3 c, bool value)
		{
			base.Set(c, value);
			ListerFilthInHomeArea.Notify_HomeAreaChanged(c);
		}
	}
}
