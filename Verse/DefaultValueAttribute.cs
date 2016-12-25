using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class DefaultValueAttribute : Attribute
	{
		public object value;

		public DefaultValueAttribute(object value)
		{
			this.value = value;
		}

		public virtual bool ObjIsDefault(object obj)
		{
			if (obj == null)
			{
				return this.value == null;
			}
			return this.value != null && this.value.Equals(obj);
		}
	}
}
