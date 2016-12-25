using System;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class MarriageSpotUtility
	{
		public static bool IsValidMarriageSpot(IntVec3 cell, StringBuilder outFailReason = null)
		{
			if (!cell.Standable())
			{
				if (outFailReason != null)
				{
					outFailReason.Append("MarriageSpotNotStandable".Translate());
				}
				return false;
			}
			return cell.Roofed() || JoyUtility.EnjoyableOutsideNow(outFailReason);
		}

		public static bool IsValidMarriageSpotFor(IntVec3 cell, Pawn firstFiance, Pawn secondFiance, StringBuilder outFailReason = null)
		{
			if (!MarriageSpotUtility.IsValidMarriageSpot(cell, outFailReason))
			{
				return false;
			}
			if (!cell.Roofed())
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
				Room room = cell.GetRoom();
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
