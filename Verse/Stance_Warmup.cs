using RimWorld;
using System;

namespace Verse
{
	public class Stance_Warmup : Stance_Busy
	{
		private bool targetStartedDowned;

		public Stance_Warmup()
		{
		}

		public Stance_Warmup(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
		{
			if (focusTarg.HasThing && focusTarg.Thing is Pawn)
			{
				Pawn pawn = (Pawn)focusTarg.Thing;
				this.targetStartedDowned = pawn.Downed;
				if (pawn.apparel != null)
				{
					for (int i = 0; i < pawn.apparel.WornApparelCount; i++)
					{
						Apparel apparel = pawn.apparel.WornApparel[i];
						ShieldBelt shieldBelt = apparel as ShieldBelt;
						if (shieldBelt != null)
						{
							shieldBelt.KeepDisplaying();
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.targetStartedDowned, "targetStartDowned", false, false);
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
			if (this.focusTarg.HasThing && (!this.focusTarg.Thing.Spawned || !this.verb.CanHitTargetFrom(base.Pawn.Position, this.focusTarg)))
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
			this.verb.WarmupComplete();
			base.Expire();
		}
	}
}
