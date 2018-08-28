using System;
using Verse;

namespace RimWorld
{
	public class Verb_MeleeApplyHediff : Verb_MeleeAttack
	{
		protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
		{
			DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
			if (this.tool == null)
			{
				Log.ErrorOnce("Attempted to apply melee hediff without a tool", 38381735, false);
				return damageResult;
			}
			Pawn pawn = target.Thing as Pawn;
			if (pawn == null)
			{
				Log.ErrorOnce("Attempted to apply melee hediff without pawn target", 78330053, false);
				return damageResult;
			}
			HediffSet arg_65_0 = pawn.health.hediffSet;
			BodyPartTagDef bodypartTagTarget = this.verbProps.bodypartTagTarget;
			foreach (BodyPartRecord current in arg_65_0.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, bodypartTagTarget, null))
			{
				damageResult.AddHediff(pawn.health.AddHediff(this.tool.hediff, current, null, null));
				damageResult.AddPart(pawn, current);
				damageResult.wounded = true;
			}
			return damageResult;
		}

		public override bool IsUsableOn(Thing target)
		{
			return target is Pawn;
		}
	}
}
