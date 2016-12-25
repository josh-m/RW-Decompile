using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_WantToSleepWithSpouseOrLover : Thought_Situational
	{
		public override string LabelCap
		{
			get
			{
				DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(this.pawn, false);
				return string.Format(base.CurStage.label, directPawnRelation.otherPawn.LabelShort).CapitalizeFirst();
			}
		}

		protected override float BaseMoodOffset
		{
			get
			{
				float a = -0.05f * (float)this.pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(this.pawn, false).otherPawn);
				return Mathf.Min(a, -1f);
			}
		}
	}
}
