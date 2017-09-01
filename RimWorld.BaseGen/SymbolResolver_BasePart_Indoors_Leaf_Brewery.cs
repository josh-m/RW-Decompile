using System;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Indoors_Leaf_Brewery : SymbolResolver
	{
		private const float MaxCoverage = 0.08f;

		public override bool CanResolve(ResolveParams rp)
		{
			return base.CanResolve(rp) && BaseGen.globalSettings.basePart_barracksResolved >= BaseGen.globalSettings.minBarracks && BaseGen.globalSettings.basePart_breweriesCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area < 0.08f && (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Medieval);
		}

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("brewery", rp);
			BaseGen.globalSettings.basePart_breweriesCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
		}
	}
}
