using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Roof : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			if (rp.noRoof.HasValue && rp.noRoof.Value)
			{
				return;
			}
			RoofGrid roofGrid = BaseGen.globalSettings.map.roofGrid;
			RoofDef def = rp.roofDef ?? RoofDefOf.RoofConstructed;
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!roofGrid.Roofed(current))
				{
					roofGrid.SetRoof(current, def);
				}
				iterator.MoveNext();
			}
		}
	}
}
