using System;

namespace Verse
{
	public class HediffCompProperties_Effecter : HediffCompProperties
	{
		public EffecterDef stateEffecter;

		public IntRange severityIndices = new IntRange(-1, -1);

		public HediffCompProperties_Effecter()
		{
			this.compClass = typeof(HediffComp_Effecter);
		}
	}
}
