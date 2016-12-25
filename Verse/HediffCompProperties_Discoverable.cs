using System;

namespace Verse
{
	public class HediffCompProperties_Discoverable : HediffCompProperties
	{
		public bool sendLetterWhenDiscovered;

		public string discoverLetterLabel;

		public string discoverLetterText;

		public HediffCompProperties_Discoverable()
		{
			this.compClass = typeof(HediffComp_Discoverable);
		}
	}
}
