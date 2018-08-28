using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public static class SitePartUtility
	{
		public static string GetDescriptionDialogue(Site site, SitePart sitePart)
		{
			if (sitePart != null && !sitePart.def.alwaysHidden)
			{
				return sitePart.def.Worker.GetPostProcessedDescriptionDialogue(site, sitePart);
			}
			return "HiddenOrNoSitePartDescription".Translate();
		}
	}
}
