using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsTextGen
	{
		[Category("Text generation"), DebugOutput]
		public static void FlavorfulCombatTest()
		{
			DebugOutputsTextGen.<FlavorfulCombatTest>c__AnonStorey1 <FlavorfulCombatTest>c__AnonStorey = new DebugOutputsTextGen.<FlavorfulCombatTest>c__AnonStorey1();
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			<FlavorfulCombatTest>c__AnonStorey.maneuvers = DefDatabase<ManeuverDef>.AllDefsListForReading;
			DebugOutputsTextGen.<FlavorfulCombatTest>c__AnonStorey1 arg_BE_0 = <FlavorfulCombatTest>c__AnonStorey;
			Func<ManeuverDef, RulePackDef>[] expr_1E = new Func<ManeuverDef, RulePackDef>[5];
			expr_1E[0] = ((ManeuverDef m) => new RulePackDef[]
			{
				m.combatLogRulesHit,
				m.combatLogRulesDeflect,
				m.combatLogRulesMiss,
				m.combatLogRulesDodge
			}.RandomElement<RulePackDef>());
			expr_1E[1] = ((ManeuverDef m) => m.combatLogRulesHit);
			expr_1E[2] = ((ManeuverDef m) => m.combatLogRulesDeflect);
			expr_1E[3] = ((ManeuverDef m) => m.combatLogRulesMiss);
			expr_1E[4] = ((ManeuverDef m) => m.combatLogRulesDodge);
			arg_BE_0.results = expr_1E;
			string[] array = new string[]
			{
				"(random)",
				"Hit",
				"Deflect",
				"Miss",
				"Dodge"
			};
			foreach (Pair<ManeuverDef, int> maneuverresult in <FlavorfulCombatTest>c__AnonStorey.maneuvers.Concat(null).Cross(Enumerable.Range(0, array.Length)))
			{
				DebugMenuOption item = new DebugMenuOption(string.Format("{0}/{1}", (maneuverresult.First != null) ? maneuverresult.First.defName : "(random)", array[maneuverresult.Second]), DebugMenuOptionMode.Action, delegate
				{
					DebugOutputsTextGen.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < 100; i++)
						{
							ManeuverDef maneuver = maneuverresult.First;
							if (maneuver == null)
							{
								maneuver = <FlavorfulCombatTest>c__AnonStorey.maneuvers.RandomElement<ManeuverDef>();
							}
							RulePackDef rulePackDef = <FlavorfulCombatTest>c__AnonStorey.results[maneuverresult.Second](maneuver);
							List<BodyPartRecord> list2 = null;
							List<bool> list3 = null;
							if (rulePackDef == maneuver.combatLogRulesHit)
							{
								list2 = new List<BodyPartRecord>();
								list3 = new List<bool>();
								bodyPartCreator(list2, list3);
							}
							Pair<ThingDef, Tool> pair = (from ttp in (from td in DefDatabase<ThingDef>.AllDefsListForReading
							where td.IsMeleeWeapon && !td.tools.NullOrEmpty<Tool>()
							select td).SelectMany((ThingDef td) => from tool in td.tools
							select new Pair<ThingDef, Tool>(td, tool))
							where ttp.Second.capacities.Contains(maneuver.requiredCapacity)
							select ttp).RandomElement<Pair<ThingDef, Tool>>();
							BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePackDef, false, CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), (pair.Second == null) ? ImplementOwnerTypeDefOf.Bodypart : ImplementOwnerTypeDefOf.Weapon, (pair.Second == null) ? "body part" : pair.Second.label, pair.First, null, null);
							battleLogEntry_MeleeCombat.FillTargets(list2, list3, battleLogEntry_MeleeCombat.RuleDef.defName.Contains("Deflect"));
							battleLogEntry_MeleeCombat.Debug_OverrideTicks(Rand.Int);
							stringBuilder.AppendLine(battleLogEntry_MeleeCombat.ToGameStringFromPOV(null, false));
						}
						Log.Message(stringBuilder.ToString(), false);
					});
				});
				list.Add(item);
			}
			int rf;
			for (rf = 0; rf < 2; rf++)
			{
				list.Add(new DebugMenuOption((rf != 0) ? "Ranged fire burst" : "Ranged fire singleshot", DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ThingDef thingDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement<ThingDef>();
						bool flag = Rand.Value < 0.2f;
						bool flag2 = !flag && Rand.Value < 0.95f;
						BattleLogEntry_RangedFire battleLogEntry_RangedFire = new BattleLogEntry_RangedFire(CombatLogTester.GenerateRandom(), (!flag) ? CombatLogTester.GenerateRandom() : null, (!flag2) ? thingDef : null, null, rf != 0);
						battleLogEntry_RangedFire.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_RangedFire.ToGameStringFromPOV(null, false));
					}
					Log.Message(stringBuilder.ToString(), false);
				}));
			}
			list.Add(new DebugMenuOption("Ranged impact hit", DebugMenuOptionMode.Action, delegate
			{
				DebugOutputsTextGen.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement<ThingDef>();
						List<BodyPartRecord> list2 = new List<BodyPartRecord>();
						List<bool> list3 = new List<bool>();
						bodyPartCreator(list2, list3);
						Pawn pawn = CombatLogTester.GenerateRandom();
						BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), pawn, pawn, weaponDef, null, ThingDefOf.Wall);
						battleLogEntry_RangedImpact.FillTargets(list2, list3, Rand.Chance(0.5f));
						battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null, false));
					}
					Log.Message(stringBuilder.ToString(), false);
				});
			}));
			list.Add(new DebugMenuOption("Ranged impact miss", DebugMenuOptionMode.Action, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 100; i++)
				{
					ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
					where td.IsRangedWeapon
					select td).RandomElement<ThingDef>();
					BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), null, CombatLogTester.GenerateRandom(), weaponDef, null, ThingDefOf.Wall);
					battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
					stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null, false));
				}
				Log.Message(stringBuilder.ToString(), false);
			}));
			list.Add(new DebugMenuOption("Ranged impact hit incorrect", DebugMenuOptionMode.Action, delegate
			{
				DebugOutputsTextGen.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						ThingDef weaponDef = (from td in DefDatabase<ThingDef>.AllDefsListForReading
						where td.IsRangedWeapon
						select td).RandomElement<ThingDef>();
						List<BodyPartRecord> list2 = new List<BodyPartRecord>();
						List<bool> list3 = new List<bool>();
						bodyPartCreator(list2, list3);
						BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), CombatLogTester.GenerateRandom(), weaponDef, null, ThingDefOf.Wall);
						battleLogEntry_RangedImpact.FillTargets(list2, list3, Rand.Chance(0.5f));
						battleLogEntry_RangedImpact.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_RangedImpact.ToGameStringFromPOV(null, false));
					}
					Log.Message(stringBuilder.ToString(), false);
				});
			}));
			foreach (RulePackDef transition in from def in DefDatabase<RulePackDef>.AllDefsListForReading
			where def.defName.Contains("Transition") && !def.defName.Contains("Include")
			select def)
			{
				list.Add(new DebugMenuOption(transition.defName, DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 100; i++)
					{
						Pawn pawn = CombatLogTester.GenerateRandom();
						Pawn initiator = CombatLogTester.GenerateRandom();
						BodyPartRecord partRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).RandomElement<BodyPartRecord>();
						BattleLogEntry_StateTransition battleLogEntry_StateTransition = new BattleLogEntry_StateTransition(pawn, transition, initiator, HediffMaker.MakeHediff(DefDatabase<HediffDef>.AllDefsListForReading.RandomElement<HediffDef>(), pawn, partRecord), pawn.RaceProps.body.AllParts.RandomElement<BodyPartRecord>());
						battleLogEntry_StateTransition.Debug_OverrideTicks(Rand.Int);
						stringBuilder.AppendLine(battleLogEntry_StateTransition.ToGameStringFromPOV(null, false));
					}
					Log.Message(stringBuilder.ToString(), false);
				}));
			}
			foreach (RulePackDef damageEvent in from def in DefDatabase<RulePackDef>.AllDefsListForReading
			where def.defName.Contains("DamageEvent") && !def.defName.Contains("Include")
			select def)
			{
				list.Add(new DebugMenuOption(damageEvent.defName, DebugMenuOptionMode.Action, delegate
				{
					DebugOutputsTextGen.CreateDamagedDestroyedMenu(delegate(Action<List<BodyPartRecord>, List<bool>> bodyPartCreator)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < 100; i++)
						{
							List<BodyPartRecord> list2 = new List<BodyPartRecord>();
							List<bool> list3 = new List<bool>();
							bodyPartCreator(list2, list3);
							Pawn recipient = CombatLogTester.GenerateRandom();
							BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(recipient, damageEvent, null);
							battleLogEntry_DamageTaken.FillTargets(list2, list3, false);
							battleLogEntry_DamageTaken.Debug_OverrideTicks(Rand.Int);
							stringBuilder.AppendLine(battleLogEntry_DamageTaken.ToGameStringFromPOV(null, false));
						}
						Log.Message(stringBuilder.ToString(), false);
					});
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static void CreateDamagedDestroyedMenu(Action<Action<List<BodyPartRecord>, List<bool>>> callback)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<int> damagedes = Enumerable.Range(0, 5);
			IEnumerable<int> destroyedes = Enumerable.Range(0, 5);
			foreach (Pair<int, int> damageddestroyed in damagedes.Concat(-1).Cross(destroyedes.Concat(-1)))
			{
				DebugMenuOption item = new DebugMenuOption(string.Format("{0} damaged/{1} destroyed", (damageddestroyed.First != -1) ? damageddestroyed.First.ToString() : "(random)", (damageddestroyed.Second != -1) ? damageddestroyed.Second.ToString() : "(random)"), DebugMenuOptionMode.Action, delegate
				{
					callback(delegate(List<BodyPartRecord> bodyparts, List<bool> flags)
					{
						int num = damageddestroyed.First;
						int destroyed = damageddestroyed.Second;
						if (num == -1)
						{
							num = damagedes.RandomElement<int>();
						}
						if (destroyed == -1)
						{
							destroyed = destroyedes.RandomElement<int>();
						}
						Pair<BodyPartRecord, bool>[] source = (from idx in Enumerable.Range(0, num + destroyed)
						select new Pair<BodyPartRecord, bool>(BodyDefOf.Human.AllParts.RandomElement<BodyPartRecord>(), idx < destroyed)).InRandomOrder(null).ToArray<Pair<BodyPartRecord, bool>>();
						bodyparts.Clear();
						flags.Clear();
						bodyparts.AddRange(from part in source
						select part.First);
						flags.AddRange(from part in source
						select part.Second);
					});
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[Category("Text generation"), DebugOutput]
		public static void ArtDescsSpecificTale()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TaleDef current in from def in DefDatabase<TaleDef>.AllDefs
			orderby def.defName
			select def)
			{
				TaleDef localDef = current;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					DebugOutputsTextGen.LogSpecificTale(localDef, 40);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[Category("Text generation"), DebugOutput]
		public static void NamesFromRulepack()
		{
			IEnumerable<RulePackDef> first = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.factionNameMaker;
			IEnumerable<RulePackDef> second = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.settlementNameMaker;
			IEnumerable<RulePackDef> second2 = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.playerInitialSettlementNameMaker;
			IEnumerable<RulePackDef> second3 = from f in DefDatabase<FactionDef>.AllDefsListForReading
			select f.pawnNameMaker;
			IEnumerable<RulePackDef> second4 = from d in DefDatabase<RulePackDef>.AllDefsListForReading
			where d.defName.Contains("Namer")
			select d;
			IOrderedEnumerable<RulePackDef> orderedEnumerable = from d in (from d in first.Concat(second).Concat(second2).Concat(second3).Concat(second4)
			where d != null
			select d).Distinct<RulePackDef>()
			orderby d.defName
			select d;
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (RulePackDef current in orderedEnumerable)
			{
				RulePackDef localNamer = current;
				FloatMenuOption item = new FloatMenuOption(localNamer.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					RulePackDef localNamer;
					stringBuilder.AppendLine("Testing RulePack " + localNamer.defName + " as  a name generator:");
					for (int i = 0; i < 200; i++)
					{
						string text = (i % 2 != 0) ? null : "Smithee";
						StringBuilder arg_58_0 = stringBuilder;
						localNamer = localNamer;
						string testPawnNameSymbol = text;
						arg_58_0.AppendLine(NameGenerator.GenerateName(localNamer, null, false, null, testPawnNameSymbol));
					}
					Log.Message(stringBuilder.ToString(), false);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[Category("Text generation"), DebugOutput, ModeRestrictionPlay]
		public static void DatabaseTalesList()
		{
			Find.TaleManager.LogTales();
		}

		[Category("Text generation"), DebugOutput, ModeRestrictionPlay]
		public static void DatabaseTalesInterest()
		{
			Find.TaleManager.LogTaleInterestSummary();
		}

		[Category("Text generation"), DebugOutput, ModeRestrictionPlay]
		public static void ArtDescsDatabaseTales()
		{
			DebugOutputsTextGen.LogTales(from t in Find.TaleManager.AllTalesListForReading
			where t.def.usableForArt
			select t);
		}

		[Category("Text generation"), DebugOutput, ModeRestrictionPlay]
		public static void ArtDescsRandomTales()
		{
			int num = 40;
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < num; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale(null));
			}
			DebugOutputsTextGen.LogTales(list);
		}

		[Category("Text generation"), DebugOutput, ModeRestrictionPlay]
		public static void ArtDescsTaleless()
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < 20; i++)
			{
				list.Add(null);
			}
			DebugOutputsTextGen.LogTales(list);
		}

		[Category("Text generation"), DebugOutput]
		public static void InteractionLogs()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (InteractionDef def in DefDatabase<InteractionDef>.AllDefsListForReading)
			{
				list.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, null);
					Pawn recipient = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, null);
					for (int i = 0; i < 100; i++)
					{
						PlayLogEntry_Interaction playLogEntry_Interaction = new PlayLogEntry_Interaction(def, pawn, recipient, null);
						stringBuilder.AppendLine(playLogEntry_Interaction.ToGameStringFromPOV(pawn, false));
					}
					Log.Message(stringBuilder.ToString(), false);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		private static void LogSpecificTale(TaleDef def, int count)
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < count; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale(def));
			}
			DebugOutputsTextGen.LogTales(list);
		}

		private static void LogTales(IEnumerable<Tale> tales)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (Tale current in tales)
			{
				TaleReference tr = new TaleReference(current);
				stringBuilder.AppendLine(DebugOutputsTextGen.RandomArtworkName(tr));
				stringBuilder.AppendLine(DebugOutputsTextGen.RandomArtworkDescription(tr));
				stringBuilder.AppendLine();
				num++;
				if (num % 20 == 0)
				{
					Log.Message(stringBuilder.ToString(), false);
					stringBuilder = new StringBuilder();
				}
			}
			if (!stringBuilder.ToString().NullOrEmpty())
			{
				Log.Message(stringBuilder.ToString(), false);
			}
		}

		private static string RandomArtworkName(TaleReference tr)
		{
			RulePackDef extraInclude = null;
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				extraInclude = RulePackDefOf.NamerArtSculpture;
				break;
			case 1:
				extraInclude = RulePackDefOf.NamerArtWeaponMelee;
				break;
			case 2:
				extraInclude = RulePackDefOf.NamerArtWeaponGun;
				break;
			case 3:
				extraInclude = RulePackDefOf.NamerArtFurniture;
				break;
			case 4:
				extraInclude = RulePackDefOf.NamerArtSarcophagusPlate;
				break;
			}
			return tr.GenerateText(TextGenerationPurpose.ArtName, extraInclude);
		}

		private static string RandomArtworkDescription(TaleReference tr)
		{
			RulePackDef extraInclude = null;
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				extraInclude = RulePackDefOf.ArtDescription_Sculpture;
				break;
			case 1:
				extraInclude = RulePackDefOf.ArtDescription_WeaponMelee;
				break;
			case 2:
				extraInclude = RulePackDefOf.ArtDescription_WeaponGun;
				break;
			case 3:
				extraInclude = RulePackDefOf.ArtDescription_Furniture;
				break;
			case 4:
				extraInclude = RulePackDefOf.ArtDescription_SarcophagusPlate;
				break;
			}
			return tr.GenerateText(TextGenerationPurpose.ArtDescription, extraInclude);
		}
	}
}
