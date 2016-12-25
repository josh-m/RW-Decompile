using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnFootprintMaker
	{
		private const float FootprintIntervalDist = 0.4f;

		private const float LeftRightOffsetDist = 0.17f;

		private Pawn pawn;

		private Vector3 lastFootprintPlacePos;

		private bool lastFootprintRight;

		private static readonly Vector3 FootprintOffset = new Vector3(0f, 0f, -0.3f);

		public PawnFootprintMaker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void FootprintMakerTick()
		{
			if (!this.pawn.RaceProps.makesFootprints)
			{
				return;
			}
			if ((this.pawn.Drawer.DrawPos - this.lastFootprintPlacePos).MagnitudeHorizontalSquared() > 0.4f)
			{
				this.TryPlaceFootprint();
			}
		}

		private void TryPlaceFootprint()
		{
			if (!this.pawn.Map.terrainGrid.TerrainAt(this.pawn.Position).takeFootprints && this.pawn.Map.snowGrid.GetDepth(this.pawn.Position) < 0.4f)
			{
				return;
			}
			Vector3 drawPos = this.pawn.Drawer.DrawPos;
			Vector3 normalized = (drawPos - this.lastFootprintPlacePos).normalized;
			float rot = normalized.AngleFlat();
			float angle = (float)((!this.lastFootprintRight) ? -90 : 90);
			Vector3 b = normalized.RotatedBy(angle) * 0.17f;
			Vector3 loc = drawPos + PawnFootprintMaker.FootprintOffset + b;
			MoteMaker.PlaceFootprint(loc, this.pawn.Map, rot);
			this.lastFootprintPlacePos = drawPos;
			this.lastFootprintRight = !this.lastFootprintRight;
		}
	}
}
