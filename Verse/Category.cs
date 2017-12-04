using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Method)]
	public class Category : Attribute
	{
		public string name;

		public Category(string name)
		{
			this.name = name;
		}
	}
}
