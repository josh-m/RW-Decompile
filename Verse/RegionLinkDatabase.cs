using System;
using System.Collections.Generic;
using System.Text;

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

		public void DebugLog()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<ulong, RegionLink> current in this.links)
			{
				stringBuilder.AppendLine(current.ToString());
			}
			Log.Message(stringBuilder.ToString(), false);
		}
	}
}
