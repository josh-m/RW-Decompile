using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ConfigurableHostilityResponse : ThinkNode_JobGiver
	{
		private static List<Thing> tmpThreats = new List<Thing>();

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.playerSettings == null || !pawn.playerSettings.UsesConfigurableHostilityResponse)
			{
				return null;
			}
			if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
			{
				return null;
			}
			switch (pawn.playerSettings.hostilityResponse)
			{
			case HostilityResponseMode.Ignore:
				return null;
			case HostilityResponseMode.Attack:
				return this.TryGetAttackNearbyEnemyJob(pawn);
			case HostilityResponseMode.Flee:
				return this.TryGetFleeJob(pawn);
			default:
				return null;
			}
		}

		private Job TryGetAttackNearbyEnemyJob(Pawn pawn)
		{
			if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			bool flag = pawn.equipment.Primary == null || pawn.equipment.Primary.def.IsMeleeWeapon;
			float num = 8f;
			if (!flag)
			{
				num = Mathf.Clamp(pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.range * 0.66f, 2f, 20f);
			}
			float maxDist = num;
			Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat, null, 0f, maxDist, default(IntVec3), 3.40282347E+38f, false);
			if (thing == null)
			{
				return null;
			}
			if (flag || pawn.CanReachImmediate(thing, PathEndMode.Touch))
			{
				return new Job(JobDefOf.AttackMelee, thing);
			}
			return new Job(JobDefOf.AttackStatic, thing)
			{
				maxNumStaticAttacks = 2,
				expiryInterval = 1800
			};
		}

		private Job TryGetFleeJob(Pawn pawn)
		{
			if (!SelfDefenseUtility.ShouldStartFleeing(pawn))
			{
				return null;
			}
			IntVec3 c;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
			{
				c = pawn.CurJob.targetA.Cell;
			}
			else
			{
				JobGiver_ConfigurableHostilityResponse.tmpThreats.Clear();
				List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
				for (int i = 0; i < potentialTargetsFor.Count; i++)
				{
					IAttackTarget attackTarget = potentialTargetsFor[i];
					if (!attackTarget.ThreatDisabled())
					{
						JobGiver_ConfigurableHostilityResponse.tmpThreats.Add((Thing)attackTarget);
					}
				}
				if (!JobGiver_ConfigurableHostilityResponse.tmpThreats.Any<Thing>())
				{
					Log.Warning(pawn.LabelShort + " decided to flee but there is no any threat around.");
					return null;
				}
				c = CellFinderLoose.GetFleeDest(pawn, JobGiver_ConfigurableHostilityResponse.tmpThreats, 23f);
				JobGiver_ConfigurableHostilityResponse.tmpThreats.Clear();
			}
			return new Job(JobDefOf.FleeAndCower, c);
		}
	}
}
