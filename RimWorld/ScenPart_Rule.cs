using System;

namespace RimWorld
{
	public abstract class ScenPart_Rule : ScenPart
	{
		public override void PostMapGenerate()
		{
			this.ApplyRule();
		}

		protected abstract void ApplyRule();
	}
}
