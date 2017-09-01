using System;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EdgeMannedMortar : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			CellRect cellRect;
			return base.CanResolve(rp) && this.TryFindRandomInnerRectTouchingEdge(rp.rect, out cellRect);
		}

		public override void Resolve(ResolveParams rp)
		{
			CellRect rect;
			if (!this.TryFindRandomInnerRectTouchingEdge(rp.rect, out rect))
			{
				return;
			}
			Rot4 value;
			if (rect.Cells.Any((IntVec3 x) => x.x == rp.rect.minX))
			{
				value = Rot4.West;
			}
			else if (rect.Cells.Any((IntVec3 x) => x.x == rp.rect.maxX))
			{
				value = Rot4.East;
			}
			else if (rect.Cells.Any((IntVec3 x) => x.z == rp.rect.minZ))
			{
				value = Rot4.South;
			}
			else
			{
				value = Rot4.North;
			}
			ResolveParams rp2 = rp;
			rp2.rect = rect;
			rp2.thingRot = new Rot4?(value);
			BaseGen.symbolStack.Push("mannedMortar", rp2);
		}

		private bool TryFindRandomInnerRectTouchingEdge(CellRect rect, out CellRect mortarRect)
		{
			Map map = BaseGen.globalSettings.map;
			IntVec2 size = new IntVec2(3, 3);
			return rect.TryFindRandomInnerRectTouchingEdge(size, out mortarRect, (CellRect x) => x.Cells.All((IntVec3 y) => y.Standable(map) && y.GetEdifice(map) == null) && GenConstruct.TerrainCanSupport(x, map, ThingDefOf.Turret_MortarBomb)) || rect.TryFindRandomInnerRectTouchingEdge(size, out mortarRect, (CellRect x) => x.Cells.All((IntVec3 y) => y.Standable(map) && y.GetEdifice(map) == null));
		}
	}
}
