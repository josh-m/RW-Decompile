using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class TrainableUtility
	{
		private static List<TrainableDef> defsInListOrder = new List<TrainableDef>();

		public const int MinTrainInterval = 15000;

		private static readonly SimpleCurve DecayIntervalDaysFromWildnessCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 12f),
				true
			},
			{
				new CurvePoint(1f, 6f),
				true
			}
		};

		public static List<TrainableDef> TrainableDefsInListOrder
		{
			get
			{
				return TrainableUtility.defsInListOrder;
			}
		}

		public static void Reset()
		{
			TrainableUtility.defsInListOrder.Clear();
			TrainableUtility.defsInListOrder.AddRange(from td in DefDatabase<TrainableDef>.AllDefsListForReading
			orderby td.listPriority descending
			select td);
			bool flag;
			do
			{
				flag = false;
				for (int i = 0; i < TrainableUtility.defsInListOrder.Count; i++)
				{
					TrainableDef trainableDef = TrainableUtility.defsInListOrder[i];
					if (trainableDef.prerequisites != null)
					{
						for (int j = 0; j < trainableDef.prerequisites.Count; j++)
						{
							if (trainableDef.indent <= trainableDef.prerequisites[j].indent)
							{
								trainableDef.indent = trainableDef.prerequisites[j].indent + 1;
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			while (flag);
		}

		public static string MasterString(Pawn pawn)
		{
			return (pawn.playerSettings.Master == null) ? ("(" + "NoneLower".Translate() + ")") : RelationsUtility.LabelWithBondInfo(pawn.playerSettings.Master, pawn);
		}

		public static int MinimumHandlingSkill(Pawn p)
		{
			return Mathf.RoundToInt(p.GetStatValue(StatDefOf.MinimumHandlingSkill, true));
		}

		public static void MasterSelectButton(Rect rect, Pawn pawn, bool paintable)
		{
			Rect rect2 = rect;
			Func<Pawn, Pawn> getPayload = new Func<Pawn, Pawn>(TrainableUtility.MasterSelectButton_GetMaster);
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<Pawn>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<Pawn>>>(TrainableUtility.MasterSelectButton_GenerateMenu);
			string buttonLabel = TrainableUtility.MasterString(pawn).Truncate(rect.width, null);
			string dragLabel = TrainableUtility.MasterString(pawn);
			Widgets.Dropdown<Pawn, Pawn>(rect2, pawn, getPayload, menuGenerator, buttonLabel, null, dragLabel, null, null, paintable);
		}

		private static Pawn MasterSelectButton_GetMaster(Pawn pet)
		{
			return pet.playerSettings.Master;
		}

		[DebuggerHidden]
		private static IEnumerable<Widgets.DropdownMenuElement<Pawn>> MasterSelectButton_GenerateMenu(Pawn p)
		{
			yield return new Widgets.DropdownMenuElement<Pawn>
			{
				option = new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
				{
					p.playerSettings.Master = null;
				}, MenuOptionPriority.Default, null, null, 0f, null, null),
				payload = null
			};
			foreach (Pawn col in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				string lab = RelationsUtility.LabelWithBondInfo(col, p);
				Action action = null;
				int level = col.skills.GetSkill(SkillDefOf.Animals).Level;
				int minLevel = TrainableUtility.MinimumHandlingSkill(p);
				if (level < minLevel)
				{
					action = null;
					lab = lab + " (" + "SkillTooLow".Translate(SkillDefOf.Animals.LabelCap, level, minLevel) + ")";
				}
				else if (TrainableUtility.CanBeMaster(col, p, true))
				{
					action = delegate
					{
						p.playerSettings.Master = col;
					};
				}
				yield return new Widgets.DropdownMenuElement<Pawn>
				{
					option = new FloatMenuOption(lab, action, MenuOptionPriority.Default, null, null, 0f, null, null),
					payload = col
				};
			}
		}

		public static bool CanBeMaster(Pawn master, Pawn animal, bool checkSpawned = true)
		{
			if ((checkSpawned && !master.Spawned) || master.IsPrisoner)
			{
				return false;
			}
			int level = master.skills.GetSkill(SkillDefOf.Animals).Level;
			int num = TrainableUtility.MinimumHandlingSkill(animal);
			return level >= num;
		}

		public static string GetIconTooltipText(Pawn pawn)
		{
			string text = string.Empty;
			if (pawn.playerSettings != null && pawn.playerSettings.Master != null)
			{
				text += string.Format("{0}: {1}\n", "Master".Translate(), pawn.playerSettings.Master.LabelShort);
			}
			IEnumerable<Pawn> allColonistBondsFor = TrainableUtility.GetAllColonistBondsFor(pawn);
			if (allColonistBondsFor.Any<Pawn>())
			{
				text += string.Format("{0}: {1}\n", "BondedTo".Translate(), (from bond in allColonistBondsFor
				select bond.LabelShort).ToCommaList(true));
			}
			return text.TrimEndNewlines();
		}

		public static IEnumerable<Pawn> GetAllColonistBondsFor(Pawn pet)
		{
			return from bond in pet.relations.DirectRelations
			where bond.def == PawnRelationDefOf.Bond && bond.otherPawn != null && bond.otherPawn.IsColonist
			select bond.otherPawn;
		}

		public static int DegradationPeriodTicks(ThingDef def)
		{
			return Mathf.RoundToInt(TrainableUtility.DecayIntervalDaysFromWildnessCurve.Evaluate(def.race.wildness) * 60000f);
		}

		public static bool TamenessCanDecay(ThingDef def)
		{
			return def.race.wildness > 0.101f;
		}

		public static bool TrainedTooRecently(Pawn animal)
		{
			return Find.TickManager.TicksGame < animal.mindState.lastAssignedInteractTime + 15000;
		}

		public static string GetWildnessExplanation(ThingDef def)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("WildnessExplanation".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(string.Format("{0}: {1}", "TrainingDecayInterval".Translate(), TrainableUtility.DegradationPeriodTicks(def).ToStringTicksToDays("F1")));
			if (!TrainableUtility.TamenessCanDecay(def))
			{
				stringBuilder.AppendLine("TamenessWillNotDecay".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
