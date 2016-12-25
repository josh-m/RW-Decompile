using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class ImpactSoundUtility
	{
		public static void PlayImpactSound(Thing hitThing, ImpactSoundType ist)
		{
			if (ist == ImpactSoundType.None)
			{
				return;
			}
			SoundDef soundDef;
			if (hitThing.Stuff != null)
			{
				soundDef = hitThing.Stuff.stuffProps.soundImpactStuff;
			}
			else
			{
				soundDef = hitThing.def.soundImpactDefault;
			}
			if (soundDef.NullOrUndefined())
			{
				soundDef = SoundDefOf.BulletImpactGround;
			}
			soundDef.PlayOneShot(hitThing.Position);
		}
	}
}
