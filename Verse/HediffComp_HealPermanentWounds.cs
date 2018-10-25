using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public class HediffComp_HealPermanentWounds : HediffComp
	{
		private int ticksToHeal;

		public HediffCompProperties_HealPermanentWounds Props
		{
			get
			{
				return (HediffCompProperties_HealPermanentWounds)this.props;
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

		public override void CompPostTick(ref float severityAdjustment)
		{
			this.ticksToHeal--;
			if (this.ticksToHeal <= 0)
			{
				this.TryHealRandomPermanentWound();
				this.ResetTicksToHeal();
			}
		}

		private void TryHealRandomPermanentWound()
		{
			Hediff hediff;
			if (!base.Pawn.health.hediffSet.hediffs.Where(new Func<Hediff, bool>(HediffUtility.IsPermanent)).TryRandomElement(out hediff))
			{
				return;
			}
			hediff.Severity = 0f;
			if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				Messages.Message("MessagePermanentWoundHealed".Translate(this.parent.LabelCap, base.Pawn.LabelShort, hediff.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksToHeal, "ticksToHeal", 0, false);
		}

		public override string CompDebugString()
		{
			return "ticksToHeal: " + this.ticksToHeal;
		}
	}
}
