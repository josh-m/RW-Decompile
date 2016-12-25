using System;
using System.Collections.Generic;

namespace Verse
{
	public class HediffCompProperties_VerbGiver : HediffCompProperties
	{
		public List<VerbProperties> verbs;

		public HediffCompProperties_VerbGiver()
		{
			this.compClass = typeof(HediffComp_VerbGiver);
		}
	}
}
