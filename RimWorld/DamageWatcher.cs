using System;
using Verse;

namespace RimWorld
{
	public class DamageWatcher : IExposable
	{
		private float everDamage;

		public float DamageTakenEver
		{
			get
			{
				return this.everDamage;
			}
		}

		public void Notify_DamageTaken(Thing damagee, float amount)
		{
			if (damagee.Faction != Faction.OfPlayer)
			{
				return;
			}
			this.everDamage += amount;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<float>(ref this.everDamage, "everDamage", 0f, false);
		}
	}
}
