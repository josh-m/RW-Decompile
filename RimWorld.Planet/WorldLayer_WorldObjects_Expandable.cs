using System;

namespace RimWorld.Planet
{
	public class WorldLayer_WorldObjects_Expandable : WorldLayer_WorldObjects
	{
		protected override float Alpha
		{
			get
			{
				return 1f - ExpandableWorldObjectsUtility.TransitionPct;
			}
		}

		protected override bool ShouldSkip(WorldObject worldObject)
		{
			return !worldObject.def.expandingIcon;
		}
	}
}
