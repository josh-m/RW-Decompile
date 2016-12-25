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
				ProfilerThreadCheck.BeginSample("tweener.PreDrawPosCalculation()");
				this.tweener.PreDrawPosCalculation();
				ProfilerThreadCheck.EndSample();
				Vector3 vector = this.tweener.TweenedPos;
				vector += this.jitterer.CurrentOffset;
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
			if (Current.ProgramState == ProgramState.Playing && !Find.CameraDriver.CurrentViewRect.ExpandedBy(3).Contains(this.pawn.Position))
			{
				return;
			}
			this.jitterer.JitterHandlerTick();
			this.footprintMaker.FootprintMakerTick();
			this.breathMoteMaker.BreathMoteMakerTick();
			this.leaner.LeanerTick();
			this.rotator.PawnRotatorTick();
			this.renderer.RendererTick();
		}

		public void DrawAt(Vector3 loc)
		{
			this.renderer.RenderPawnAt(loc, RotDrawMode.Fresh);
		}

		public void Notify_Spawned()
		{
			this.tweener.ResetTweenedPosToRoot();
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
			else if (Target.DrawPos != this.pawn.DrawPos)
			{
				this.jitterer.AddOffset(0.25f, (Target.DrawPos - this.pawn.DrawPos).AngleFlat());
			}
		}

		public void Notify_DebugAffected()
		{
			for (int i = 0; i < 10; i++)
			{
				MoteMaker.ThrowAirPuffUp(this.pawn.DrawPos, this.pawn.Map);
			}
			this.jitterer.AddOffset(0.05f, (float)Rand.Range(0, 360));
		}
	}
}
