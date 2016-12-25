using System;
using Verse;

namespace RimWorld
{
	public class FactionRelation : IExposable
	{
		public Faction other;

		public float goodwill = 100f;

		public bool hostile;

		public void ExposeData()
		{
			Scribe_References.LookReference<Faction>(ref this.other, "other", false);
			Scribe_Values.LookValue<float>(ref this.goodwill, "goodwill", 0f, false);
			Scribe_Values.LookValue<bool>(ref this.hostile, "hostile", false, false);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.other,
				", goodwill=",
				this.goodwill.ToString("F1"),
				(!this.hostile) ? string.Empty : " hostile",
				")"
			});
		}
	}
}
