using RimWorld;
using System;

namespace Verse
{
	public class Stance_Warmup : Stance_Busy
	{
		public Verb verb;

		private bool targetStartedDowned;

		public Stance_Warmup()
		{
		}

		public Stance_Warmup(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg)
		{
			this.verb = verb;
			if (focusTarg.HasThing && focusTarg.Thing is Pawn)
			{
				Pawn pawn = (Pawn)focusTarg.Thing;
				this.targetStartedDowned = pawn.Downed;
				if (pawn.apparel != null)
				{
					for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
					{
						Apparel apparel = pawn.apparel.WornApparel[i];
						PersonalShield personalShield = apparel as PersonalShield;
						if (personalShield != null)
						{
							personalShield.KeepDisplaying();
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.LookReference<Verb>(ref this.verb, "verb", false);
			Scribe_Values.LookValue<bool>(ref this.targetStartedDowned, "targetStartDowned", false, false);
		}

		public override void StanceDraw()
		{
			if (Find.Selector.IsSelected(this.stanceTracker.pawn))
			{
				GenDraw.DrawAimPie(this.stanceTracker.pawn, this.focusTarg, (int)((float)this.ticksLeft * this.pieSizeFactor), 0.2f);
			}
		}

		public override void StanceTick()
		{
			if (!this.targetStartedDowned && this.focusTarg.HasThing && this.focusTarg.Thing is Pawn && ((Pawn)this.focusTarg.Thing).Downed)
			{
				this.stanceTracker.SetStance(new Stance_Mobile());
				return;
			}
			if (this.focusTarg.HasThing && (!this.focusTarg.Thing.Spawned || !this.verb.CanHitTargetFrom(base.Pawn.Position, this.focusTarg) || base.Pawn.Position.DistanceToSquared(this.focusTarg.Thing.Position) > this.verb.verbProps.range * this.verb.verbProps.range))
			{
				this.stanceTracker.SetStance(new Stance_Mobile());
				return;
			}
			if (this.focusTarg == base.Pawn.mindState.enemyTarget)
			{
				base.Pawn.mindState.Notify_EngagedTarget();
			}
			base.StanceTick();
		}

		protected override void Expire()
		{
			base.Expire();
			this.verb.WarmupComplete();
		}
	}
}
