using RimWorld;
using System;

namespace Verse
{
	public class HediffCompProperties_DrugEffectFactor : HediffCompProperties
	{
		public ChemicalDef chemical;

		public HediffCompProperties_DrugEffectFactor()
		{
			this.compClass = typeof(HediffComp_DrugEffectFactor);
		}
	}
}
