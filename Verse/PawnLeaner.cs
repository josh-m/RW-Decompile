using System;
using UnityEngine;

namespace Verse
{
	public class PawnLeaner
	{
		private const float LeanOffsetPctChangeRate = 0.075f;

		private const float LeanOffsetDistanceMultiplier = 0.5f;

		private Pawn pawn;

		private IntVec3 shootSourceOffset = new IntVec3(0, 0, 0);

		private float leanOffsetCurPct;

		public Vector3 LeanOffset
		{
			get
			{
				return this.shootSourceOffset.ToVector3() * 0.5f * this.leanOffsetCurPct;
			}
		}

		public PawnLeaner(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void LeanerTick()
		{
			if (this.ShouldLean())
			{
				this.leanOffsetCurPct += 0.075f;
				if (this.leanOffsetCurPct > 1f)
				{
					this.leanOffsetCurPct = 1f;
				}
			}
			else
			{
				this.leanOffsetCurPct -= 0.075f;
				if (this.leanOffsetCurPct < 0f)
				{
					this.leanOffsetCurPct = 0f;
				}
			}
		}

		public bool ShouldLean()
		{
			Stance_Busy stance_Busy = this.pawn.stances.curStance as Stance_Busy;
			return stance_Busy != null && !(this.shootSourceOffset == new IntVec3(0, 0, 0));
		}

		public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
		{
			this.shootSourceOffset = newShootLine.Source - this.pawn.Position;
		}
	}
}
