using System;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_FollowFieldwork : PawnColumnWorker_Checkbox
	{
		protected override bool HasCheckbox(Pawn pawn)
		{
			return pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && pawn.training.IsCompleted(TrainableDefOf.Obedience);
		}

		protected override bool GetValue(Pawn pawn)
		{
			return pawn.playerSettings.followFieldwork;
		}

		protected override void SetValue(Pawn pawn, bool value)
		{
			pawn.playerSettings.followFieldwork = value;
		}
	}
}
