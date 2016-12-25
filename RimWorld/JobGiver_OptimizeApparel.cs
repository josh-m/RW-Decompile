using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_OptimizeApparel : ThinkNode_JobGiver
	{
		private const int ApparelOptimizeCheckIntervalMin = 6000;

		private const int ApparelOptimizeCheckIntervalMax = 9000;

		private const float MinScoreGainToCare = 0.05f;

		private const float ScoreFactorIfNotReplacing = 10f;

		private static NeededWarmth neededWarmth;

		private static StringBuilder debugSb;

		private static readonly SimpleCurve InsulationColdScoreFactorCurve_NeedWarm = new SimpleCurve
		{
			new CurvePoint(-30f, 8f),
			new CurvePoint(0f, 1f)
		};

		private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(0.2f, 0.15f),
			new CurvePoint(0.25f, 0.3f),
			new CurvePoint(0.5f, 0.4f),
			new CurvePoint(0.55f, 0.85f),
			new CurvePoint(1f, 1f)
		};

		private void SetNextOptimizeTick(Pawn pawn)
		{
			pawn.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame + UnityEngine.Random.Range(6000, 9000);
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.outfits == null)
			{
				Log.ErrorOnce(pawn + " tried to run JobGiver_OptimizeApparel without an OutfitTracker", 5643897);
				return null;
			}
			if (pawn.Faction != Faction.OfPlayer)
			{
				Log.ErrorOnce("Non-colonist " + pawn + " tried to optimize apparel.", 764323);
				return null;
			}
			if (!DebugViewSettings.debugApparelOptimize)
			{
				if (Find.TickManager.TicksGame < pawn.mindState.nextApparelOptimizeTick)
				{
					return null;
				}
			}
			else
			{
				JobGiver_OptimizeApparel.debugSb = new StringBuilder();
				JobGiver_OptimizeApparel.debugSb.AppendLine(string.Concat(new object[]
				{
					"Scanning for ",
					pawn,
					" at ",
					pawn.Position
				}));
			}
			Outfit currentOutfit = pawn.outfits.CurrentOutfit;
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int i = wornApparel.Count - 1; i >= 0; i--)
			{
				if (!currentOutfit.filter.Allows(wornApparel[i]) && pawn.outfits.forcedHandler.AllowedToAutomaticallyDrop(wornApparel[i]))
				{
					return new Job(JobDefOf.RemoveApparel, wornApparel[i])
					{
						haulDroppedApparel = true
					};
				}
			}
			Thing thing = null;
			float num = 0f;
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel);
			if (list.Count == 0)
			{
				this.SetNextOptimizeTick(pawn);
				return null;
			}
			JobGiver_OptimizeApparel.neededWarmth = PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map, GenLocalDate.Month(pawn));
			for (int j = 0; j < list.Count; j++)
			{
				Apparel apparel = (Apparel)list[j];
				if (currentOutfit.filter.Allows(apparel))
				{
					if (apparel.Map.slotGroupManager.SlotGroupAt(apparel.Position) != null)
					{
						if (!apparel.IsForbidden(pawn))
						{
							float num2 = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel);
							if (DebugViewSettings.debugApparelOptimize)
							{
								JobGiver_OptimizeApparel.debugSb.AppendLine(apparel.LabelCap + ": " + num2.ToString("F2"));
							}
							if (num2 >= 0.05f && num2 >= num)
							{
								if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
								{
									if (pawn.CanReserveAndReach(apparel, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1))
									{
										thing = apparel;
										num = num2;
									}
								}
							}
						}
					}
				}
			}
			if (DebugViewSettings.debugApparelOptimize)
			{
				JobGiver_OptimizeApparel.debugSb.AppendLine("BEST: " + thing);
				Log.Message(JobGiver_OptimizeApparel.debugSb.ToString());
				JobGiver_OptimizeApparel.debugSb = null;
			}
			if (thing == null)
			{
				this.SetNextOptimizeTick(pawn);
				return null;
			}
			return new Job(JobDefOf.Wear, thing);
		}

		public static float ApparelScoreGain(Pawn pawn, Apparel ap)
		{
			if (ap.def == ThingDefOf.Apparel_PersonalShield && pawn.equipment.Primary != null && !pawn.equipment.Primary.def.Verbs[0].MeleeRange)
			{
				return -1000f;
			}
			float num = JobGiver_OptimizeApparel.ApparelScoreRaw(ap);
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			bool flag = false;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (!ApparelUtility.CanWearTogether(wornApparel[i].def, ap.def))
				{
					if (!pawn.outfits.forcedHandler.AllowedToAutomaticallyDrop(wornApparel[i]))
					{
						return -1000f;
					}
					num -= JobGiver_OptimizeApparel.ApparelScoreRaw(wornApparel[i]);
					flag = true;
				}
			}
			if (!flag)
			{
				num *= 10f;
			}
			return num;
		}

		public static float ApparelScoreRaw(Apparel ap)
		{
			float num = 0.1f;
			float num2 = ap.GetStatValue(StatDefOf.ArmorRating_Sharp, true) + ap.GetStatValue(StatDefOf.ArmorRating_Blunt, true);
			num += num2;
			if (ap.def.useHitPoints)
			{
				float x = (float)ap.HitPoints / (float)ap.MaxHitPoints;
				num *= JobGiver_OptimizeApparel.HitPointsPercentScoreFactorCurve.Evaluate(x);
			}
			num += ap.GetSpecialApparelScoreOffset();
			float num3 = 1f;
			if (JobGiver_OptimizeApparel.neededWarmth == NeededWarmth.Warm)
			{
				float statValue = ap.GetStatValue(StatDefOf.Insulation_Cold, true);
				num3 *= JobGiver_OptimizeApparel.InsulationColdScoreFactorCurve_NeedWarm.Evaluate(statValue);
			}
			num *= num3;
			if (ap.WornByCorpse)
			{
				num -= 0.5f;
				if (num > 0f)
				{
					num *= 0.1f;
				}
			}
			return num;
		}
	}
}
