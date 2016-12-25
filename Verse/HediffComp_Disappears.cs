using System;

namespace Verse
{
	public class HediffComp_Disappears : HediffComp
	{
		private int ticksToDisappear;

		public HediffCompProperties_Disappears Props
		{
			get
			{
				return (HediffCompProperties_Disappears)this.props;
			}
		}

		public override bool CompShouldRemove
		{
			get
			{
				return base.CompShouldRemove || this.ticksToDisappear <= 0;
			}
		}

		public override void CompPostMake()
		{
			base.CompPostMake();
			this.ticksToDisappear = this.Props.disappearsAfterTicks.RandomInRange;
		}

		public override void CompPostTick()
		{
			this.ticksToDisappear--;
		}

		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);
			HediffComp_Disappears hediffComp_Disappears = other.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > this.ticksToDisappear)
			{
				this.ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ticksToDisappear, "ticksToDisappear", 0, false);
		}

		public override string CompDebugString()
		{
			return "ticksToDisappear: " + this.ticksToDisappear;
		}
	}
}
