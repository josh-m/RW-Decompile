using System;
using Verse;

namespace RimWorld
{
	public class OrbitalStrike : ThingWithComps
	{
		public int duration;

		public Thing instigator;

		public ThingDef weaponDef;

		private float angle;

		private int startTick;

		private static readonly FloatRange AngleRange = new FloatRange(-12f, 12f);

		private const int SkyColorFadeInTicks = 30;

		private const int SkyColorFadeOutTicks = 15;

		private const int OrbitalBeamFadeOutTicks = 10;

		protected int TicksPassed
		{
			get
			{
				return Find.TickManager.TicksGame - this.startTick;
			}
		}

		protected int TicksLeft
		{
			get
			{
				return this.duration - this.TicksPassed;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Thing>(ref this.instigator, "instigator", false);
			Scribe_Defs.Look<ThingDef>(ref this.weaponDef, "weaponDef");
			Scribe_Values.Look<int>(ref this.duration, "duration", 0, false);
			Scribe_Values.Look<float>(ref this.angle, "angle", 0f, false);
			Scribe_Values.Look<int>(ref this.startTick, "startTick", 0, false);
		}

		public override void Draw()
		{
			base.Comps_PostDraw();
		}

		public virtual void StartStrike()
		{
			if (!base.Spawned)
			{
				Log.Error("Called StartStrike() on unspawned thing.");
				return;
			}
			this.angle = OrbitalStrike.AngleRange.RandomInRange;
			this.startTick = Find.TickManager.TicksGame;
			base.GetComp<CompAffectsSky>().StartFadeInHoldFadeOut(30, this.duration - 30 - 15, 15, 1f);
			base.GetComp<CompOrbitalBeam>().StartAnimation(this.duration, 10, this.angle);
		}

		public override void Tick()
		{
			base.Tick();
			if (this.TicksPassed >= this.duration)
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
