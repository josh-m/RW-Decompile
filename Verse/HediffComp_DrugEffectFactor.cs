using RimWorld;
using System;

namespace Verse
{
	public class HediffComp_DrugEffectFactor : HediffComp
	{
		private static readonly SimpleCurve EffectFactorSeverityCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 0.25f)
		};

		private float CurrentFactor
		{
			get
			{
				return HediffComp_DrugEffectFactor.EffectFactorSeverityCurve.Evaluate(this.parent.Severity);
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				return "DrugEffectMultiplier".Translate(new object[]
				{
					this.props.chemical
				}) + ": " + this.CurrentFactor.ToStringPercent();
			}
		}

		public override void CompFactorDrugEffect(ChemicalDef chem, ref float effect)
		{
			if (this.props.chemical == chem)
			{
				effect *= this.CurrentFactor;
			}
		}
	}
}
