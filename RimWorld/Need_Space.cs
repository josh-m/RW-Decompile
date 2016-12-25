using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_Space : Need_Seeker
	{
		private const float MinCramped = 0.01f;

		private const float MinNormal = 0.3f;

		private const float MinSpacious = 0.7f;

		public override float CurInstantLevel
		{
			get
			{
				return BeautyUtility.SpacePerceptible(this.pawn.Position);
			}
		}

		public SpaceCategory CurCategory
		{
			get
			{
				if (this.CurLevel < 0.01f)
				{
					return SpaceCategory.VeryCramped;
				}
				if (this.CurLevel < 0.3f)
				{
					return SpaceCategory.Cramped;
				}
				if (this.CurLevel < 0.7f)
				{
					return SpaceCategory.Normal;
				}
				return SpaceCategory.Spacious;
			}
		}

		public Need_Space(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			this.threshPercents.Add(0.3f);
			this.threshPercents.Add(0.7f);
		}
	}
}
