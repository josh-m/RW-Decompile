using System;
using Verse;

namespace RimWorld
{
	public class Verb_Bombardment : Verb
	{
		private const int DurationTicks = 450;

		protected override bool TryCastShot()
		{
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			Bombardment bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, this.currentTarget.Cell, this.caster.Map);
			bombardment.duration = 450;
			bombardment.instigator = this.caster;
			bombardment.weaponDef = ((this.ownerEquipment == null) ? null : this.ownerEquipment.def);
			bombardment.StartStrike();
			if (this.ownerEquipment != null && !this.ownerEquipment.Destroyed)
			{
				this.ownerEquipment.Destroy(DestroyMode.Vanish);
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return 23f;
		}
	}
}
