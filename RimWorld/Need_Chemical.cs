using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_Chemical : Need
	{
		private const float ThreshDesire = 0.01f;

		private const float ThreshSatisfied = 0.3f;

		public override int GUIChangeArrow
		{
			get
			{
				return -1;
			}
		}

		public DrugDesireCategory CurCategory
		{
			get
			{
				if (this.CurLevel > 0.3f)
				{
					return DrugDesireCategory.Satisfied;
				}
				if (this.CurLevel > 0.01f)
				{
					return DrugDesireCategory.Desire;
				}
				return DrugDesireCategory.Withdrawal;
			}
		}

		public override float CurLevel
		{
			get
			{
				return base.CurLevel;
			}
			set
			{
				DrugDesireCategory curCategory = this.CurCategory;
				base.CurLevel = value;
				if (this.CurCategory != curCategory)
				{
					this.CategoryChanged();
				}
			}
		}

		public Hediff_Addiction AddictionHediff
		{
			get
			{
				List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
					if (hediff_Addiction != null && hediff_Addiction.def.causesNeed == this.def)
					{
						return hediff_Addiction;
					}
				}
				return null;
			}
		}

		private float ChemicalFallPerTick
		{
			get
			{
				return this.def.fallPerDay / 60000f;
			}
		}

		public Need_Chemical(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			this.threshPercents.Add(0.3f);
		}

		public override void SetInitialLevel()
		{
			base.CurLevelPercentage = Rand.Range(0.8f, 1f);
		}

		public override void NeedInterval()
		{
			this.CurLevel -= this.ChemicalFallPerTick * 150f;
		}

		private void CategoryChanged()
		{
			Hediff_Addiction addictionHediff = this.AddictionHediff;
			if (addictionHediff != null)
			{
				addictionHediff.Notify_NeedCategoryChanged();
			}
		}
	}
}
