using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Jetter : Thing
	{
		private enum JetterState
		{
			Resting,
			WickBurning,
			Jetting
		}

		private const int TicksBeforeBeginAccelerate = 25;

		private const int TicksBetweenMoves = 3;

		private Jetter.JetterState JState;

		private int WickTicksLeft;

		private int TicksUntilMove;

		protected Sustainer wickSoundSustainer;

		protected Sustainer jetSoundSustainer;

		public override void Tick()
		{
			if (this.JState == Jetter.JetterState.WickBurning)
			{
				base.Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
				this.WickTicksLeft--;
				if (this.WickTicksLeft == 0)
				{
					this.StartJetting();
				}
			}
			else if (this.JState == Jetter.JetterState.Jetting)
			{
				this.TicksUntilMove--;
				if (this.TicksUntilMove <= 0)
				{
					this.MoveJetter();
					this.TicksUntilMove = 3;
				}
			}
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (!base.Destroyed && dinfo.Def.harmsHealth && this.JState == Jetter.JetterState.Resting)
			{
				this.StartWick();
			}
		}

		protected void StartWick()
		{
			this.JState = Jetter.JetterState.WickBurning;
			this.WickTicksLeft = 25;
			SoundDef.Named("MetalHitImportant").PlayOneShot(this);
			this.wickSoundSustainer = SoundDef.Named("HissSmall").TrySpawnSustainer(this);
		}

		protected void StartJetting()
		{
			this.JState = Jetter.JetterState.Jetting;
			this.TicksUntilMove = 3;
			this.wickSoundSustainer.End();
			this.wickSoundSustainer = null;
			this.wickSoundSustainer = SoundDef.Named("HissJet").TrySpawnSustainer(this);
		}

		protected void MoveJetter()
		{
			IntVec3 intVec = base.Position + base.Rotation.FacingCell;
			if (!intVec.Walkable(base.Map) || base.Map.thingGrid.CellContains(intVec, ThingCategory.Pawn) || intVec.GetEdifice(base.Map) != null)
			{
				this.Destroy(DestroyMode.Vanish);
				GenExplosion.DoExplosion(base.Position, base.Map, 2.9f, DamageDefOf.Bomb, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
				return;
			}
			base.Position = intVec;
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			if (this.wickSoundSustainer != null)
			{
				this.wickSoundSustainer.End();
				this.wickSoundSustainer = null;
			}
			if (this.jetSoundSustainer != null)
			{
				this.jetSoundSustainer.End();
				this.jetSoundSustainer = null;
			}
		}
	}
}
