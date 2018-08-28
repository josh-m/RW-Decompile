using System;
using Verse;

namespace RimWorld
{
	public static class MainTabWindowUtility
	{
		public static void NotifyAllPawnTables_PawnsChanged()
		{
			if (Find.WindowStack == null)
			{
				return;
			}
			WindowStack windowStack = Find.WindowStack;
			for (int i = 0; i < windowStack.Count; i++)
			{
				MainTabWindow_PawnTable mainTabWindow_PawnTable = windowStack[i] as MainTabWindow_PawnTable;
				if (mainTabWindow_PawnTable != null)
				{
					mainTabWindow_PawnTable.Notify_PawnsChanged();
				}
			}
		}
	}
}
