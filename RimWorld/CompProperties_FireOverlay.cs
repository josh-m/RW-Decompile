using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_FireOverlay : CompProperties
	{
		public float fireSize = 1f;

		public Vector3 offset;

		public CompProperties_FireOverlay()
		{
			this.compClass = typeof(CompFireOverlay);
		}
	}
}
