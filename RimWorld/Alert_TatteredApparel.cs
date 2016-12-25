using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_TatteredApparel : Alert_High
	{
		public override string FullExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Pawn current in this.TatteredClothesWearers())
				{
					stringBuilder.AppendLine("    " + current.NameStringShort);
				}
				return "AlertTatteredApparelDesc".Translate(new object[]
				{
					stringBuilder.ToString()
				});
			}
		}

		public override AlertReport Report
		{
			get
			{
				Pawn pawn = this.TatteredClothesWearers().FirstOrDefault<Pawn>();
				if (pawn != null)
				{
					return AlertReport.CulpritIs(pawn);
				}
				return AlertReport.Inactive;
			}
		}

		public Alert_TatteredApparel()
		{
			this.baseLabel = "AlertTatteredApparel".Translate();
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> TatteredClothesWearers()
		{
			foreach (Pawn p in Find.MapPawns.FreeColonists)
			{
				List<Thought> thoughts = p.needs.mood.thoughts.Thoughts;
				for (int i = 0; i < thoughts.Count; i++)
				{
					if (thoughts[i].def == ThoughtDefOf.ApparelDamaged)
					{
						yield return p;
					}
				}
			}
		}
	}
}
