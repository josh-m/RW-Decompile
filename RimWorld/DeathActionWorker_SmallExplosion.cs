using System;
using Verse;

namespace RimWorld
{
	public class DeathActionWorker_SmallExplosion : DeathActionWorker
	{
		public override void PawnDied(Corpse corpse)
		{
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, 1.9f, DamageDefOf.Flame, corpse.InnerPawn, null, null, null, null, 0f, 1, false, null, 0f, 1);
		}
	}
}
