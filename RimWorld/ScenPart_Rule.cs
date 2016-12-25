using System;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_Rule : ScenPart
	{
		public override void PostMapGenerate(Map map)
		{
			this.ApplyRule();
		}

		protected abstract void ApplyRule();
	}
}
