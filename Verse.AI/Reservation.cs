using RimWorld;
using System;

namespace Verse.AI
{
	internal class Reservation : IExposable
	{
		private Pawn claimant;

		private LocalTargetInfo target;

		private int maxPawns;

		public Pawn Claimant
		{
			get
			{
				return this.claimant;
			}
		}

		public LocalTargetInfo Target
		{
			get
			{
				return this.target;
			}
		}

		public int MaxPawns
		{
			get
			{
				return this.maxPawns;
			}
		}

		public Faction Faction
		{
			get
			{
				return this.claimant.Faction;
			}
		}

		public Reservation()
		{
		}

		public Reservation(Pawn claimant, int maxPawns, LocalTargetInfo target)
		{
			this.claimant = claimant;
			this.maxPawns = maxPawns;
			this.target = target;
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Pawn>(ref this.claimant, "claimant", false);
			Scribe_TargetInfo.LookTargetInfo(ref this.target, "target");
			Scribe_Values.LookValue<int>(ref this.maxPawns, "maxPawns", 0, false);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				(this.claimant == null) ? "null" : this.claimant.LabelShort,
				", ",
				this.target.ToString(),
				", ",
				this.maxPawns
			});
		}
	}
}
