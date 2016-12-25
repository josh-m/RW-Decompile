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
			d.category = VerbCategory.BeatFire;
			d.label = "Beat fire";
			d.range = 1f;
			d.noiseRadius = 3f;
			d.targetParams.canTargetFires = true;
			d.targetParams.canTargetPawns = false;
			d.targetParams.canTargetBuildings = false;
			d.targetParams.worldObjectTargetsMustBeAutoAttackable = false;
			d.warmupTicks = 0;
			d.defaultCooldownTicks = 65;
			d.soundCast = SoundDef.Named("Interact_BeatFire");
			yield return d;
			d = new VerbProperties();
			d.category = VerbCategory.Ignite;
			d.label = "Ignite";
			d.range = 1f;
			d.noiseRadius = 3f;
			d.targetParams.onlyTargetFlammables = true;
			d.targetParams.canTargetBuildings = true;
			d.targetParams.canTargetPawns = false;
			d.targetParams.worldObjectTargetsMustBeAutoAttackable = false;
			d.warmupTicks = 180;
			d.defaultCooldownTicks = 80;
			d.soundCast = SoundDef.Named("Interact_Ignite");
			yield return d;
		}
	}
}
