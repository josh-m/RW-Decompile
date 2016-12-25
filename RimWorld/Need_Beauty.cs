using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Beauty : Need_Seeker
	{
		private const float BeautyImpactFactor = 0.1f;

		private const float ThreshVeryUgly = 0.01f;

		private const float ThreshUgly = 0.15f;

		private const float ThreshNeutral = 0.35f;

		private const float ThreshPretty = 0.65f;

		private const float ThreshVeryPretty = 0.85f;

		private const float ThreshBeautiful = 0.99f;

		public override float CurInstantLevel
		{
			get
			{
				if (!this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
				{
					return 0.5f;
				}
				if (!this.pawn.Spawned)
				{
					return 0.5f;
				}
				return this.LevelFromBeauty(this.CurrentInstantBeauty());
			}
		}

		public BeautyCategory CurCategory
		{
			get
			{
				if (this.CurLevel > 0.99f)
				{
					return BeautyCategory.Beautiful;
				}
				if (this.CurLevel > 0.85f)
				{
					return BeautyCategory.VeryPretty;
				}
				if (this.CurLevel > 0.65f)
				{
					return BeautyCategory.Pretty;
				}
				if (this.CurLevel > 0.35f)
				{
					return BeautyCategory.Neutral;
				}
				if (this.CurLevel > 0.15f)
				{
					return BeautyCategory.Ugly;
				}
				if (this.CurLevel > 0.01f)
				{
					return BeautyCategory.VeryUgly;
				}
				return BeautyCategory.Hideous;
			}
		}

		public Need_Beauty(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			this.threshPercents.Add(0.15f);
			this.threshPercents.Add(0.35f);
			this.threshPercents.Add(0.65f);
			this.threshPercents.Add(0.85f);
		}

		private float LevelFromBeauty(float beauty)
		{
			return Mathf.Clamp01(this.def.baseLevel + beauty * 0.1f);
		}

		public float CurrentInstantBeauty()
		{
			if (this.pawn.MapHeld == null)
			{
				return 0.5f;
			}
			return BeautyUtility.AverageBeautyPerceptible(this.pawn.PositionHeld, this.pawn.MapHeld);
		}
	}
}
