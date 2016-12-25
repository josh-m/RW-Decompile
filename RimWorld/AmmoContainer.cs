using System;
using Verse;

namespace RimWorld
{
	public class AmmoContainer
	{
		public Verb verb;

		public bool limitedAmmo;

		public int ammoMax;

		public int ammo;

		public AmmoContainer(Verb verb)
		{
			this.verb = verb;
		}

		public bool HasAmmo()
		{
			return !this.limitedAmmo || this.ammo > 0;
		}

		public void ConsumeAmmo()
		{
			if (this.verb.caster.Faction == Faction.OfPlayer)
			{
				this.ammo--;
			}
		}

		public void RefillAmmo()
		{
			this.ammo = this.ammoMax;
		}
	}
}
