using System;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Indoors_Leaf_Storage : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			return base.CanResolve(rp) && BaseGen.globalSettings.basePart_barracksResolved >= BaseGen.globalSettings.minBarracks;
		}

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("storage", rp);
		}
	}
}
