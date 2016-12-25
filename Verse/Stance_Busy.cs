using System;

namespace Verse
{
	public abstract class Stance_Busy : Stance
	{
		public int ticksLeft;

		public LocalTargetInfo focusTarg;

		public bool neverAimWeapon;

		protected float pieSizeFactor = 1f;

		public override bool StanceBusy
		{
			get
			{
				return true;
			}
		}

		public Stance_Busy()
		{
			this.SetPieSizeFactor();
		}

		public Stance_Busy(int ticks, LocalTargetInfo focusTarg)
		{
			this.ticksLeft = ticks;
			this.focusTarg = focusTarg;
		}

		public Stance_Busy(int ticks) : this(ticks, null)
		{
		}

		private void SetPieSizeFactor()
		{
			if (this.ticksLeft < 300)
			{
				this.pieSizeFactor = 1f;
			}
			else if (this.ticksLeft < 450)
			{
				this.pieSizeFactor = 0.75f;
			}
			else
			{
				this.pieSizeFactor = 0.5f;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksLeft, "ticksLeft", 0, false);
			Scribe_TargetInfo.LookTargetInfo(ref this.focusTarg, "focusTarg");
			Scribe_Values.LookValue<bool>(ref this.neverAimWeapon, "neverAimWeapon", false, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.SetPieSizeFactor();
			}
		}

		public override void StanceTick()
		{
			this.ticksLeft--;
			if (this.ticksLeft <= 0)
			{
				this.Expire();
			}
		}

		protected virtual void Expire()
		{
			this.stanceTracker.SetStance(new Stance_Mobile());
		}
	}
}
