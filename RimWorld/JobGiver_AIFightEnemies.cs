using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIFightEnemies : JobGiver_AIFightEnemy
	{
		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
		{
			bool allowManualCastWeapons = !pawn.IsColonist;
			Verb verb = pawn.TryGetAttackVerb(allowManualCastWeapons);
			if (verb == null)
			{
				dest = IntVec3.Invalid;
				return false;
			}
			return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
			{
				caster = pawn,
				target = pawn.mindState.enemyTarget,
				verb = verb,
				maxRangeFromTarget = verb.verbProps.range,
				wantCoverFromTarget = (verb.verbProps.range > 5f)
			}, out dest);
		}
	}
}
