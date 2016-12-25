using System;

namespace Verse
{
	public class HediffComp_SelfHeal : HediffComp
	{
		public int ticksSinceHeal;

		public HediffCompProperties_SelfHeal Props
		{
			get
			{
				return (HediffCompProperties_SelfHeal)this.props;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ticksSinceHeal, "ticksSinceHeal", 0, false);
		}

		public override void CompPostTick()
		{
			this.ticksSinceHeal++;
			if (this.ticksSinceHeal > this.Props.healIntervalTicksStanding)
			{
				this.parent.Severity -= 1f;
				this.ticksSinceHeal = 0;
				base.Pawn.health.Notify_HediffChanged(this.parent);
			}
		}
	}
}
