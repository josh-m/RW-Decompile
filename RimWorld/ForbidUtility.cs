using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class ForbidUtility
	{
		public static void SetForbidden(this Thing t, bool value, bool warnOnFail = true)
		{
			if (t == null)
			{
				if (warnOnFail)
				{
					Log.Error("Tried to SetForbidden on null Thing.");
				}
				return;
			}
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps == null)
			{
				if (warnOnFail)
				{
					Log.Error("Tried to SetForbidden on non-ThingWithComps Thing " + t);
				}
				return;
			}
			CompForbiddable comp = thingWithComps.GetComp<CompForbiddable>();
			if (comp == null)
			{
				if (warnOnFail)
				{
					Log.Error("Tried to SetForbidden on non-Forbiddable Thing " + t);
				}
				return;
			}
			comp.Forbidden = value;
		}

		public static void SetForbiddenIfOutsideHomeArea(this Thing t)
		{
			if (!t.Spawned)
			{
				Log.Error("SetForbiddenIfOutsideHomeArea unspawned thing " + t);
			}
			if (t.Position.InBounds(t.Map) && !t.Map.areaManager.Home[t.Position])
			{
				t.SetForbidden(true, false);
			}
		}

		private static bool CaresAboutForbidden(Pawn pawn, bool cellTarget)
		{
			return !pawn.InMentalState && pawn.HostFaction == null && (!cellTarget || !ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(pawn));
		}

		public static bool InAllowedArea(this IntVec3 c, Pawn forPawn)
		{
			if (forPawn.playerSettings != null)
			{
				Area areaRestrictionInPawnCurrentMap = forPawn.playerSettings.AreaRestrictionInPawnCurrentMap;
				if (areaRestrictionInPawnCurrentMap != null && areaRestrictionInPawnCurrentMap.TrueCount > 0 && !areaRestrictionInPawnCurrentMap[c])
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsForbidden(this Thing t, Pawn pawn)
		{
			if (!ForbidUtility.CaresAboutForbidden(pawn, false))
			{
				return false;
			}
			if (t.Spawned && t.Position.IsForbidden(pawn))
			{
				return true;
			}
			if (t.IsForbidden(pawn.Faction))
			{
				return true;
			}
			Lord lord = pawn.GetLord();
			return lord != null && lord.extraForbiddenThings.Contains(t);
		}

		public static bool IsForbiddenToPass(this Thing t, Pawn pawn)
		{
			return ForbidUtility.CaresAboutForbidden(pawn, false) && ((t.Spawned && t.Position.IsForbidden(pawn) && !(t is Building_Door)) || t.IsForbidden(pawn.Faction));
		}

		public static bool IsForbidden(this IntVec3 c, Pawn pawn)
		{
			return ForbidUtility.CaresAboutForbidden(pawn, true) && !c.InAllowedArea(pawn);
		}

		public static bool IsForbiddenEntirely(this Region r, Pawn pawn)
		{
			if (!ForbidUtility.CaresAboutForbidden(pawn, true))
			{
				return false;
			}
			if (pawn.playerSettings != null)
			{
				Area areaRestriction = pawn.playerSettings.AreaRestriction;
				if (areaRestriction != null && areaRestriction.TrueCount > 0 && areaRestriction.Map == r.Map && r.OverlapWith(areaRestriction) == AreaOverlap.None)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsForbidden(this Thing t, Faction faction)
		{
			if (faction == null)
			{
				return false;
			}
			if (faction != Faction.OfPlayer)
			{
				return false;
			}
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps == null)
			{
				return false;
			}
			CompForbiddable comp = thingWithComps.GetComp<CompForbiddable>();
			return comp != null && comp.Forbidden;
		}
	}
}
