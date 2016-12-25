using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public static class CastPositionFinder
	{
		private const float BaseAIPreference = 0.3f;

		private const float MinimumPreferredRange = 5f;

		private const float OptimalRangeFactor = 0.8f;

		private const float OptimalRangeFactorImportance = 0.3f;

		private static CastPositionRequest req;

		private static IntVec3 casterLoc;

		private static IntVec3 targetLoc;

		private static Verb verb;

		private static float rangeFromTarget;

		private static float rangeFromTargetSquared;

		private static float optimalRangeSquared;

		private static float rangeFromCasterToCellSquared;

		private static float rangeFromTargetToCellSquared;

		private static int inRadiusMark;

		private static ByteGrid avoidGrid;

		private static float maxRangeFromCasterSquared;

		private static float maxRangeFromTargetSquared;

		private static float maxRangeFromLocusSquared;

		private static IntVec3 bestSpot = IntVec3.Invalid;

		private static float bestSpotPref = 0.001f;

		public static bool TryFindCastPosition(CastPositionRequest newReq, out IntVec3 dest)
		{
			CastPositionFinder.req = newReq;
			CastPositionFinder.casterLoc = CastPositionFinder.req.caster.Position;
			CastPositionFinder.targetLoc = CastPositionFinder.req.target.Position;
			CastPositionFinder.verb = CastPositionFinder.req.verb;
			CastPositionFinder.avoidGrid = newReq.caster.GetAvoidGrid();
			if (CastPositionFinder.verb == null)
			{
				Log.Error(CastPositionFinder.req.caster + " tried to find casting position without a verb.");
				dest = IntVec3.Invalid;
				return false;
			}
			if (CastPositionFinder.req.maxRegionsRadius > 0)
			{
				Region region = CastPositionFinder.casterLoc.GetRegion(CastPositionFinder.req.caster.Map);
				if (region == null)
				{
					Log.Error("TryFindCastPosition requiring region traversal but root region is null.");
					dest = IntVec3.Invalid;
					return false;
				}
				CastPositionFinder.inRadiusMark = Rand.Int;
				RegionTraverser.MarkRegionsBFS(region, null, newReq.maxRegionsRadius, CastPositionFinder.inRadiusMark);
				if (CastPositionFinder.req.maxRangeFromLocus > 0.01f)
				{
					Region region2 = CastPositionFinder.req.locus.GetRegion(CastPositionFinder.req.caster.Map);
					if (region2 == null)
					{
						Log.Error("locus " + CastPositionFinder.req.locus + " has no region");
						dest = IntVec3.Invalid;
						return false;
					}
					if (region2.mark != CastPositionFinder.inRadiusMark)
					{
						Log.Error(string.Concat(new object[]
						{
							CastPositionFinder.req.caster,
							" can't possibly get to locus ",
							CastPositionFinder.req.locus,
							" as it's not in a maxRegionsRadius of ",
							CastPositionFinder.req.maxRegionsRadius,
							". Overriding maxRegionsRadius."
						}));
						CastPositionFinder.req.maxRegionsRadius = 0;
					}
				}
			}
			CellRect cellRect = CellRect.WholeMap(CastPositionFinder.req.caster.Map);
			if (CastPositionFinder.req.maxRangeFromCaster > 0.01f)
			{
				int num = Mathf.CeilToInt(CastPositionFinder.req.maxRangeFromCaster);
				CellRect otherRect = new CellRect(CastPositionFinder.casterLoc.x - num, CastPositionFinder.casterLoc.z - num, num * 2 + 1, num * 2 + 1);
				cellRect.ClipInsideRect(otherRect);
			}
			int num2 = Mathf.CeilToInt(CastPositionFinder.req.maxRangeFromTarget);
			CellRect otherRect2 = new CellRect(CastPositionFinder.targetLoc.x - num2, CastPositionFinder.targetLoc.z - num2, num2 * 2 + 1, num2 * 2 + 1);
			cellRect.ClipInsideRect(otherRect2);
			if (CastPositionFinder.req.maxRangeFromLocus > 0.01f)
			{
				int num3 = Mathf.CeilToInt(CastPositionFinder.req.maxRangeFromLocus);
				CellRect otherRect3 = new CellRect(CastPositionFinder.targetLoc.x - num3, CastPositionFinder.targetLoc.z - num3, num3 * 2 + 1, num3 * 2 + 1);
				cellRect.ClipInsideRect(otherRect3);
			}
			CastPositionFinder.bestSpot = IntVec3.Invalid;
			CastPositionFinder.bestSpotPref = 0.001f;
			CastPositionFinder.maxRangeFromCasterSquared = CastPositionFinder.req.maxRangeFromCaster * CastPositionFinder.req.maxRangeFromCaster;
			CastPositionFinder.maxRangeFromTargetSquared = CastPositionFinder.req.maxRangeFromTarget * CastPositionFinder.req.maxRangeFromTarget;
			CastPositionFinder.maxRangeFromLocusSquared = CastPositionFinder.req.maxRangeFromLocus * CastPositionFinder.req.maxRangeFromLocus;
			CastPositionFinder.rangeFromTarget = (CastPositionFinder.req.caster.Position - CastPositionFinder.req.target.Position).LengthHorizontal;
			CastPositionFinder.rangeFromTargetSquared = (CastPositionFinder.req.caster.Position - CastPositionFinder.req.target.Position).LengthHorizontalSquared;
			CastPositionFinder.optimalRangeSquared = CastPositionFinder.verb.verbProps.range * 0.8f * (CastPositionFinder.verb.verbProps.range * 0.8f);
			CastPositionFinder.EvaluateCell(CastPositionFinder.req.caster.Position);
			if ((double)CastPositionFinder.bestSpotPref >= 1.0)
			{
				dest = CastPositionFinder.req.caster.Position;
				return true;
			}
			float slope = -1f / CellLine.Between(CastPositionFinder.req.target.Position, CastPositionFinder.req.caster.Position).Slope;
			CellLine cellLine = new CellLine(CastPositionFinder.req.target.Position, slope);
			bool flag = cellLine.CellIsAbove(CastPositionFinder.req.caster.Position);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (cellLine.CellIsAbove(current) == flag && cellRect.Contains(current))
				{
					CastPositionFinder.EvaluateCell(current);
				}
				iterator.MoveNext();
			}
			if (CastPositionFinder.bestSpot.IsValid && CastPositionFinder.bestSpotPref > 0.33f)
			{
				dest = CastPositionFinder.bestSpot;
				return true;
			}
			CellRect.CellRectIterator iterator2 = cellRect.GetIterator();
			while (!iterator2.Done())
			{
				IntVec3 current2 = iterator2.Current;
				if (cellLine.CellIsAbove(current2) != flag && cellRect.Contains(current2))
				{
					CastPositionFinder.EvaluateCell(current2);
				}
				iterator2.MoveNext();
			}
			if (CastPositionFinder.bestSpot.IsValid)
			{
				dest = CastPositionFinder.bestSpot;
				return true;
			}
			dest = CastPositionFinder.casterLoc;
			return false;
		}

		private static void EvaluateCell(IntVec3 c)
		{
			if (CastPositionFinder.maxRangeFromTargetSquared > 0.01f && CastPositionFinder.maxRangeFromTargetSquared < 250000f && (c - CastPositionFinder.req.target.Position).LengthHorizontalSquared > CastPositionFinder.maxRangeFromTargetSquared)
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, 0f, "range target");
				}
				return;
			}
			if ((double)CastPositionFinder.maxRangeFromLocusSquared > 0.01 && (c - CastPositionFinder.req.locus).LengthHorizontalSquared > CastPositionFinder.maxRangeFromLocusSquared)
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, 0.1f, "range home");
				}
				return;
			}
			if (CastPositionFinder.maxRangeFromCasterSquared > 0.01f)
			{
				CastPositionFinder.rangeFromCasterToCellSquared = (c - CastPositionFinder.req.caster.Position).LengthHorizontalSquared;
				if (CastPositionFinder.rangeFromCasterToCellSquared > CastPositionFinder.maxRangeFromCasterSquared)
				{
					if (DebugViewSettings.drawCastPositionSearch)
					{
						CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, 0.2f, "range caster");
					}
					return;
				}
			}
			if (!c.Walkable(CastPositionFinder.req.caster.Map))
			{
				return;
			}
			if (CastPositionFinder.req.maxRegionsRadius > 0 && c.GetRegion(CastPositionFinder.req.caster.Map).mark != CastPositionFinder.inRadiusMark)
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, 0.64f, "reg radius");
				}
				return;
			}
			if (!CastPositionFinder.req.caster.Map.reachability.CanReach(CastPositionFinder.req.caster.Position, c, PathEndMode.OnCell, TraverseParms.For(CastPositionFinder.req.caster, Danger.Some, TraverseMode.ByPawn, false)))
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, 0.4f, "can't reach");
				}
				return;
			}
			float num = CastPositionFinder.CastPositionPreference(c);
			if (CastPositionFinder.avoidGrid != null)
			{
				byte b = CastPositionFinder.avoidGrid[c];
				num *= Mathf.Max(0.1f, (37f - (float)b) / 37f);
			}
			if (DebugViewSettings.drawCastPositionSearch)
			{
				CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, num / 4f, num.ToString("F3"));
			}
			if (num < CastPositionFinder.bestSpotPref)
			{
				return;
			}
			if (!CastPositionFinder.verb.CanHitTargetFrom(c, CastPositionFinder.req.target))
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, 0.6f, "can't hit");
				}
				return;
			}
			if (CastPositionFinder.req.caster.Map.pawnDestinationManager.DestinationIsReserved(c, CastPositionFinder.req.caster))
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					CastPositionFinder.req.caster.Map.debugDrawer.FlashCell(c, num * 0.9f, "resvd");
				}
				return;
			}
			CastPositionFinder.bestSpot = c;
			CastPositionFinder.bestSpotPref = num;
		}

		private static float CastPositionPreference(IntVec3 c)
		{
			bool flag = true;
			List<Thing> list = CastPositionFinder.req.caster.Map.thingGrid.ThingsListAtFast(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				Fire fire = thing as Fire;
				if (fire != null && fire.parent == null)
				{
					return -1f;
				}
				if (thing.def.passability == Traversability.PassThroughOnly)
				{
					flag = false;
				}
			}
			float num = 0.3f;
			if (CastPositionFinder.req.caster.kindDef.aiAvoidCover)
			{
				num += 8f - CoverUtility.TotalSurroundingCoverScore(c, CastPositionFinder.req.caster.Map);
			}
			if (CastPositionFinder.req.wantCoverFromTarget)
			{
				num += CoverUtility.CalculateOverallBlockChance(c, CastPositionFinder.req.target.Position, CastPositionFinder.req.caster.Map);
			}
			float num2 = (CastPositionFinder.req.caster.Position - c).LengthHorizontal;
			if (CastPositionFinder.rangeFromTarget > 100f)
			{
				num2 -= CastPositionFinder.rangeFromTarget - 100f;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
			}
			num *= Mathf.Pow(0.967f, num2);
			float num3 = 1f;
			CastPositionFinder.rangeFromTargetToCellSquared = (c - CastPositionFinder.req.target.Position).LengthHorizontalSquared;
			float num4 = Mathf.Abs(CastPositionFinder.rangeFromTargetToCellSquared - CastPositionFinder.optimalRangeSquared) / CastPositionFinder.optimalRangeSquared;
			num4 = 1f - num4;
			num4 = 0.7f + 0.3f * num4;
			num3 *= num4;
			if (CastPositionFinder.rangeFromTargetToCellSquared < 25f)
			{
				num3 *= 0.5f;
			}
			num *= num3;
			if (CastPositionFinder.rangeFromCasterToCellSquared > CastPositionFinder.rangeFromTargetSquared)
			{
				num *= 0.4f;
			}
			if (!flag)
			{
				num *= 0.2f;
			}
			return num;
		}
	}
}
