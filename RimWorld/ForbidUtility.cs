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
			if (t.Position.InBounds() && !Find.AreaHome[t.Position])
			{
				t.SetForbidden(true, false);
			}
		}

		private static bool CaresAboutForbidden(Pawn pawn, bool cellTarget)
		{
			return !pawn.InMentalState && pawn.HostFaction == null && (!cellTarget || pawn.playerSettings == null || pawn.playerSettings.master == null || !pawn.playerSettings.master.Drafted);
		}

		private static Area GetApplicableAllowedArea(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.AreaRestriction != null && pawn.playerSettings.AreaRestriction.TrueCount > 0)
			{
				return pawn.playerSettings.AreaRestriction;
			}
			return null;
		}

		public static bool InAllowedArea(this IntVec3 c, Pawn forPawn)
		{
			Area applicableAllowedArea = ForbidUtility.GetApplicableAllowedArea(forPawn);
			return applicableAllowedArea == null || applicableAllowedArea[c];
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
			Area applicableAllowedArea = ForbidUtility.GetApplicableAllowedArea(pawn);
			return applicableAllowedArea != null && r.OverlapWith(applicableAllowedArea) == AreaOverlap.None;
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
