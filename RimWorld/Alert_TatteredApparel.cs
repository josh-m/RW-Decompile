using System;
using Verse;

namespace RimWorld
{
	public class Alert_TatteredApparel : Alert_Thought
	{
		protected override ThoughtDef Thought
		{
			get
			{
				return ThoughtDefOf.ApparelDamaged;
			}
		}

		public Alert_TatteredApparel()
		{
			this.defaultLabel = "AlertTatteredApparel".Translate();
			this.explanationKey = "AlertTatteredApparelDesc";
		}
	}
}
