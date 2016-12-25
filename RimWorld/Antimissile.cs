using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Antimissile : Projectile
	{
		public override Vector3 ExactPosition
		{
			get
			{
				Vector3 b = (((Projectile)this.assignedTarget).ExactPosition - this.origin) * (1f - (float)this.ticksToImpact / (float)base.StartingTicksToImpact);
				return this.origin + b + Vector3.up * this.def.Altitude;
			}
		}

		public override Quaternion ExactRotation
		{
			get
			{
				return Quaternion.LookRotation(((Projectile)this.assignedTarget).ExactPosition - this.ExactPosition);
			}
		}

		protected override void Impact(Thing hitThing)
		{
			base.Impact(hitThing);
			hitThing.Destroy(DestroyMode.Vanish);
		}
	}
}
