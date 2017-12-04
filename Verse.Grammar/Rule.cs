using System;
using System.Collections.Generic;

namespace Verse.Grammar
{
	public abstract class Rule
	{
		public struct ConstrantConstraint
		{
			public string key;

			public string value;

			public bool equality;
		}

		public string keyword;

		public List<Rule.ConstrantConstraint> constantConstraints;

		public abstract float BaseSelectionWeight
		{
			get;
		}

		public abstract string Generate();

		public virtual void Init()
		{
		}

		public void AddConstantConstraint(string key, string value, bool equality)
		{
			if (this.constantConstraints == null)
			{
				this.constantConstraints = new List<Rule.ConstrantConstraint>();
			}
			this.constantConstraints.Add(new Rule.ConstrantConstraint
			{
				key = key,
				value = value,
				equality = equality
			});
		}
	}
}
