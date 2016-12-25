using System;
using Verse;

namespace RimWorld
{
	public class DeathActionWorker_BigExplosion : DeathActionWorker
	{
		public override void PawnDied(Corpse corpse)
		{
			float radius;
			if (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0)
			{
				radius = 1.9f;
			}
			else if (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 1)
			{
				radius = 2.9f;
			}
			else
			{
				radius = 4.9f;
			}
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, radius, DamageDefOf.Flame, corpse.InnerPawn, null, null, null, null, 0f, 1, false, null, 0f, 1);
		}
	}
}
