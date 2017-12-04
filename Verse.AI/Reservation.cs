using RimWorld;
using System;

namespace Verse.AI
{
	internal class Reservation : IExposable
	{
		private Pawn claimant;

		private Job job;

		private LocalTargetInfo target;

		private ReservationLayerDef layer;

		private int maxPawns;

		private int stackCount = -1;

		public Pawn Claimant
		{
			get
			{
				return this.claimant;
			}
		}

		public Job Job
		{
			get
			{
				return this.job;
			}
		}

		public LocalTargetInfo Target
		{
			get
			{
				return this.target;
			}
		}

		public ReservationLayerDef Layer
		{
			get
			{
				return this.layer;
			}
		}

		public int MaxPawns
		{
			get
			{
				return this.maxPawns;
			}
		}

		public int StackCount
		{
			get
			{
				return this.stackCount;
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

		public Reservation(Pawn claimant, Job job, int maxPawns, int stackCount, LocalTargetInfo target, ReservationLayerDef layer)
		{
			this.claimant = claimant;
			this.job = job;
			this.maxPawns = maxPawns;
			this.stackCount = stackCount;
			this.target = target;
			this.layer = layer;
		}

		public void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.claimant, "claimant", false);
			Scribe_References.Look<Job>(ref this.job, "job", false);
			Scribe_TargetInfo.Look(ref this.target, "target");
			Scribe_Values.Look<int>(ref this.maxPawns, "maxPawns", 0, false);
			Scribe_Values.Look<int>(ref this.stackCount, "stackCount", 0, false);
			Scribe_Defs.Look<ReservationLayerDef>(ref this.layer, "layer");
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				(this.claimant == null) ? "null" : this.claimant.LabelShort,
				":",
				(this.job == null) ? "null" : this.job.ToString(),
				", ",
				this.target.ToString(),
				", ",
				(this.layer == null) ? "null" : this.layer.ToString(),
				", ",
				this.maxPawns,
				", ",
				this.stackCount
			});
		}
	}
}
