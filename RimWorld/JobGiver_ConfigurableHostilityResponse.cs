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
			TargetScanFlags flags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat;
			float maxDist = num;
			Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, flags, null, 0f, maxDist, default(IntVec3), 3.40282347E+38f, false);
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
				expiryInterval = 2000,
				endIfCantShootTargetFromCurPos = true
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
					Thing thing = potentialTargetsFor[i].Thing;
					if (SelfDefenseUtility.ShouldFleeFrom(thing, pawn, false, false))
					{
						JobGiver_ConfigurableHostilityResponse.tmpThreats.Add(thing);
					}
				}
				List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing2 = list[j];
					if (SelfDefenseUtility.ShouldFleeFrom(thing2, pawn, false, false))
					{
						JobGiver_ConfigurableHostilityResponse.tmpThreats.Add(thing2);
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
