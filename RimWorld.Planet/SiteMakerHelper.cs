using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class SiteMakerHelper
	{
		private static List<Faction> possibleFactions = new List<Faction>();

		public static bool TryFindSiteParams_SingleSitePart(SiteCoreDef core, IEnumerable<SitePartDef> singleSitePartCandidates, out SitePartDef sitePart, out Faction faction, Faction factionToUse = null, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			faction = factionToUse;
			if (singleSitePartCandidates != null)
			{
				if (!SiteMakerHelper.TryFindNewRandomSitePartFor(core, null, singleSitePartCandidates, faction, out sitePart, disallowAlliedFactions, extraFactionValidator))
				{
					return false;
				}
			}
			else
			{
				sitePart = null;
			}
			if (faction == null)
			{
				IEnumerable<SitePartDef> parts = (sitePart == null) ? null : Gen.YieldSingle<SitePartDef>(sitePart);
				if (!SiteMakerHelper.TryFindRandomFactionFor(core, parts, out faction, disallowAlliedFactions, extraFactionValidator))
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryFindSiteParams_SingleSitePart(SiteCoreDef core, string singleSitePartTag, out SitePartDef sitePart, out Faction faction, Faction factionToUse = null, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			IEnumerable<SitePartDef> singleSitePartCandidates = (singleSitePartTag == null) ? null : (from x in DefDatabase<SitePartDef>.AllDefsListForReading
			where x.tags.Contains(singleSitePartTag)
			select x);
			return SiteMakerHelper.TryFindSiteParams_SingleSitePart(core, singleSitePartCandidates, out sitePart, out faction, factionToUse, disallowAlliedFactions, extraFactionValidator);
		}

		public static bool TryFindNewRandomSitePartFor(SiteCoreDef core, IEnumerable<SitePartDef> existingSiteParts, IEnumerable<SitePartDef> possibleSiteParts, Faction faction, out SitePartDef sitePart, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			if (faction != null)
			{
				if ((from x in possibleSiteParts
				where x == null || SiteMakerHelper.FactionCanOwn(x, faction, disallowAlliedFactions, extraFactionValidator)
				select x).TryRandomElement(out sitePart))
				{
					return true;
				}
			}
			else
			{
				SiteMakerHelper.possibleFactions.Clear();
				SiteMakerHelper.possibleFactions.Add(null);
				SiteMakerHelper.possibleFactions.AddRange(Find.FactionManager.AllFactionsListForReading);
				if ((from x in possibleSiteParts
				where x == null || SiteMakerHelper.possibleFactions.Any((Faction fac) => SiteMakerHelper.FactionCanOwn(core, existingSiteParts, fac, disallowAlliedFactions, extraFactionValidator) && SiteMakerHelper.FactionCanOwn(x, fac, disallowAlliedFactions, extraFactionValidator))
				select x).TryRandomElement(out sitePart))
				{
					SiteMakerHelper.possibleFactions.Clear();
					return true;
				}
				SiteMakerHelper.possibleFactions.Clear();
			}
			sitePart = null;
			return false;
		}

		public static bool TryFindRandomFactionFor(SiteCoreDef core, IEnumerable<SitePartDef> parts, out Faction faction, bool disallowAlliedFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			if (SiteMakerHelper.FactionCanOwn(core, parts, null, disallowAlliedFactions, extraFactionValidator))
			{
				faction = null;
				return true;
			}
			if ((from x in Find.FactionManager.AllFactionsListForReading
			where SiteMakerHelper.FactionCanOwn(core, parts, x, disallowAlliedFactions, extraFactionValidator)
			select x).TryRandomElement(out faction))
			{
				return true;
			}
			faction = null;
			return false;
		}

		public static bool FactionCanOwn(SiteCoreDef core, IEnumerable<SitePartDef> parts, Faction faction, bool disallowAlliedFactions, Predicate<Faction> extraFactionValidator)
		{
			if (!SiteMakerHelper.FactionCanOwn(core, faction, disallowAlliedFactions, extraFactionValidator))
			{
				return false;
			}
			if (parts != null)
			{
				foreach (SitePartDef current in parts)
				{
					if (!SiteMakerHelper.FactionCanOwn(current, faction, disallowAlliedFactions, extraFactionValidator))
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		private static bool FactionCanOwn(SiteDefBase siteDefBase, Faction faction, bool disallowAlliedFactions, Predicate<Faction> extraFactionValidator)
		{
			if (siteDefBase == null)
			{
				Log.Error("Called FactionCanOwn() with null SiteDefBase.");
				return false;
			}
			return siteDefBase.FactionCanOwn(faction) && (!disallowAlliedFactions || faction == null || faction.HostileTo(Faction.OfPlayer)) && (extraFactionValidator == null || extraFactionValidator(faction));
		}
	}
}
