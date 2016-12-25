using System;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class MarriageSpotUtility
	{
		public static bool IsValidMarriageSpot(IntVec3 cell, Map map, StringBuilder outFailReason = null)
		{
			if (!cell.Standable(map))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotNotStandable".Translate());
				}
				return false;
			}
			return cell.Roofed(map) || JoyUtility.EnjoyableOutsideNow(map, outFailReason);
		}

		public static bool IsValidMarriageSpotFor(IntVec3 cell, Pawn firstFiance, Pawn secondFiance, StringBuilder outFailReason = null)
		{
			if (!firstFiance.Spawned || !secondFiance.Spawned)
			{
				Log.Warning("Can't check if a marriage spot is valid because one of the fiances isn't spawned.");
				return false;
			}
			if (firstFiance.Map != secondFiance.Map)
			{
				return false;
			}
			if (!MarriageSpotUtility.IsValidMarriageSpot(cell, firstFiance.Map, outFailReason))
			{
				return false;
			}
			if (!cell.Roofed(firstFiance.Map))
			{
				if (!JoyUtility.EnjoyableOutsideNow(firstFiance, outFailReason))
				{
					return false;
				}
				if (!JoyUtility.EnjoyableOutsideNow(secondFiance, outFailReason))
				{
					return false;
				}
			}
			if (cell.GetDangerFor(firstFiance) != Danger.None)
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotDangerous".Translate(new object[]
					{
						firstFiance.LabelShort
					}));
				}
				return false;
			}
			if (cell.GetDangerFor(secondFiance) != Danger.None)
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotDangerous".Translate(new object[]
					{
						secondFiance.LabelShort
					}));
				}
				return false;
			}
			if (cell.IsForbidden(firstFiance))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotForbidden".Translate(new object[]
					{
						firstFiance.LabelShort
					}));
				}
				return false;
			}
			if (cell.IsForbidden(secondFiance))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotForbidden".Translate(new object[]
					{
						secondFiance.LabelShort
					}));
				}
				return false;
			}
			if (!firstFiance.CanReserve(cell, 1) || !secondFiance.CanReserve(cell, 1))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotReserved".Translate());
				}
				return false;
			}
			if (!firstFiance.CanReach(cell, PathEndMode.OnCell, Danger.None, false, TraverseMode.ByPawn))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotUnreachable".Translate(new object[]
					{
						firstFiance.LabelShort
					}));
				}
				return false;
			}
			if (!secondFiance.CanReach(cell, PathEndMode.OnCell, Danger.None, false, TraverseMode.ByPawn))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotUnreachable".Translate(new object[]
					{
						secondFiance.LabelShort
					}));
				}
				return false;
			}
			if (!firstFiance.IsPrisoner && !secondFiance.IsPrisoner)
			{
				Room room = cell.GetRoom(firstFiance.Map);
				if (room != null && room.isPrisonCell)
				{
					if (outFailReason != null)
					{
						outFailReason.Append("MarriageSpotInPrisonCell".Translate());
					}
					return false;
				}
			}
			return true;
		}
	}
}
