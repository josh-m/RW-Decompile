using System;

namespace Verse
{
	public class PawnRotator
	{
		private Pawn pawn;

		public PawnRotator(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void PawnRotatorTick()
		{
			if (this.pawn.Destroyed)
			{
				return;
			}
			if (Find.TickManager.TicksGame % 2 == 0)
			{
				return;
			}
			if (this.pawn.pather.Moving)
			{
				if (this.pawn.pather.curPath == null || this.pawn.pather.curPath.NodesLeftCount < 1)
				{
					return;
				}
				this.FaceAdjacentCell(this.pawn.pather.nextCell);
				return;
			}
			else
			{
				Stance_Busy stance_Busy = this.pawn.stances.curStance as Stance_Busy;
				if (stance_Busy != null && stance_Busy.focusTarg.IsValid)
				{
					this.FaceCell(stance_Busy.focusTarg.Cell);
					return;
				}
				if (this.pawn.jobs.curJob != null)
				{
					TargetInfo target = this.pawn.CurJob.GetTarget(this.pawn.jobs.curDriver.rotateToFace);
					if (target.HasThing)
					{
						bool flag = false;
						IntVec3 c = default(IntVec3);
						CellRect cellRect = target.Thing.OccupiedRect();
						for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
						{
							for (int j = cellRect.minX; j <= cellRect.maxX; j++)
							{
								IntVec3 intVec = new IntVec3(j, 0, i);
								if (intVec.AdjacentToCardinal(this.pawn.Position))
								{
									this.FaceAdjacentCell(intVec);
									return;
								}
								if (intVec.AdjacentTo8Way(this.pawn.Position))
								{
									flag = true;
									c = intVec;
								}
							}
						}
						if (flag)
						{
							if (DebugViewSettings.drawPawnRotatorTarget)
							{
								Find.DebugDrawer.FlashCell(this.pawn.Position, 0.6f, "jbthing");
								GenDraw.DrawLineBetween(this.pawn.Position.ToVector3Shifted(), c.ToVector3Shifted());
							}
							this.FaceAdjacentCell(c);
							return;
						}
					}
					else if (this.pawn.Position.AdjacentTo8Way(target.Cell))
					{
						if (DebugViewSettings.drawPawnRotatorTarget)
						{
							Find.DebugDrawer.FlashCell(this.pawn.Position, 0.2f, "jbloc");
							GenDraw.DrawLineBetween(this.pawn.Position.ToVector3Shifted(), target.Cell.ToVector3Shifted());
						}
						this.FaceAdjacentCell(target.Cell);
						return;
					}
				}
				if (this.pawn.RaceProps.Humanlike)
				{
					this.pawn.Rotation = Rot4.South;
				}
				return;
			}
		}

		private void FaceAdjacentCell(IntVec3 c)
		{
			IntVec3 intVec = c - this.pawn.Position;
			if (intVec.x > 0)
			{
				this.pawn.Rotation = Rot4.East;
			}
			else if (intVec.x < 0)
			{
				this.pawn.Rotation = Rot4.West;
			}
			else if (intVec.z > 0)
			{
				this.pawn.Rotation = Rot4.North;
			}
			else
			{
				this.pawn.Rotation = Rot4.South;
			}
		}

		public void FaceCell(IntVec3 c)
		{
			float angle = (c - this.pawn.Position).ToVector3().AngleFlat();
			this.pawn.Rotation = PawnRotator.RotFromAngleBiased(angle);
		}

		public static Rot4 RotFromAngleBiased(float angle)
		{
			if (angle < 30f)
			{
				return Rot4.North;
			}
			if (angle < 150f)
			{
				return Rot4.East;
			}
			if (angle < 210f)
			{
				return Rot4.South;
			}
			if (angle < 330f)
			{
				return Rot4.West;
			}
			return Rot4.North;
		}
	}
}
