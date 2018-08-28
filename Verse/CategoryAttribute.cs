using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CategoryAttribute : Attribute
	{
		public string name;

		public CategoryAttribute(string name)
		{
			this.name = name;
		}
	}
}
