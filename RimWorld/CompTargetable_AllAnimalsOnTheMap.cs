using System;
using Verse;

namespace RimWorld
{
	public class CompTargetable_AllAnimalsOnTheMap : CompTargetable_AllPawnsOnTheMap
	{
		protected override TargetingParameters GetTargetingParameters()
		{
			TargetingParameters targetingParameters = base.GetTargetingParameters();
			targetingParameters.validator = delegate(TargetInfo targ)
			{
				if (!base.BaseTargetValidator(targ.Thing))
				{
					return false;
				}
				Pawn pawn = targ.Thing as Pawn;
				return pawn != null && pawn.RaceProps.Animal;
			};
			return targetingParameters;
		}
	}
}
