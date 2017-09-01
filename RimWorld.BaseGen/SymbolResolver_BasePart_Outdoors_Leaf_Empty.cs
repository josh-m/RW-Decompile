using System;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Outdoors_Leaf_Empty : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			return base.CanResolve(rp) && BaseGen.globalSettings.basePart_buildingsResolved >= BaseGen.globalSettings.minBuildings;
		}

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.globalSettings.basePart_emptyNodesResolved++;
		}
	}
}
