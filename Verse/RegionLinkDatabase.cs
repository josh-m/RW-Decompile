using System;
using System.Collections.Generic;

namespace Verse
{
	public class RegionLinkDatabase
	{
		private Dictionary<ulong, RegionLink> links = new Dictionary<ulong, RegionLink>();

		public RegionLink LinkFrom(EdgeSpan span)
		{
			ulong key = span.UniqueHashCode();
			RegionLink regionLink;
			if (!this.links.TryGetValue(key, out regionLink))
			{
				regionLink = new RegionLink();
				regionLink.span = span;
				this.links.Add(key, regionLink);
			}
			return regionLink;
		}

		public void Notify_LinkHasNoRegions(RegionLink link)
		{
			this.links.Remove(link.UniqueHashCode());
		}
	}
}
