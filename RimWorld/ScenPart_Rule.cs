using System;

namespace RimWorld
{
	public abstract class ScenPart_Rule : ScenPart
	{
		public override void PostGameStart()
		{
			this.ApplyRule();
		}

		protected abstract void ApplyRule();
	}
}
