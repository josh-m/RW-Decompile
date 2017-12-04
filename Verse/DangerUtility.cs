using RimWorld;
using System;

namespace Verse
{
	public static class DangerUtility
	{
		public static Danger NormalMaxDanger(this Pawn p)
		{
			if (p.CurJob != null && p.CurJob.playerForced)
			{
				return Danger.Deadly;
			}
			if (FloatMenuMakerMap.makingFor == p)
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
