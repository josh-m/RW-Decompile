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

		public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude)
		{
			Graphic graphic = GhostUtility.GhostGraphicFor(CompFireOverlay.FireGraphic, thingDef, ghostCol);
			graphic.DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef, 0f);
		}
	}
}
