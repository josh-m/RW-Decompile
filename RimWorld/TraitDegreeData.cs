using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TraitDegreeData
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string description;

		public int degree;

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		public ThinkTreeDef thinkTree;

		private float commonality = -1f;

		public MentalStateDef randomMentalState;

		public SimpleCurve randomMentalStateMtbDaysMoodCurve;

		public List<MentalStateDef> disallowedMentalStates;

		public List<MentalBreakDef> allowedMentalBreaks;

		public Dictionary<SkillDef, int> skillGains = new Dictionary<SkillDef, int>();

		public float socialFightChanceFactor = 1f;

		public float Commonality
		{
			get
			{
				if (this.commonality >= 0f)
				{
					return this.commonality;
				}
				switch (Mathf.Abs(this.degree))
				{
				case 0:
					return 1f;
				case 1:
					return 1f;
				case 2:
					return 0.4f;
				case 3:
					return 0.2f;
				case 4:
					return 0.1f;
				default:
					return 0.05f;
				}
			}
		}
	}
}
