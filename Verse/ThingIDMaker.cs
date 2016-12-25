using System;

namespace Verse
{
	public static class ThingIDMaker
	{
		public static void GiveIDTo(Thing t)
		{
			if (!t.def.HasThingIDNumber)
			{
				return;
			}
			if (t.thingIDNumber != -1)
			{
				Log.Error(string.Concat(new object[]
				{
					"Giving ID to ",
					t,
					" which already has id ",
					t.thingIDNumber
				}));
			}
			t.thingIDNumber = Find.World.uniqueIDsManager.GetNextThingID();
		}
	}
}
