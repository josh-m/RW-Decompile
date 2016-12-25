using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Slaughter : JobDriver
	{
		private const int SlaughterDuration = 180;

		protected Pawn Victim
		{
			get
			{
				return (Pawn)base.CurJob.targetA.Thing;
			}
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Slaughter);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil prepare = new Toil();
			prepare.initAction = delegate
			{
				PawnUtility.ForceWait((Pawn)this.<>f__this.CurJob.GetTarget(TargetIndex.A).Thing, 180, null);
			};
			prepare.defaultCompleteMode = ToilCompleteMode.Delay;
			prepare.defaultDuration = 180;
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate
				{
					IntVec3 position = this.<>f__this.Victim.Position;
					int num = Mathf.Max(GenMath.RoundRandom(this.<>f__this.Victim.BodySize * 3f), 1);
					for (int i = 0; i < num; i++)
					{
						this.<>f__this.Victim.health.DropBloodFilth();
					}
					BodyPartRecord bodyPartRecord = JobDriver_Slaughter.SlaughterCutPart(this.<>f__this.Victim);
					int amount = Mathf.Clamp((int)this.<>f__this.Victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 5);
					Thing arg_BF_0 = this.<>f__this.Victim;
					Pawn actor = this.<execute>__1.actor;
					arg_BF_0.TakeDamage(new DamageInfo(DamageDefOf.Cut, amount, -1f, actor, bodyPartRecord, null));
					if (!this.<>f__this.Victim.Dead)
					{
						this.<>f__this.Victim.health.Kill(null, null);
					}
					Thing thing = position.GetThingList(this.<>f__this.Map).FirstOrDefault((Thing t) => t is Corpse && ((Corpse)t).InnerPawn == this.<>f__this.Victim);
					if (thing != null)
					{
						thing.SetForbidden(false, true);
					}
					this.<>f__this.pawn.records.Increment(RecordDefOf.AnimalsSlaughtered);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		private static BodyPartRecord SlaughterCutPart(Pawn pawn)
		{
			BodyPartRecord bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.InsectHead);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Body);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Torso);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			return pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).RandomElementByWeight((BodyPartRecord x) => x.absoluteCoverage);
		}
	}
}
