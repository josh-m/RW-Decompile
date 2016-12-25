using System;
using Verse;

namespace RimWorld
{
	public static class AutoHomeAreaMaker
	{
		private const int BorderWidth = 4;

		private static bool ShouldAdd()
		{
			return Find.PlaySettings.autoHomeArea && Current.ProgramState == ProgramState.MapPlaying;
		}

		public static void Notify_BuildingSpawned(Thing b)
		{
			if (!AutoHomeAreaMaker.ShouldAdd() || !b.def.building.expandHomeArea || b.Faction != Faction.OfPlayer)
			{
				return;
			}
			CellRect cellRect = new CellRect(b.Position.x - b.RotatedSize.x / 2 - 4, b.Position.z - b.RotatedSize.z / 2 - 4, b.RotatedSize.x + 8, b.RotatedSize.z + 8);
			cellRect.ClipInsideMap();
			foreach (IntVec3 current in cellRect)
			{
				Find.AreaHome.Set(current);
			}
		}

		public static void Notify_ZoneCellAdded(IntVec3 c)
		{
			if (!AutoHomeAreaMaker.ShouldAdd())
			{
				return;
			}
			CellRect.CellRectIterator iterator = CellRect.CenteredOn(c, 4).ClipInsideMap().GetIterator();
			while (!iterator.Done())
			{
				Find.AreaHome.Set(iterator.Current);
				iterator.MoveNext();
			}
		}
	}
}
