using System;

namespace Verse
{
	public static class LoaderHelper
	{
		public static T TryResolveDef<T>(string defName, FailMode failReportMode)
		{
			T t = (T)((object)GenDefDatabase.GetDefSilentFail(typeof(T), defName));
			if (t != null)
			{
				return t;
			}
			if (failReportMode == FailMode.LogErrors)
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not resolve cross-reference to ",
					typeof(T),
					" named ",
					defName
				}));
			}
			return default(T);
		}
	}
}
