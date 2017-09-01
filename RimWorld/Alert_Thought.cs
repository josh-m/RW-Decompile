using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class Alert_Thought : Alert
	{
		protected string explanationKey;

		private static List<Thought> tmpThoughts = new List<Thought>();

		protected abstract ThoughtDef Thought
		{
			get;
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> AffectedPawns()
		{
			foreach (Pawn p in PawnsFinder.AllMaps_FreeColonists)
			{
				if (p.Dead)
				{
					Log.Error("Dead pawn in PawnsFinder.AllMaps_FreeColonists:" + p);
				}
				else
				{
					p.needs.mood.thoughts.GetAllMoodThoughts(Alert_Thought.tmpThoughts);
					try
					{
						ThoughtDef requiredDef = this.Thought;
						for (int i = 0; i < Alert_Thought.tmpThoughts.Count; i++)
						{
							if (Alert_Thought.tmpThoughts[i].def == requiredDef)
							{
								yield return p;
							}
						}
					}
					finally
					{
						base.<>__Finally0();
					}
				}
			}
		}

		public override AlertReport GetReport()
		{
			Pawn pawn = this.AffectedPawns().FirstOrDefault<Pawn>();
			if (pawn != null)
			{
				return AlertReport.CulpritIs(pawn);
			}
			return AlertReport.Inactive;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn current in this.AffectedPawns())
			{
				stringBuilder.AppendLine("    " + current.NameStringShort);
			}
			return this.explanationKey.Translate(new object[]
			{
				stringBuilder.ToString()
			});
		}
	}
}
