using System;
using UnityEngine;

namespace Verse
{
	public class SkyfallerProperties
	{
		public bool hitRoof = true;

		public IntRange ticksToImpactRange = new IntRange(120, 200);

		public bool reversed;

		public float explosionRadius = 3f;

		public DamageDef explosionDamage;

		public float explosionDamageFactor = 1f;

		public IntRange metalShrapnelCountRange = IntRange.zero;

		public IntRange rubbleShrapnelCountRange = IntRange.zero;

		public float shrapnelDistanceFactor = 1f;

		public SkyfallerMovementType movementType;

		public float speed = 1f;

		public string shadow = "Things/Skyfaller/SkyfallerShadowCircle";

		public Vector2 shadowSize = Vector2.one;

		public float cameraShake;

		public SoundDef impactSound;

		public bool rotateGraphicTowardsDirection;

		public SoundDef anticipationSound;

		public int anticipationSoundTicks = 100;

		public int motesPerCell = 3;

		public bool MakesShrapnel
		{
			get
			{
				return this.metalShrapnelCountRange.max > 0 || this.rubbleShrapnelCountRange.max > 0;
			}
		}
	}
}
