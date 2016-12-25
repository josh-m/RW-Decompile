using System;

namespace Verse
{
	public class ResearchMod
	{
		private Action specialAction;

		public void Apply()
		{
			if (this.specialAction != null)
			{
				this.specialAction();
			}
		}
	}
}
