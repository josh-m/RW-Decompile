using System;
using Verse;

namespace RimWorld
{
	public static class SlotGroupUtility
	{
		public static void Notify_TakingThing(Thing t)
		{
			if (!t.Spawned)
			{
				return;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			if (slotGroup != null && slotGroup.parent != null)
			{
				slotGroup.parent.Notify_LostThing(t);
			}
		}
	}
}
