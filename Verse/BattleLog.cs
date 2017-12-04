using System;
using System.Collections.Generic;

namespace Verse
{
	public class BattleLog : IExposable
	{
		private List<LogEntry> rawEntries = new List<LogEntry>();

		private const int BattleHistoryLength = 200;

		public List<LogEntry> RawEntries
		{
			get
			{
				return this.rawEntries;
			}
		}

		public void Add(LogEntry entry)
		{
			this.rawEntries.Insert(0, entry);
			this.ReduceToCapacity();
		}

		private void ReduceToCapacity()
		{
			if (this.rawEntries.Count > 200)
			{
				this.rawEntries.RemoveRange(200, this.rawEntries.Count - 200);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<LogEntry>(ref this.rawEntries, "rawEntries", LookMode.Deep, new object[0]);
		}

		public bool AnyEntryConcerns(Pawn p)
		{
			for (int i = 0; i < this.rawEntries.Count; i++)
			{
				if (this.rawEntries[i].Concerns(p))
				{
					return true;
				}
			}
			return false;
		}

		public void Notify_PawnDiscarded(Pawn p, bool silentlyRemoveReferences)
		{
			for (int i = this.rawEntries.Count - 1; i >= 0; i--)
			{
				if (this.rawEntries[i].Concerns(p))
				{
					if (!silentlyRemoveReferences)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Discarding pawn ",
							p,
							", but he is referenced by a battle log entry ",
							this.rawEntries[i],
							"."
						}));
					}
					this.rawEntries.Remove(this.rawEntries[i]);
				}
			}
		}
	}
}
