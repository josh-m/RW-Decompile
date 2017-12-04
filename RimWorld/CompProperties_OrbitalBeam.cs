using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_OrbitalBeam : CompProperties
	{
		public float width = 8f;

		public Color color = Color.white;

		public SoundDef sound;

		public CompProperties_OrbitalBeam()
		{
			this.compClass = typeof(CompOrbitalBeam);
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string err in base.ConfigErrors(parentDef))
			{
				yield return err;
			}
			if (parentDef.drawerType != DrawerType.RealtimeOnly && parentDef.drawerType != DrawerType.MapMeshAndRealTime)
			{
				yield return "orbital beam requires realtime drawer";
			}
		}
	}
}
