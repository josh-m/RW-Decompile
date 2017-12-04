using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class SiteMaker
	{
		public static Site MakeSite(SiteCoreDef core, SitePartDef sitePart, Faction faction)
		{
			IEnumerable<SitePartDef> siteParts = (sitePart == null) ? null : Gen.YieldSingle<SitePartDef>(sitePart);
			return SiteMaker.MakeSite(core, siteParts, faction);
		}

		public static Site MakeSite(SiteCoreDef core, IEnumerable<SitePartDef> siteParts, Faction faction)
		{
			Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
			site.core = core;
			site.SetFaction(faction);
			if (siteParts != null)
			{
				site.parts.AddRange(siteParts);
			}
			return site;
		}

		public static Site TryMakeSite_SingleSitePart(SiteCoreDef core, IEnumerable<SitePartDef> singleSitePartCandidates, Faction faction = null, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			SitePartDef sitePart;
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(core, singleSitePartCandidates, out sitePart, out faction, faction, disallowAlliedFactions, extraFactionValidator))
			{
				return null;
			}
			return SiteMaker.MakeSite(core, sitePart, faction);
		}

		public static Site TryMakeSite_SingleSitePart(SiteCoreDef core, string singleSitePartTag, Faction faction = null, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			SitePartDef sitePart;
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(core, singleSitePartTag, out sitePart, out faction, faction, disallowAlliedFactions, extraFactionValidator))
			{
				return null;
			}
			return SiteMaker.MakeSite(core, sitePart, faction);
		}

		public static Site TryMakeSite(SiteCoreDef core, IEnumerable<SitePartDef> siteParts, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			Faction faction;
			if (!SiteMakerHelper.TryFindRandomFactionFor(core, siteParts, out faction, disallowAlliedFactions, extraFactionValidator))
			{
				return null;
			}
			return SiteMaker.MakeSite(core, siteParts, faction);
		}
	}
}
