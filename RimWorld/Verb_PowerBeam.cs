using System;
using Verse;

namespace RimWorld
{
	public class Verb_PowerBeam : Verb
	{
		private const int DurationTicks = 600;

		protected override bool TryCastShot()
		{
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				return false;
			}
			PowerBeam powerBeam = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, this.currentTarget.Cell, this.caster.Map);
			powerBeam.duration = 600;
			powerBeam.instigator = this.caster;
			powerBeam.weaponDef = ((this.ownerEquipment == null) ? null : this.ownerEquipment.def);
			powerBeam.StartStrike();
			if (this.ownerEquipment != null && !this.ownerEquipment.Destroyed)
			{
				this.ownerEquipment.Destroy(DestroyMode.Vanish);
			}
			return true;
		}

		public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
		{
			needLOSToCenter = false;
			return 15f;
		}
	}
}
