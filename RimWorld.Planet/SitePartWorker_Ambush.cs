using System;

namespace RimWorld.Planet
{
	public class SitePartWorker_Ambush : SitePartWorker
	{
		private const float ThreatPointsFactor = 0.8f;

		public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
			siteCoreOrPartParams.threatPoints *= 0.8f;
			return siteCoreOrPartParams;
		}
	}
}
