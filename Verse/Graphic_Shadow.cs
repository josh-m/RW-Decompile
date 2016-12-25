using System;
using UnityEngine;

namespace Verse
{
	public class Graphic_Shadow : Graphic
	{
		private Mesh shadowMesh;

		private ShadowData shadowInfo;

		public Graphic_Shadow(ShadowData shadowInfo)
		{
			this.shadowInfo = shadowInfo;
			if (shadowInfo == null)
			{
				throw new ArgumentNullException("shadowInfo");
			}
			this.shadowMesh = ShadowMeshPool.GetShadowMesh(shadowInfo);
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing)
		{
			if (this.shadowMesh != null && thingDef != null && this.shadowInfo != null && (Find.VisibleMap == null || !Find.VisibleMap.roofGrid.Roofed(loc.ToIntVec3())))
			{
				Vector3 position = loc + this.shadowInfo.offset;
				position.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
				Graphics.DrawMesh(this.shadowMesh, position, Quaternion.identity, MatBases.SunShadowFade, 0);
			}
		}

		public override void Print(SectionLayer layer, Thing thing)
		{
			Vector3 center = thing.TrueCenter() + this.shadowInfo.offset;
			center.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
			Printer_Shadow.PrintShadow(layer, center, this.shadowInfo);
		}

		public override string ToString()
		{
			return "Graphic_Shadow(" + this.shadowInfo + ")";
		}
	}
}
