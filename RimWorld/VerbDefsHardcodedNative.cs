using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class VerbDefsHardcodedNative
	{
		[DebuggerHidden]
		public static IEnumerable<VerbProperties> AllVerbDefs()
		{
			VerbProperties d = new VerbProperties();
			d.verbClass = typeof(Verb_BeatFire);
			d.category = VerbCategory.BeatFire;
			d.range = 1.42f;
			d.noiseRadius = 3f;
			d.targetParams.canTargetFires = true;
			d.targetParams.canTargetPawns = false;
			d.targetParams.canTargetBuildings = false;
			d.targetParams.mapObjectTargetsMustBeAutoAttackable = false;
			d.warmupTime = 0f;
			d.defaultCooldownTime = 1.1f;
			d.soundCast = SoundDefOf.Interact_BeatFire;
			yield return d;
			d = new VerbProperties();
			d.verbClass = typeof(Verb_Ignite);
			d.category = VerbCategory.Ignite;
			d.range = 1.42f;
			d.noiseRadius = 3f;
			d.targetParams.onlyTargetFlammables = true;
			d.targetParams.canTargetBuildings = true;
			d.targetParams.canTargetPawns = false;
			d.targetParams.mapObjectTargetsMustBeAutoAttackable = false;
			d.warmupTime = 3f;
			d.defaultCooldownTime = 1.3f;
			d.soundCast = SoundDefOf.Interact_Ignite;
			yield return d;
		}
	}
}
