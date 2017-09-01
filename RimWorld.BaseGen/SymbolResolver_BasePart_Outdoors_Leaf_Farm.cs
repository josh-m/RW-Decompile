using System;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_BasePart_Outdoors_Leaf_Farm : SymbolResolver
	{
		private const float MaxCoverage = 0.55f;

		public override bool CanResolve(ResolveParams rp)
		{
			return base.CanResolve(rp) && BaseGen.globalSettings.basePart_buildingsResolved >= BaseGen.globalSettings.minBuildings && BaseGen.globalSettings.basePart_emptyNodesResolved >= BaseGen.globalSettings.minEmptyNodes && BaseGen.globalSettings.basePart_farmsCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area < 0.55f && (rp.rect.Width <= 15 && rp.rect.Height <= 15) && (rp.cultivatedPlantDef != null || SymbolResolver_CultivatedPlants.DeterminePlantDef(rp.rect) != null);
		}

		public override void Resolve(ResolveParams rp)
		{
			BaseGen.symbolStack.Push("farm", rp);
			BaseGen.globalSettings.basePart_farmsCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
		}
	}
}
