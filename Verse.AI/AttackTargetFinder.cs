using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public static class AttackTargetFinder
	{
		private static List<IAttackTarget> tmpTargets = new List<IAttackTarget>();

		private static List<Pair<IAttackTarget, float>> availableShootingTargets = new List<Pair<IAttackTarget, float>>();

		private static List<float> tmpTargetScores = new List<float>();

		private static List<bool> tmpCanShootAtTarget = new List<bool>();

		private static List<IntVec3> tempDestList = new List<IntVec3>();

		private static List<IntVec3> tempSourceList = new List<IntVec3>();

		public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0f, float maxDist = 9999f, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBash = false)
		{
			Thing searcherThing = searcher.Thing;
			Pawn searcherPawn = searcher as Pawn;
			Verb verb = searcher.CurrentEffectiveVerb;
			if (verb == null)
			{
				Log.Error("BestAttackTarget with " + searcher + " who has no attack verb.");
				return null;
			}
			bool onlyTargetMachines = verb != null && verb.verbProps.projectileDef != null && verb.verbProps.projectileDef.projectile.damageDef == DamageDefOf.EMP;
			float minDistanceSquared = minDist * minDist;
			float num = maxTravelRadiusFromLocus + verb.verbProps.range;
			float maxLocusDistSquared = num * num;
			Func<IntVec3, bool> losValidator = null;
			if ((byte)(flags & TargetScanFlags.LOSBlockableByGas) != 0)
			{
				losValidator = delegate(IntVec3 vec3)
				{
					Thing gas = vec3.GetGas(searcherThing.Map);
					return gas == null || !gas.def.gas.blockTurretTracking;
				};
			}
			Predicate<IAttackTarget> innerValidator = delegate(IAttackTarget t)
			{
				Thing thing = t.Thing;
				if (t == searcher)
				{
					return false;
				}
				if (minDistanceSquared > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < minDistanceSquared)
				{
					return false;
				}
				if (maxTravelRadiusFromLocus < 9999f && (float)(thing.Position - locus).LengthHorizontalSquared > maxLocusDistSquared)
				{
					return false;
				}
				if (!searcherThing.HostileTo(thing))
				{
					return false;
				}
				if (validator != null && !validator(thing))
				{
					return false;
				}
				if ((byte)(flags & TargetScanFlags.NeedLOSToAll) != 0 && !searcherThing.CanSee(thing, losValidator))
				{
					if (t is Pawn)
					{
						if ((byte)(flags & TargetScanFlags.NeedLOSToPawns) != 0)
						{
							return false;
						}
					}
					else if ((byte)(flags & TargetScanFlags.NeedLOSToNonPawns) != 0)
					{
						return false;
					}
				}
				if ((byte)(flags & TargetScanFlags.NeedThreat) != 0 && t.ThreatDisabled())
				{
					return false;
				}
				Pawn pawn = t as Pawn;
				if (onlyTargetMachines && pawn != null && pawn.RaceProps.IsFlesh)
				{
					return false;
				}
				if ((byte)(flags & TargetScanFlags.NeedNonBurning) != 0 && thing.IsBurning())
				{
					return false;
				}
				if (searcherThing.def.race != null && searcherThing.def.race.intelligence >= Intelligence.Humanlike)
				{
					CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
					if (compExplosive != null && compExplosive.wickStarted)
					{
						return false;
					}
				}
				return true;
			};
			if (AttackTargetFinder.HasRangedAttack(searcher))
			{
				AttackTargetFinder.tmpTargets.Clear();
				AttackTargetFinder.tmpTargets.AddRange(searcherThing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher));
				if ((byte)(flags & TargetScanFlags.NeedReachable) != 0)
				{
					Predicate<IAttackTarget> oldValidator = innerValidator;
					innerValidator = ((IAttackTarget t) => oldValidator(t) && AttackTargetFinder.CanReach(searcherThing, t.Thing, canBash));
				}
				bool flag = false;
				if (searcherThing.Faction != Faction.OfPlayer)
				{
					for (int i = 0; i < AttackTargetFinder.tmpTargets.Count; i++)
					{
						IAttackTarget attackTarget = AttackTargetFinder.tmpTargets[i];
						if (attackTarget.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) && innerValidator(attackTarget) && AttackTargetFinder.CanShootAtFromCurrentPosition(attackTarget, searcher, verb))
						{
							flag = true;
							break;
						}
					}
				}
				IAttackTarget result;
				if (flag)
				{
					AttackTargetFinder.tmpTargets.RemoveAll((IAttackTarget x) => !x.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) || !innerValidator(x));
					result = AttackTargetFinder.GetRandomShootingTargetByScore(AttackTargetFinder.tmpTargets, searcher, verb);
				}
				else
				{
					Predicate<Thing> validator2;
					if ((byte)(flags & TargetScanFlags.NeedReachableIfCantHitFromMyPos) != 0 && (byte)(flags & TargetScanFlags.NeedReachable) == 0)
					{
						validator2 = ((Thing t) => innerValidator((IAttackTarget)t) && (AttackTargetFinder.CanReach(searcherThing, t, canBash) || AttackTargetFinder.CanShootAtFromCurrentPosition((IAttackTarget)t, searcher, verb)));
					}
					else
					{
						validator2 = ((Thing t) => innerValidator((IAttackTarget)t));
					}
					result = (IAttackTarget)GenClosest.ClosestThing_Global(searcherThing.Position, AttackTargetFinder.tmpTargets, maxDist, validator2);
				}
				AttackTargetFinder.tmpTargets.Clear();
				return result;
			}
			if (searcherPawn != null && searcherPawn.mindState.duty != null && searcherPawn.mindState.duty.radius > 0f)
			{
				Predicate<IAttackTarget> oldValidator = innerValidator;
				innerValidator = ((IAttackTarget t) => oldValidator(t) && t.Thing.Position.InHorDistOf(searcherPawn.mindState.duty.focus.Cell, searcherPawn.mindState.duty.radius));
			}
			int searchRegionsMax = (maxDist <= 800f) ? 40 : -1;
			IntVec3 arg_415_0 = searcherThing.Position;
			Map arg_415_1 = searcherThing.Map;
			ThingRequest arg_415_2 = ThingRequest.ForGroup(ThingRequestGroup.AttackTarget);
			PathEndMode arg_415_3 = PathEndMode.Touch;
			bool canBash2 = canBash;
			IAttackTarget attackTarget2 = (IAttackTarget)GenClosest.ClosestThingReachable(arg_415_0, arg_415_1, arg_415_2, arg_415_3, TraverseParms.For(searcherPawn, Danger.Deadly, TraverseMode.ByPawn, canBash2), maxDist, (Thing x) => innerValidator((IAttackTarget)x), null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
			if (attackTarget2 != null && PawnUtility.ShouldCollideWithPawns(searcherPawn))
			{
				IAttackTarget attackTarget3 = AttackTargetFinder.FindBestReachableMeleeTarget(innerValidator, searcherPawn, maxDist, canBash);
				if (attackTarget3 != null)
				{
					float lengthHorizontal = (searcherPawn.Position - attackTarget2.Thing.Position).LengthHorizontal;
					float lengthHorizontal2 = (searcherPawn.Position - attackTarget3.Thing.Position).LengthHorizontal;
					if (Mathf.Abs(lengthHorizontal - lengthHorizontal2) < 50f)
					{
						attackTarget2 = attackTarget3;
					}
				}
			}
			return attackTarget2;
		}

		private static bool CanReach(Thing searcher, Thing target, bool canBash)
		{
			Pawn pawn = searcher as Pawn;
			if (pawn != null)
			{
				if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Some, canBash, TraverseMode.ByPawn))
				{
					return false;
				}
			}
			else
			{
				TraverseMode mode = (!canBash) ? TraverseMode.NoPassClosedDoors : TraverseMode.PassDoors;
				if (!searcher.Map.reachability.CanReach(searcher.Position, target, PathEndMode.Touch, TraverseParms.For(mode, Danger.Deadly, false)))
				{
					return false;
				}
			}
			return true;
		}

		private static IAttackTarget FindBestReachableMeleeTarget(Predicate<IAttackTarget> validator, Pawn searcherPawn, float maxTargDist, bool canBash)
		{
			maxTargDist = Mathf.Min(maxTargDist, 30f);
			IAttackTarget reachableTarget = null;
			Func<IntVec3, IAttackTarget> bestTargetOnCell = delegate(IntVec3 x)
			{
				List<Thing> thingList = x.GetThingList(searcherPawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					IAttackTarget attackTarget = thing as IAttackTarget;
					if (attackTarget != null)
					{
						if (validator(attackTarget))
						{
							if (ReachabilityImmediate.CanReachImmediate(x, thing, searcherPawn.Map, PathEndMode.Touch, searcherPawn))
							{
								if (searcherPawn.CanReachImmediate(thing, PathEndMode.Touch) || searcherPawn.Map.attackTargetReservationManager.CanReserve(searcherPawn, attackTarget))
								{
									return attackTarget;
								}
							}
						}
					}
				}
				return null;
			};
			searcherPawn.Map.floodFiller.FloodFill(searcherPawn.Position, delegate(IntVec3 x)
			{
				if (!x.Walkable(searcherPawn.Map))
				{
					return false;
				}
				if ((float)x.DistanceToSquared(searcherPawn.Position) > maxTargDist * maxTargDist)
				{
					return false;
				}
				if (!canBash)
				{
					Building_Door building_Door = x.GetEdifice(searcherPawn.Map) as Building_Door;
					if (building_Door != null && !building_Door.CanPhysicallyPass(searcherPawn))
					{
						return false;
					}
				}
				return !PawnUtility.AnyPawnBlockingPathAt(x, searcherPawn, true, false);
			}, delegate(IntVec3 x)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = x + GenAdj.AdjacentCells[i];
					if (intVec.InBounds(searcherPawn.Map))
					{
						IAttackTarget attackTarget = bestTargetOnCell(intVec);
						if (attackTarget != null)
						{
							reachableTarget = attackTarget;
							break;
						}
					}
				}
				return reachableTarget != null;
			}, false);
			return reachableTarget;
		}

		private static bool HasRangedAttack(IAttackTargetSearcher t)
		{
			Verb currentEffectiveVerb = t.CurrentEffectiveVerb;
			return currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.MeleeRange;
		}

		private static bool CanShootAtFromCurrentPosition(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			return verb != null && verb.CanHitTargetFrom(searcher.Thing.Position, target.Thing);
		}

		private static IAttackTarget GetRandomShootingTargetByScore(List<IAttackTarget> targets, IAttackTargetSearcher searcher, Verb verb)
		{
			Pair<IAttackTarget, float> pair;
			if (AttackTargetFinder.GetAvailableShootingTargetsByScore(targets, searcher, verb).TryRandomElementByWeight((Pair<IAttackTarget, float> x) => x.Second, out pair))
			{
				return pair.First;
			}
			return null;
		}

		private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(List<IAttackTarget> rawTargets, IAttackTargetSearcher searcher, Verb verb)
		{
			AttackTargetFinder.availableShootingTargets.Clear();
			if (rawTargets.Count == 0)
			{
				return AttackTargetFinder.availableShootingTargets;
			}
			AttackTargetFinder.tmpTargetScores.Clear();
			AttackTargetFinder.tmpCanShootAtTarget.Clear();
			float num = 0f;
			IAttackTarget attackTarget = null;
			for (int i = 0; i < rawTargets.Count; i++)
			{
				AttackTargetFinder.tmpTargetScores.Add(-3.40282347E+38f);
				AttackTargetFinder.tmpCanShootAtTarget.Add(false);
				if (rawTargets[i] != searcher)
				{
					bool flag = AttackTargetFinder.CanShootAtFromCurrentPosition(rawTargets[i], searcher, verb);
					AttackTargetFinder.tmpCanShootAtTarget[i] = flag;
					if (flag)
					{
						float shootingTargetScore = AttackTargetFinder.GetShootingTargetScore(rawTargets[i], searcher, verb);
						AttackTargetFinder.tmpTargetScores[i] = shootingTargetScore;
						if (attackTarget == null || shootingTargetScore > num)
						{
							attackTarget = rawTargets[i];
							num = shootingTargetScore;
						}
					}
				}
			}
			if (num < 1f)
			{
				if (attackTarget != null)
				{
					AttackTargetFinder.availableShootingTargets.Add(new Pair<IAttackTarget, float>(attackTarget, 1f));
				}
			}
			else
			{
				float num2 = num - 30f;
				for (int j = 0; j < rawTargets.Count; j++)
				{
					if (rawTargets[j] != searcher)
					{
						if (AttackTargetFinder.tmpCanShootAtTarget[j])
						{
							float num3 = AttackTargetFinder.tmpTargetScores[j];
							if (num3 >= num2)
							{
								float second = Mathf.InverseLerp(num - 30f, num, num3);
								AttackTargetFinder.availableShootingTargets.Add(new Pair<IAttackTarget, float>(rawTargets[j], second));
							}
						}
					}
				}
			}
			return AttackTargetFinder.availableShootingTargets;
		}

		private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			float num = 60f;
			num -= Mathf.Min((target.Thing.Position - searcher.Thing.Position).LengthHorizontal, 40f);
			if (target.TargetCurrentlyAimingAt == searcher.Thing)
			{
				num += 10f;
			}
			if (searcher.LastAttackedTarget == target.Thing && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
			{
				num += 40f;
			}
			num -= CoverUtility.CalculateOverallBlockChance(target.Thing.Position, searcher.Thing.Position, searcher.Thing.Map) * 10f;
			Pawn pawn = target as Pawn;
			if (pawn != null && pawn.RaceProps.Animal && pawn.Faction != null && !pawn.IsFighting())
			{
				num -= 50f;
			}
			return num + AttackTargetFinder.FriendlyFireShootingTargetScoreOffset(target, searcher, verb);
		}

		private static float FriendlyFireShootingTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			if (verb.verbProps.ai_AvoidFriendlyFireRadius <= 0f)
			{
				return 0f;
			}
			Map map = target.Thing.Map;
			IntVec3 position = target.Thing.Position;
			int num = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					bool flag = true;
					List<Thing> thingList = intVec.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j] is IAttackTarget && thingList[j] != target)
						{
							if (flag)
							{
								if (!GenSight.LineOfSight(position, intVec, map, true, null, 0, 0))
								{
									break;
								}
								flag = false;
							}
							float num3;
							if (thingList[j] == searcher)
							{
								num3 = 40f;
							}
							else if (thingList[j] is Pawn)
							{
								num3 = ((!thingList[j].def.race.Animal) ? 18f : 7f);
							}
							else
							{
								num3 = 10f;
							}
							if (searcher.Thing.HostileTo(thingList[j]))
							{
								num2 += num3 * 0.6f;
							}
							else
							{
								num2 -= num3;
							}
						}
					}
				}
			}
			return Mathf.Min(num2, 0f);
		}

		public static IAttackTarget BestShootTargetFromCurrentPosition(IAttackTargetSearcher searcher, Predicate<Thing> validator, float maxDistance, float minDistance, TargetScanFlags flags)
		{
			return AttackTargetFinder.BestAttackTarget(searcher, flags, validator, minDistance, maxDistance, default(IntVec3), 3.40282347E+38f, false);
		}

		public static bool CanSee(this Thing seer, Thing target, Func<IntVec3, bool> validator = null)
		{
			ShootLeanUtility.CalcShootableCellsOf(AttackTargetFinder.tempDestList, target);
			for (int i = 0; i < AttackTargetFinder.tempDestList.Count; i++)
			{
				if (GenSight.LineOfSight(seer.Position, AttackTargetFinder.tempDestList[i], seer.Map, true, validator, 0, 0))
				{
					return true;
				}
			}
			ShootLeanUtility.LeanShootingSourcesFromTo(seer.Position, target.Position, seer.Map, AttackTargetFinder.tempSourceList);
			for (int j = 0; j < AttackTargetFinder.tempSourceList.Count; j++)
			{
				for (int k = 0; k < AttackTargetFinder.tempDestList.Count; k++)
				{
					if (GenSight.LineOfSight(AttackTargetFinder.tempSourceList[j], AttackTargetFinder.tempDestList[k], seer.Map, true, validator, 0, 0))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void DebugDrawAttackTargetScores_Update()
		{
			IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
			if (attackTargetSearcher == null)
			{
				return;
			}
			if (attackTargetSearcher.Thing.Map != Find.VisibleMap)
			{
				return;
			}
			Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb == null)
			{
				return;
			}
			AttackTargetFinder.tmpTargets.Clear();
			List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
			for (int i = 0; i < list.Count; i++)
			{
				AttackTargetFinder.tmpTargets.Add((IAttackTarget)list[i]);
			}
			List<Pair<IAttackTarget, float>> availableShootingTargetsByScore = AttackTargetFinder.GetAvailableShootingTargetsByScore(AttackTargetFinder.tmpTargets, attackTargetSearcher, currentEffectiveVerb);
			for (int j = 0; j < availableShootingTargetsByScore.Count; j++)
			{
				GenDraw.DrawLineBetween(attackTargetSearcher.Thing.DrawPos, availableShootingTargetsByScore[j].First.Thing.DrawPos);
			}
		}

		public static void DebugDrawAttackTargetScores_OnGUI()
		{
			IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
			if (attackTargetSearcher == null)
			{
				return;
			}
			if (attackTargetSearcher.Thing.Map != Find.VisibleMap)
			{
				return;
			}
			Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb == null)
			{
				return;
			}
			List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing != attackTargetSearcher)
				{
					string text;
					Color red;
					if (!AttackTargetFinder.CanShootAtFromCurrentPosition((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb))
					{
						text = "out of range";
						red = Color.red;
					}
					else
					{
						text = AttackTargetFinder.GetShootingTargetScore((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb).ToString("F0");
						red = new Color(0.25f, 1f, 0.25f);
					}
					Vector2 screenPos = thing.DrawPos.MapToUIPosition();
					GenMapUI.DrawThingLabel(screenPos, text, red);
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}
	}
}
