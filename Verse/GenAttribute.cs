using System;
using System.Linq;
using System.Reflection;

namespace Verse
{
	public static class GenAttribute
	{
		public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
		{
			T t;
			return memberInfo.TryGetAttribute(out t);
		}

		public static bool TryGetAttribute<T>(this MemberInfo memberInfo, out T customAttribute) where T : Attribute
		{
			object obj = memberInfo.GetCustomAttributes(typeof(T), true).FirstOrDefault<object>();
			if (obj == null)
			{
				customAttribute = (T)((object)null);
				return false;
			}
			customAttribute = (T)((object)obj);
			return true;
		}
	}
}
