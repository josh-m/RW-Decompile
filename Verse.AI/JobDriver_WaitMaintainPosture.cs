using RimWorld;
using System;

namespace Verse.AI
{
	public class JobDriver_WaitMaintainPosture : JobDriver_Wait
	{
		private PawnPosture lastPosture;

		public override PawnPosture Posture
		{
			get
			{
				return this.lastPosture;
			}
		}

		public override void Notify_LastPosture(PawnPosture posture, LayingDownState layingDown)
		{
			this.lastPosture = posture;
			this.layingDown = layingDown;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<PawnPosture>(ref this.lastPosture, "lastPosture", PawnPosture.Standing, false);
		}
	}
}
