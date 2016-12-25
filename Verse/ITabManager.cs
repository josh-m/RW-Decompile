using System;
using System.Collections.Generic;

namespace Verse
{
	public static class ITabManager
	{
		private static Dictionary<Type, ITab> sharedInstances = new Dictionary<Type, ITab>();

		public static ITab GetSharedInstance(Type tabType)
		{
			ITab tab;
			if (ITabManager.sharedInstances.TryGetValue(tabType, out tab))
			{
				return tab;
			}
			tab = (ITab)Activator.CreateInstance(tabType);
			ITabManager.sharedInstances.Add(tabType, tab);
			return tab;
		}
	}
}
