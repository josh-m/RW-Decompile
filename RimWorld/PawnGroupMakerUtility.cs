using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnGroupMakerUtility
	{
		private const string MeleeWeaponTag = "Melee";

		private const float CostWeightDenominator = 100f;

		private static readonly SimpleCurve MaxPawnCostPerRaidPointsCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 25f),
				true
			},
			{
				new CurvePoint(100f, 40f),
				true
			},
			{
				new CurvePoint(300f, 50f),
				true
			},
			{
				new CurvePoint(700f, 100f),
				true
			},
			{
				new CurvePoint(1000f, 150f),
				true
			},
			{
				new CurvePoint(1500f, 200f),
				true
			},
			{
				new CurvePoint(2000f, 400f),
				true
			},
			{
				new CurvePoint(3000f, 800f),
				true
			},
			{
				new CurvePoint(100000f, 50000f),
				true
			}
		};

		private static readonly SimpleCurve DesireToSuppressCountPerRaidPointsCurve = new SimpleCurve
		{
			{
				new CurvePoint(600f, 0f),
				true
			},
			{
				new CurvePoint(1000f, 0.5f),
				true
			},
			{
				new CurvePoint(2000f, 1f),
				true
			},
			{
				new CurvePoint(4000f, 2f),
				true
			}
		};

		[DebuggerHidden]
		public static IEnumerable<Pawn> GeneratePawns(PawnGroupKindDef groupKind, PawnGroupMakerParms parms, bool warnOnZeroResults = true)
		{
			if (groupKind == null)
			{
				Log.Error("Tried to generate pawns with null pawn group kind def. parms=" + parms);
			}
			else if (parms.faction.def.pawnGroupMakers == null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Faction ",
					parms.faction,
					" of def ",
					parms.faction.def,
					" has no any PawnGroupMakers."
				}));
			}
			else
			{
				IEnumerable<PawnGroupMaker> usableGroupMakers = from gm in parms.faction.def.pawnGroupMakers
				where gm.kindDef == this.groupKind && gm.CanGenerateFrom(this.parms)
				select gm;
				PawnGroupMaker chosenGroupMaker;
				if (!usableGroupMakers.TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out chosenGroupMaker))
				{
					Log.Error(string.Concat(new object[]
					{
						"Faction ",
						parms.faction,
						" of def ",
						parms.faction.def,
						" has no usable PawnGroupMakers for parms ",
						parms
					}));
				}
				else
				{
					foreach (Pawn p in chosenGroupMaker.GeneratePawns(parms, warnOnZeroResults))
					{
						yield return p;
					}
				}
			}
		}

		public static IEnumerable<PawnGenOption> ChoosePawnGenOptionsByPoints(float points, List<PawnGenOption> options, PawnGroupMakerParms parms)
		{
			float num = PawnGroupMakerUtility.MaxAllowedPawnGenOptionCost(parms.faction, points, parms.raidStrategy);
			List<PawnGenOption> list = new List<PawnGenOption>();
			List<PawnGenOption> list2 = new List<PawnGenOption>();
			float num2 = points;
			bool flag = false;
			while (true)
			{
				list.Clear();
				for (int i = 0; i < options.Count; i++)
				{
					PawnGenOption pawnGenOption = options[i];
					if (pawnGenOption.Cost <= num2)
					{
						if (pawnGenOption.Cost <= num)
						{
							if (!parms.generateFightersOnly || pawnGenOption.kind.isFighter)
							{
								if (parms.raidStrategy == null || parms.raidStrategy.Worker.CanUsePawnGenOption(pawnGenOption, list2))
								{
									if (!flag || !pawnGenOption.kind.factionLeader)
									{
										list.Add(pawnGenOption);
									}
								}
							}
						}
					}
				}
				if (list.Count == 0)
				{
					break;
				}
				float desireToSuppressCount = PawnGroupMakerUtility.DesireToSuppressCountPerRaidPointsCurve.Evaluate(points);
				Func<PawnGenOption, float> weightSelector = delegate(PawnGenOption gr)
				{
					float num3 = (float)gr.selectionWeight;
					if (desireToSuppressCount > 0f)
					{
						float b = num3 * (gr.Cost / 100f);
						num3 = Mathf.Lerp(num3, b, desireToSuppressCount);
					}
					return num3;
				};
				PawnGenOption pawnGenOption2 = list.RandomElementByWeight(weightSelector);
				list2.Add(pawnGenOption2);
				num2 -= pawnGenOption2.Cost;
				if (pawnGenOption2.kind.factionLeader)
				{
					flag = true;
				}
			}
			if (list2.Count == 1 && num2 > points / 2f)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Used only ",
					points - num2,
					" / ",
					points,
					" points generating for ",
					parms.faction
				}));
			}
			return list2;
		}

		private static float MaxAllowedPawnGenOptionCost(Faction faction, float totalPoints, RaidStrategyDef raidStrategy)
		{
			float num = PawnGroupMakerUtility.MaxPawnCostPerRaidPointsCurve.Evaluate(totalPoints);
			num *= faction.def.maxPawnOptionCostFactor;
			if (raidStrategy != null)
			{
				num = Mathf.Min(num, totalPoints / raidStrategy.minPawns);
			}
			num = Mathf.Max(num, faction.def.MinPointsToGenerateNormalPawnGroup() * 1.2f);
			if (raidStrategy != null)
			{
				num = Mathf.Max(num, raidStrategy.Worker.MinMaxAllowedPawnGenOptionCost(faction) * 1.2f);
			}
			return num;
		}

		public static void LogPawnGroupsMade()
		{
			foreach (Faction fac in Find.FactionManager.AllFactions)
			{
				if (!fac.def.pawnGroupMakers.NullOrEmpty<PawnGroupMaker>())
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine(string.Concat(new object[]
					{
						"FACTION: ",
						fac.Name,
						" (",
						fac.def.defName,
						") min=",
						fac.def.MinPointsToGenerateNormalPawnGroup()
					}));
					Action<float> action = delegate(float points)
					{
						if (points < fac.def.MinPointsToGenerateNormalPawnGroup())
						{
							return;
						}
						PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
						pawnGroupMakerParms.tile = Find.VisibleMap.Tile;
						pawnGroupMakerParms.points = points;
						pawnGroupMakerParms.faction = fac;
						sb.AppendLine(string.Concat(new object[]
						{
							"Group with ",
							pawnGroupMakerParms.points,
							" points (max option cost: ",
							PawnGroupMakerUtility.MaxAllowedPawnGenOptionCost(fac, points, RaidStrategyDefOf.ImmediateAttack),
							")"
						}));
						float num = 0f;
						foreach (Pawn current in from pa in PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, pawnGroupMakerParms, false)
						orderby pa.kindDef.combatPower
						select pa)
						{
							string text;
							if (current.equipment.Primary != null)
							{
								text = current.equipment.Primary.Label;
							}
							else
							{
								text = "no-equipment";
							}
							Apparel apparel = current.apparel.FirstApparelOnBodyPartGroup(BodyPartGroupDefOf.Torso);
							string text2;
							if (apparel != null)
							{
								text2 = apparel.LabelCap;
							}
							else
							{
								text2 = "shirtless";
							}
							sb.AppendLine(string.Concat(new string[]
							{
								"  ",
								current.kindDef.combatPower.ToString("F0").PadRight(6),
								current.kindDef.defName,
								", ",
								text,
								", ",
								text2
							}));
							num += current.kindDef.combatPower;
						}
						sb.AppendLine("         totalCost " + num);
						sb.AppendLine();
					};
					foreach (float obj in Dialog_DebugActionsMenu.PointsOptions())
					{
						action(obj);
					}
					Log.Message(sb.ToString());
				}
			}
		}
	}
}
