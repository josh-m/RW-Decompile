using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class ProjectileProperties
	{
		public float speed = 4f;

		public bool flyOverhead;

		public bool alwaysFreeIntercept;

		public DamageDef damageDef;

		public int damageAmountBase = 1;

		public SoundDef soundHitThickRoof;

		public SoundDef soundExplode;

		public SoundDef soundImpactAnticipate;

		public SoundDef soundAmbient;

		public float explosionRadius;

		public int explosionDelay;

		public ThingDef preExplosionSpawnThingDef;

		public ThingDef postExplosionSpawnThingDef;

		public float explosionSpawnChance = 1f;

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors(ThingDef parent)
		{
			if (this.alwaysFreeIntercept && this.flyOverhead)
			{
				yield return "alwaysFreeIntercept and flyOverhead are both true";
			}
		}
	}
}
