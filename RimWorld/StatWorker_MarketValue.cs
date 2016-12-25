using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker_MarketValue : StatWorker
	{
		public const float ValuePerWork = 0.004f;

		private const float DefaultGuessStuffCost = 2f;

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			if (req.HasThing && req.Thing is Pawn)
			{
				return base.GetValueUnfinalized(StatRequest.For(req.Def, req.StuffDef), applyPostProcess) * PriceUtility.PawnQualityPriceFactor((Pawn)req.Thing);
			}
			if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
			{
				return base.GetValueUnfinalized(req, true);
			}
			float num = 0f;
			if (req.Def.costList != null)
			{
				for (int i = 0; i < req.Def.costList.Count; i++)
				{
					num += (float)req.Def.costList[i].count * req.Def.costList[i].thingDef.BaseMarketValue;
				}
			}
			if (req.Def.costStuffCount > 0)
			{
				if (req.StuffDef != null)
				{
					num += (float)req.Def.costStuffCount / req.StuffDef.VolumePerUnit * req.StuffDef.GetStatValueAbstract(StatDefOf.MarketValue, null);
				}
				else
				{
					num += (float)req.Def.costStuffCount * 2f;
				}
			}
			float num2 = Mathf.Max(req.Def.GetStatValueAbstract(StatDefOf.WorkToMake, req.StuffDef), req.Def.GetStatValueAbstract(StatDefOf.WorkToBuild, req.StuffDef));
			return num + num2 * 0.004f;
		}

		public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
		{
			if (req.HasThing && req.Thing is Pawn)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.GetExplanation(req, numberSense));
				Pawn pawn = req.Thing as Pawn;
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatsReport_CharacterQuality".Translate() + ": x" + PriceUtility.PawnQualityPriceFactor(pawn).ToStringPercent());
				return stringBuilder.ToString();
			}
			if (req.Def.statBases.StatListContains(StatDefOf.MarketValue))
			{
				return base.GetExplanation(req, numberSense);
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendLine("StatsReport_MarketValueFromStuffsAndWork".Translate());
			return stringBuilder2.ToString();
		}

		public override bool ShouldShowFor(BuildableDef def)
		{
			ThingDef thingDef = def as ThingDef;
			return thingDef != null && (TradeUtility.EverTradeable(thingDef) || thingDef.category == ThingCategory.Building);
		}
	}
}
