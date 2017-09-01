using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public static class DangerUtility
	{
		private static Pawn directOrderingPawn;

		private static int directOrderFrame = -1;

		public static void NotifyDirectOrderingThisFrame(Pawn p)
		{
			DangerUtility.directOrderingPawn = p;
			DangerUtility.directOrderFrame = Time.frameCount;
		}

		public static void DoneDirectOrdering()
		{
			DangerUtility.directOrderingPawn = null;
		}

		public static Danger NormalMaxDanger(this Pawn p)
		{
			if (p.CurJob != null && p.CurJob.playerForced)
			{
				return Danger.Deadly;
			}
			if (Time.frameCount != DangerUtility.directOrderFrame)
			{
				DangerUtility.directOrderingPawn = null;
			}
			if (DangerUtility.directOrderingPawn == p)
			{
				return Danger.Deadly;
			}
			if (p.Faction != Faction.OfPlayer)
			{
				return Danger.Some;
			}
			if (p.health.hediffSet.HasTemperatureInjury(TemperatureInjuryStage.Minor) && GenTemperature.FactionOwnsPassableRoomInTemperatureRange(p.Faction, p.SafeTemperatureRange(), p.MapHeld))
			{
				return Danger.None;
			}
			return Danger.Some;
		}

		public static Danger GetDangerFor(this IntVec3 c, Pawn p, Map map)
		{
			Map mapHeld = p.MapHeld;
			if (mapHeld == null || mapHeld != map)
			{
				return Danger.None;
			}
			Region region = c.GetRegion(mapHeld, RegionType.Set_All);
			if (region == null)
			{
				return Danger.None;
			}
			return region.DangerFor(p);
		}
	}
}
