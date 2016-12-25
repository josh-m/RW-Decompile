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

		[DebuggerHidden]
		public static IEnumerable<Pawn> GenerateArrivingPawns(IncidentParms parms, bool warnOnZeroResults = true)
		{
			if (parms.faction.def.pawnGroupMakers == null)
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
				where gm.CanGenerateFrom(this.parms)
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
					foreach (Pawn p in chosenGroupMaker.GenerateArrivingPawns(parms, warnOnZeroResults))
					{
						yield return p;
					}
				}
			}
		}

		public static void AdjustPointsForGroupArrivalParams(IncidentParms parms)
		{
			if (parms.raidStrategy != null)
			{
				parms.points *= parms.raidStrategy.pointsFactor;
			}
			switch (parms.raidArrivalMode)
			{
			case PawnsArriveMode.EdgeWalkIn:
				parms.points *= 1f;
				break;
			case PawnsArriveMode.EdgeDrop:
				parms.points *= 1f;
				break;
			case PawnsArriveMode.CenterDrop:
				parms.points *= 0.45f;
				break;
			}
			if (parms.raidStrategy != null)
			{
				parms.points = Mathf.Max(parms.points, parms.raidStrategy.Worker.MinimumPoints(parms.faction) * 1.05f);
			}
		}

		public static IEnumerable<PawnGenOption> ChoosePawnGenOptionsByPoints(float points, List<PawnGenOption> options, IncidentParms parms)
		{
			float num = points;
			List<PawnGenOption> list = new List<PawnGenOption>();
			List<PawnGenOption> list2 = new List<PawnGenOption>();
			bool flag = false;
			while (true)
			{
				list.Clear();
				for (int i = 0; i < options.Count; i++)
				{
					PawnGenOption pawnGenOption = options[i];
					if (pawnGenOption.Cost <= num)
					{
						if (pawnGenOption.Cost <= PawnGroupMakerUtility.MaxAllowedPawnGenOptionCost(parms.faction, points, parms.raidStrategy))
						{
							if (!parms.generateFightersOnly || pawnGenOption.kind.isFighter)
							{
								if (parms.generateMeleeOnly)
								{
									if (pawnGenOption.kind.weaponTags.Any((string tag) => tag != "Melee"))
									{
										goto IL_F8;
									}
								}
								if (parms.raidStrategy == null || parms.raidStrategy.Worker.CanUsePawnGenOption(pawnGenOption, list2))
								{
									list.Add(pawnGenOption);
								}
							}
						}
					}
					IL_F8:;
				}
				if (flag)
				{
					list.RemoveAll((PawnGenOption group) => group.kind.factionLeader);
				}
				if (list.Count == 0)
				{
					break;
				}
				float desireToSuppressCount = Mathf.InverseLerp(800f, 1600f, points);
				desireToSuppressCount = Mathf.Clamp(desireToSuppressCount, 0f, 0.5f);
				Func<PawnGenOption, float> weightSelector = delegate(PawnGenOption gr)
				{
					float num2 = (float)gr.selectionWeight;
					if (desireToSuppressCount > 0f)
					{
						float b = num2 * gr.Cost;
						num2 = Mathf.Lerp(num2, b, desireToSuppressCount);
					}
					return num2;
				};
				PawnGenOption pawnGenOption2 = list.RandomElementByWeight(weightSelector);
				if (pawnGenOption2.kind.factionLeader)
				{
					flag = true;
				}
				list2.Add(pawnGenOption2);
				num -= pawnGenOption2.Cost;
			}
			return list2;
		}

		private static float MaxAllowedPawnGenOptionCost(Faction faction, float totalPoints, RaidStrategyDef raidStrategy)
		{
			float num = Mathf.Max(totalPoints * 0.5f, 50f);
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
						"======== FACTION: ",
						fac.Name,
						" (",
						fac.def.defName,
						") min=",
						fac.def.MinPointsToGenerateNormalPawnGroup(),
						" ======="
					}));
					Action<float> action = delegate(float points)
					{
						if (points < fac.def.MinPointsToGenerateNormalPawnGroup())
						{
							return;
						}
						IncidentParms incidentParms = new IncidentParms();
						incidentParms.points = points;
						sb.AppendLine("Group with " + incidentParms.points + " points");
						float num = 0f;
						foreach (Pawn current in PawnGroupMakerUtility.GenerateArrivingPawns(incidentParms, false))
						{
							string text;
							if (current.equipment.Primary != null)
							{
								text = current.equipment.Primary.Label;
							}
							else
							{
								text = "NoEquipment";
							}
							sb.AppendLine(string.Concat(new string[]
							{
								"  ",
								current.kindDef.combatPower.ToString("F0").PadRight(5),
								"- ",
								current.kindDef.defName,
								", ",
								text
							}));
							num += current.kindDef.combatPower;
						}
						sb.AppendLine("         totalCost " + num);
						sb.AppendLine();
					};
					action(35f);
					action(70f);
					action(135f);
					action(200f);
					action(300f);
					action(500f);
					action(800f);
					action(1200f);
					action(2000f);
					action(3000f);
					action(4000f);
					Log.Message(sb.ToString());
				}
			}
		}
	}
}
