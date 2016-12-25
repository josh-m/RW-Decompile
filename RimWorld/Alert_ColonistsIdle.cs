using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistsIdle : Alert_Medium
	{
		public const int MinDaysPassed = 1;

		private IEnumerable<Pawn> IdleColonists
		{
			get
			{
				return from p in Find.MapPawns.FreeColonistsSpawned
				where p.mindState.IsIdle
				select p;
			}
		}

		public override string FullLabel
		{
			get
			{
				return string.Format("ColonistsIdle".Translate(), this.IdleColonists.Count<Pawn>().ToStringCached());
			}
		}

		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.IdleColonists)
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return string.Format("ColonistsIdleDesc".Translate(), stringBuilder.ToString());
			}
		}

		public override AlertReport Report
		{
			get
			{
				if (GenDate.DaysPassed < 1)
				{
					return AlertReport.Inactive;
				}
				return this.IdleColonists.FirstOrDefault<Pawn>();
			}
		}
	}
}
