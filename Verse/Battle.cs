using System;
using System.Collections.Generic;

namespace Verse
{
	public class Battle : IExposable, ILoadReferenceable
	{
		public const int TicksForBattleExit = 5000;

		private List<LogEntry> entries = new List<LogEntry>();

		private int loadID;

		public static Battle Create()
		{
			return new Battle
			{
				loadID = Find.UniqueIDsManager.GetNextBattleID()
			};
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Collections.Look<LogEntry>(ref this.entries, "entries", LookMode.Deep, new object[0]);
		}

		public string GetUniqueLoadID()
		{
			return "Battle_" + this.loadID;
		}
	}
}
