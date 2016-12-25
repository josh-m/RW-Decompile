using System;

namespace Verse.Sound
{
	public class SustainerScopeFader
	{
		private const float ScopeMatchFallRate = 0.03f;

		private const float ScopeMatchRiseRate = 0.05f;

		public bool inScope = true;

		public float inScopePercent = 1f;

		public void SustainerScopeUpdate()
		{
			if (this.inScope)
			{
				float num = this.inScopePercent + 0.05f;
				this.inScopePercent = num;
				if (this.inScopePercent > 1f)
				{
					this.inScopePercent = 1f;
				}
			}
			else
			{
				this.inScopePercent -= 0.03f;
				if (this.inScopePercent <= 0.001f)
				{
					this.inScopePercent = 0f;
				}
			}
		}
	}
}
