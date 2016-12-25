using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Pawn_DrawTracker
	{
		private const float MeleeJitterDistance = 0.5f;

		private Pawn pawn;

		public PawnTweener tweener;

		private JitterHandler jitterer;

		public PawnLeaner leaner;

		public PawnRotator rotator;

		public PawnRenderer renderer;

		public PawnUIOverlay ui;

		private PawnFootprintMaker footprintMaker;

		private PawnBreathMoteMaker breathMoteMaker;

		public Vector3 DrawPos
		{
			get
			{
				Vector3 vector = this.tweener.TweenedPos;
				vector += this.jitterer.CurrentJitterOffset;
				vector += this.leaner.LeanOffset;
				vector.y = this.pawn.def.Altitude;
				return vector;
			}
		}

		public Pawn_DrawTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.tweener = new PawnTweener(pawn);
			this.jitterer = new JitterHandler();
			this.leaner = new PawnLeaner(pawn);
			this.rotator = new PawnRotator(pawn);
			this.renderer = new PawnRenderer(pawn);
			this.ui = new PawnUIOverlay(pawn);
			this.footprintMaker = new PawnFootprintMaker(pawn);
			this.breathMoteMaker = new PawnBreathMoteMaker(pawn);
		}

		public void DrawTrackerTick()
		{
			if (!this.pawn.Spawned)
			{
				return;
			}
			this.jitterer.JitterHandlerTick();
			this.tweener.TweenerTick();
			this.footprintMaker.FootprintMakerTick();
			this.breathMoteMaker.BreathMoteMakerTick();
			this.leaner.LeanerTick();
			this.rotator.PawnRotatorTick();
			this.renderer.RendererTick();
		}

		public void Notify_Spawned()
		{
			this.tweener.ResetToPosition();
		}

		public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
		{
			this.leaner.Notify_WarmingCastAlongLine(newShootLine, ShootPosition);
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (this.pawn.Destroyed)
			{
				return;
			}
			this.jitterer.Notify_DamageApplied(dinfo);
			this.renderer.Notify_DamageApplied(dinfo);
		}

		public void Notify_MeleeAttackOn(Thing Target)
		{
			if (Target.Position != this.pawn.Position)
			{
				this.jitterer.AddOffset(0.5f, (Target.Position - this.pawn.Position).AngleFlat);
			}
		}

		public void Notify_DebugAffected()
		{
			for (int i = 0; i < 10; i++)
			{
				MoteMaker.ThrowAirPuffUp(this.pawn.DrawPos);
			}
			this.jitterer.AddOffset(0.05f, (float)Rand.Range(0, 360));
		}
	}
}
