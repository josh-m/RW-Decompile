using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_TurretTop : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol)
		{
			Graphic baseGraphic = GraphicDatabase.Get<Graphic_Single>(def.building.turretTopGraphicPath, ShaderDatabase.Cutout, new Vector2(def.building.turretTopDrawSize, def.building.turretTopDrawSize), Color.white);
			Graphic graphic = GhostUtility.GhostGraphicFor(baseGraphic, def, ghostCol);
			graphic.DrawFromDef(GenThing.TrueCenter(loc, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor()), rot, def, 0f);
		}
	}
}
