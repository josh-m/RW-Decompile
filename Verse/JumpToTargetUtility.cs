using System;

namespace Verse
{
	public static class JumpToTargetUtility
	{
		public static void TryJumpAndSelect(Thing thing)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			JumpToTargetUtility.TryJump(thing);
			JumpToTargetUtility.TrySelect(thing);
		}

		public static void TryJumpAndSelect(TargetInfo target)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			if (!target.IsValid)
			{
				return;
			}
			if (target.HasThing)
			{
				JumpToTargetUtility.TryJumpAndSelect(target.Thing);
			}
			else
			{
				JumpToTargetUtility.TryJump(target);
			}
		}

		public static void TrySelect(Thing thing)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			Thing corpseIfDeadPawn = JumpToTargetUtility.GetCorpseIfDeadPawn(thing);
			if (corpseIfDeadPawn.Spawned)
			{
				Find.Selector.ClearSelection();
				Find.Selector.Select(corpseIfDeadPawn, true, true);
			}
		}

		public static void TryJump(Thing thing)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			Thing corpseIfDeadPawn = JumpToTargetUtility.GetCorpseIfDeadPawn(thing);
			if (corpseIfDeadPawn.PositionHeld.IsValid)
			{
				Find.CameraDriver.JumpTo(corpseIfDeadPawn.PositionHeld);
			}
		}

		public static void TryJump(TargetInfo target)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return;
			}
			if (!target.IsValid)
			{
				return;
			}
			if (target.HasThing)
			{
				JumpToTargetUtility.TryJump(target.Thing);
			}
			else
			{
				Find.CameraDriver.JumpTo(target.Cell);
			}
		}

		private static Thing GetCorpseIfDeadPawn(Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn != null && pawn.Dead && pawn.corpse != null)
			{
				return pawn.corpse;
			}
			return thing;
		}
	}
}
