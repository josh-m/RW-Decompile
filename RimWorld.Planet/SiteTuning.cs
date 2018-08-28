using System;
using Verse;

namespace RimWorld.Planet
{
	public static class SiteTuning
	{
		public static readonly IntRange QuestSiteTimeoutDaysRange = new IntRange(12, 28);

		public static readonly IntRange QuestSiteRefugeeTimeoutDaysRange = new IntRange(7, 15);

		public static readonly IntRange MineralScannerPreciousLumpTimeoutDaysRange = new IntRange(30, 30);

		public static readonly IntRange PrisonerRescueQuestSiteDistanceRange = new IntRange(2, 18);

		public static readonly IntRange PeaceTalksQuestSiteDistanceRange = new IntRange(5, 13);

		public static readonly IntRange DownedRefugeeQuestSiteDistanceRange = new IntRange(2, 13);

		public static readonly IntRange PreciousLumpSiteDistanceRange = new IntRange(7, 27);

		public static readonly IntRange BanditCampQuestSiteDistanceRange = new IntRange(7, 27);

		public static readonly IntRange ItemStashQuestSiteDistanceRange = new IntRange(7, 27);

		public static readonly FloatRange SitePointRandomFactorRange = new FloatRange(0.7f, 1.3f);

		public static readonly SimpleCurve ThreatPointsToSiteThreatPointsCurve = new SimpleCurve
		{
			{
				new CurvePoint(100f, 120f),
				true
			},
			{
				new CurvePoint(1000f, 300f),
				true
			},
			{
				new CurvePoint(2000f, 600f),
				true
			},
			{
				new CurvePoint(3000f, 800f),
				true
			},
			{
				new CurvePoint(4000f, 900f),
				true
			},
			{
				new CurvePoint(5000f, 1000f),
				true
			}
		};

		public static readonly SimpleCurve QuestRewardMarketValueThreatPointsFactor = new SimpleCurve
		{
			{
				new CurvePoint(300f, 0.8f),
				true
			},
			{
				new CurvePoint(700f, 1f),
				true
			}
		};

		public static readonly FloatRange BanditCampQuestRewardMarketValueRange = new FloatRange(1900f, 3500f);

		public static readonly FloatRange ItemStashQuestMarketValueRange = new FloatRange(1900f, 3500f);

		public const string PrisonerRescueQuestThreatTag = "PrisonerRescueQuestThreat";

		public const string DownedRefugeeQuestThreatTag = "DownedRefugeeQuestThreat";

		public const string MineralScannerPreciousLumpThreatTag = "MineralScannerPreciousLumpThreat";

		public const string ItemStashQuestThreatTag = "ItemStashQuestThreat";

		public const float DownedRefugeeNoSitePartChance = 0.3f;

		public const float MineralScannerPreciousLumpNoSitePartChance = 0.6f;

		public const float ItemStashNoSitePartChance = 0.15f;
	}
}
