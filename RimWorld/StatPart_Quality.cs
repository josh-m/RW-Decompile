using System;
using Verse;

namespace RimWorld
{
	public class StatPart_Quality : StatPart
	{
		private float factorAwful = 1f;

		private float factorShoddy = 1f;

		private float factorPoor = 1f;

		private float factorNormal = 1f;

		private float factorGood = 1f;

		private float factorSuperior = 1f;

		private float factorExcellent = 1f;

		private float factorMasterwork = 1f;

		private float factorLegendary = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (val <= 0f)
			{
				return;
			}
			QualityCategory qc;
			if (req.HasThing && req.Thing.TryGetQuality(out qc))
			{
				val *= this.QualityMultiplier(qc);
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing.GetStatValue(this.parentStat, true) <= 0f)
			{
				return null;
			}
			QualityCategory qc;
			if (req.HasThing && req.Thing.TryGetQuality(out qc))
			{
				return "StatsReport_QualityMultiplier".Translate() + ": x" + this.QualityMultiplier(qc).ToStringPercent();
			}
			return null;
		}

		private float QualityMultiplier(QualityCategory qc)
		{
			switch (qc)
			{
			case QualityCategory.Awful:
				return this.factorAwful;
			case QualityCategory.Shoddy:
				return this.factorShoddy;
			case QualityCategory.Poor:
				return this.factorPoor;
			case QualityCategory.Normal:
				return this.factorNormal;
			case QualityCategory.Good:
				return this.factorGood;
			case QualityCategory.Superior:
				return this.factorSuperior;
			case QualityCategory.Excellent:
				return this.factorExcellent;
			case QualityCategory.Masterwork:
				return this.factorMasterwork;
			case QualityCategory.Legendary:
				return this.factorLegendary;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
