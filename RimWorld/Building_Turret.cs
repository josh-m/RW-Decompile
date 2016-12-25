using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Building_Turret : Building, IAttackTarget, ILoadReferenceable
	{
		private const float SightRadiusTurret = 13.4f;

		protected StunHandler stunner;

		protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;

		public abstract LocalTargetInfo CurrentTarget
		{
			get;
		}

		public abstract Verb AttackVerb
		{
			get;
		}

		public Building_Turret()
		{
			this.stunner = new StunHandler(this);
		}

		public override void Tick()
		{
			base.Tick();
			this.stunner.StunHandlerTick();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.LookDeep<StunHandler>(ref this.stunner, "stunner", new object[]
			{
				this
			});
		}

		public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(dinfo, out absorbed);
			if (absorbed)
			{
				return;
			}
			this.stunner.Notify_DamageApplied(dinfo, true);
			absorbed = false;
		}

		public abstract void OrderAttack(LocalTargetInfo targ);

		public bool ThreatDisabled()
		{
			CompPowerTrader comp = base.GetComp<CompPowerTrader>();
			if (comp == null || !comp.PowerOn)
			{
				CompMannable comp2 = base.GetComp<CompMannable>();
				if (comp2 == null || !comp2.MannedNow)
				{
					return true;
				}
			}
			return false;
		}
	}
}
