using System;
using System.Collections.Generic;

namespace Verse
{
	public static class CoverUtility
	{
		public const float CoverPercent_Corner = 0.75f;

		public static List<CoverInfo> CalculateCoverGiverSet(IntVec3 targetLoc, IntVec3 shooterLoc)
		{
			List<CoverInfo> list = new List<CoverInfo>();
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = targetLoc + GenAdj.AdjacentCells[i];
				if (intVec.InBounds())
				{
					CoverInfo item;
					if (CoverUtility.TryFindAdjustedCoverInCell(shooterLoc, targetLoc, intVec, out item))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		public static float CalculateOverallBlockChance(IntVec3 targetLoc, IntVec3 shooterLoc)
		{
			float num = 0f;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = targetLoc + GenAdj.AdjacentCells[i];
				if (intVec.InBounds())
				{
					CoverInfo coverInfo;
					if (CoverUtility.TryFindAdjustedCoverInCell(shooterLoc, targetLoc, intVec, out coverInfo))
					{
						num += (1f - num) * coverInfo.BlockChance;
					}
				}
			}
			return num;
		}

		private static bool TryFindAdjustedCoverInCell(IntVec3 shooterLoc, IntVec3 targetLoc, IntVec3 adjCell, out CoverInfo result)
		{
			Thing cover = adjCell.GetCover();
			if (cover == null || shooterLoc == targetLoc)
			{
				result = CoverInfo.Invalid;
				return false;
			}
			float angleFlat = (shooterLoc - targetLoc).AngleFlat;
			float angleFlat2 = (adjCell - targetLoc).AngleFlat;
			float num = GenGeo.AngleDifferenceBetween(angleFlat2, angleFlat);
			if (!targetLoc.AdjacentToCardinal(adjCell))
			{
				num *= 1.75f;
			}
			float num2 = cover.def.BaseBlockChance();
			if (num < 15f)
			{
				num2 *= 1f;
			}
			else if (num < 27f)
			{
				num2 *= 0.8f;
			}
			else if (num < 40f)
			{
				num2 *= 0.6f;
			}
			else if (num < 52f)
			{
				num2 *= 0.4f;
			}
			else
			{
				if (num >= 65f)
				{
					result = CoverInfo.Invalid;
					return false;
				}
				num2 *= 0.2f;
			}
			float lengthHorizontal = (shooterLoc - adjCell).LengthHorizontal;
			if (lengthHorizontal < 1.9f)
			{
				num2 *= 0.3333f;
			}
			else if (lengthHorizontal < 2.9f)
			{
				num2 *= 0.66666f;
			}
			result = new CoverInfo(cover, num2);
			return true;
		}

		public static float BaseBlockChance(this ThingDef def)
		{
			if (def.Fillage == FillCategory.Full)
			{
				return 0.75f;
			}
			return def.fillPercent;
		}

		public static float TotalSurroundingCoverScore(IntVec3 c)
		{
			float num = 0f;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (c2.InBounds())
				{
					Thing cover = c2.GetCover();
					if (cover != null)
					{
						num += cover.def.BaseBlockChance();
					}
				}
			}
			return num;
		}
	}
}
