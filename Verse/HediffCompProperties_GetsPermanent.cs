using System;

namespace Verse
{
	public class HediffCompProperties_GetsPermanent : HediffCompProperties
	{
		public float becomePermanentChanceFactor = 1f;

		public string permanentLabel;

		public string instantlyPermanentLabel;

		public HediffCompProperties_GetsPermanent()
		{
			this.compClass = typeof(HediffComp_GetsPermanent);
		}
	}
}
