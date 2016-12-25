using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public class HediffComp_HealOldWounds : HediffComp
	{
		private int ticksToHeal;

		public HediffCompProperties_HealOldWounds Props
		{
			get
			{
				return (HediffCompProperties_HealOldWounds)this.props;
			}
		}

		public override void CompPostMake()
		{
			base.CompPostMake();
			this.ResetTicksToHeal();
		}

		private void ResetTicksToHeal()
		{
			this.ticksToHeal = Rand.Range(15, 30) * 60000;
		}

		public override void CompPostTick()
		{
			this.ticksToHeal--;
			if (this.ticksToHeal <= 0)
			{
				this.TryHealRandomOldWound();
				this.ResetTicksToHeal();
			}
		}

		private void TryHealRandomOldWound()
		{
			Hediff hediff;
			if (!(from hd in base.Pawn.health.hediffSet.hediffs
			where hd.IsOld()
			select hd).TryRandomElement(out hediff))
			{
				return;
			}
			hediff.Severity = 0f;
			if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				Messages.Message("MessageOldWoundHealed".Translate(new object[]
				{
					this.parent.Label,
					base.Pawn.LabelShort,
					hediff.Label
				}), MessageSound.Benefit);
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ticksToHeal, "ticksToHeal", 0, false);
		}

		public override string CompDebugString()
		{
			return "ticksToHeal: " + this.ticksToHeal;
		}
	}
}
