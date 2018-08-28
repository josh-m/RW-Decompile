using System;
using Verse;

namespace RimWorld
{
	public class Verb_Spawn : Verb
	{
		protected override bool TryCastShot()
		{
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			GenSpawn.Spawn(this.verbProps.spawnDef, this.currentTarget.Cell, this.caster.Map, WipeMode.Vanish);
			if (this.verbProps.colonyWideTaleDef != null)
			{
				Pawn pawn = this.caster.Map.mapPawns.FreeColonistsSpawned.RandomElementWithFallback(null);
				TaleRecorder.RecordTale(this.verbProps.colonyWideTaleDef, new object[]
				{
					this.caster,
					pawn
				});
			}
			if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
			{
				base.EquipmentSource.Destroy(DestroyMode.Vanish);
			}
			return true;
		}
	}
}
