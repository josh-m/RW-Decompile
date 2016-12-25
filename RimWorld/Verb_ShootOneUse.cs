using System;
using Verse;

namespace RimWorld
{
	public class Verb_ShootOneUse : Verb_Shoot
	{
		protected override bool TryCastShot()
		{
			if (base.TryCastShot())
			{
				if (this.burstShotsLeft <= 1)
				{
					this.SelfConsume();
				}
				return true;
			}
			if (this.burstShotsLeft < this.verbProps.burstShotCount)
			{
				this.SelfConsume();
			}
			return false;
		}

		public override void Notify_EquipmentLost()
		{
			base.Notify_EquipmentLost();
			if (this.state == VerbState.Bursting && this.burstShotsLeft < this.verbProps.burstShotCount)
			{
				this.SelfConsume();
			}
		}

		private void SelfConsume()
		{
			if (this.ownerEquipment != null && !this.ownerEquipment.Destroyed)
			{
				this.ownerEquipment.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
