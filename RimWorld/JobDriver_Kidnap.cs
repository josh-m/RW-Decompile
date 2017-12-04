using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Kidnap : JobDriver_TakeAndExitMap
	{
		protected Pawn Takee
		{
			get
			{
				return (Pawn)base.Item;
			}
		}

		public override string GetReport()
		{
			if (this.Takee == null || this.pawn.HostileTo(this.Takee))
			{
				return base.GetReport();
			}
			return JobDefOf.Rescue.reportString.Replace("TargetA", this.Takee.LabelShort);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => this.$this.Takee == null || (!this.$this.Takee.Downed && this.$this.Takee.Awake()));
			foreach (Toil t in base.MakeNewToils())
			{
				yield return t;
			}
		}
	}
}
