using System;
using Verse.Sound;

namespace Verse
{
	public static class GenDrop
	{
		public static bool TryDropSpawn(Thing thing, IntVec3 dropCell, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (!dropCell.InBounds())
			{
				Log.Error(string.Concat(new object[]
				{
					"Dropped ",
					thing,
					" out of bounds at ",
					dropCell
				}));
				resultingThing = null;
				return false;
			}
			if (thing.def.destroyOnDrop)
			{
				thing.Destroy(DestroyMode.Vanish);
				resultingThing = null;
				return true;
			}
			if (thing.def.soundDrop != null)
			{
				thing.def.soundDrop.PlayOneShot(dropCell);
			}
			return GenPlace.TryPlaceThing(thing, dropCell, mode, out resultingThing, placedAction);
		}
	}
}
