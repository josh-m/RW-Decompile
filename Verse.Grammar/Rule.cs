using System;

namespace Verse.Grammar
{
	public abstract class Rule
	{
		public string keyword;

		public abstract float BaseSelectionWeight
		{
			get;
		}

		public abstract string Generate();

		public virtual void Init()
		{
		}
	}
}
