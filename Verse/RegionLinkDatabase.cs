using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RegionLinkDatabase
	{
		private static Dictionary<ulong, RegionLink> links;

		public static void Reinit()
		{
			RegionLinkDatabase.links = new Dictionary<ulong, RegionLink>();
		}

		public static RegionLink LinkFrom(EdgeSpan span)
		{
			ulong key = span.UniqueHashCode();
			RegionLink regionLink;
			if (!RegionLinkDatabase.links.TryGetValue(key, out regionLink))
			{
				regionLink = new RegionLink();
				regionLink.span = span;
				RegionLinkDatabase.links.Add(key, regionLink);
			}
			return regionLink;
		}

		public static void Notify_LinkHasNoRegions(RegionLink link)
		{
			RegionLinkDatabase.links.Remove(link.UniqueHashCode());
		}
	}
}
