using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Pain : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			float pain = p.health.hediffSet.Pain;
			if (pain < 0.0001f)
			{
				return ThoughtState.Inactive;
			}
			if (pain < 0.15f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (pain < 0.4f)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (pain < 0.8f)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			return ThoughtState.ActiveAtStage(3);
		}
	}
}
