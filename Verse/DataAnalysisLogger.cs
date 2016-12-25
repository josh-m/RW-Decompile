using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	internal static class DataAnalysisLogger
	{
		public static void DoLog_MinifiableTags()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.Minifiable)
				{
					stringBuilder.Append(current.defName);
					if (!current.tradeTags.NullOrEmpty<string>())
					{
						stringBuilder.Append(" - ");
						stringBuilder.Append(GenText.ToCommaList(current.tradeTags, true));
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_ItemDeteriorationRates()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && !d.destroyOnDrop && d.useHitPoints
			orderby d.GetStatValueAbstract(StatDefOf.DeteriorationRate, null) descending
			select d)
			{
				stringBuilder.AppendLine(current.defName + "  " + current.GetStatValueAbstract(StatDefOf.DeteriorationRate, null).ToString("F1"));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_ItemBeauties()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Item && !d.destroyOnDrop
			orderby d.GetStatValueAbstract(StatDefOf.Beauty, null) descending
			select d)
			{
				stringBuilder.AppendLine(current.defName + "  " + current.GetStatValueAbstract(StatDefOf.Beauty, null).ToString("F1"));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_TestRulepack()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (RulePackDef current in DefDatabase<RulePackDef>.AllDefs)
			{
				RulePackDef localNamer = current;
				FloatMenuOption item = new FloatMenuOption(localNamer.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < 200; i++)
					{
						stringBuilder.AppendLine(NameGenerator.GenerateName(localNamer, null, false));
					}
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_WeaponApparelGenData()
		{
			PawnApparelGenerator.LogGenerationData();
			PawnWeaponGenerator.LogGenerationData();
		}

		public static void DoLog_GeneratedNames()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (RulePackDef current in DefDatabase<RulePackDef>.AllDefs)
			{
				RulePackDef localRp = current;
				FloatMenuOption item = new FloatMenuOption(localRp.defName, delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Test names for " + localRp.defName + ":");
					for (int i = 0; i < 200; i++)
					{
						stringBuilder.AppendLine(NameGenerator.GenerateName(localRp, null, false));
					}
					Log.Message(stringBuilder.ToString());
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_ThingList()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			using (IEnumerator enumerator = Enum.GetValues(typeof(ThingRequestGroup)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ThingRequestGroup localRg2 = (ThingRequestGroup)((byte)enumerator.Current);
					ThingRequestGroup localRg = localRg2;
					FloatMenuOption item = new FloatMenuOption(localRg.ToString(), delegate
					{
						StringBuilder stringBuilder = new StringBuilder();
						List<Thing> list2 = Find.VisibleMap.listerThings.ThingsInGroup(localRg);
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							"Global things in group ",
							localRg,
							" (count ",
							list2.Count,
							")"
						}));
						Log.Message(DebugLogsUtility.ThingListToUniqueCountString(list2));
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
					list.Add(item);
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DoLog_MiscIncidentChances()
		{
			Log.Message("(note that some incident makers never yield misc incidents)");
			List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				storytellerComps[i].DebugLogIncidentChances(IncidentCategory.Misc);
			}
		}

		public static void DoLog_SimpleCurveTest()
		{
			StringBuilder stringBuilder = new StringBuilder();
			SimpleCurve simpleCurve = new SimpleCurve
			{
				new CurvePoint(5f, 0f),
				new CurvePoint(10f, 1f),
				new CurvePoint(20f, 3f),
				new CurvePoint(40f, 2f)
			};
			for (int i = 0; i < 50; i++)
			{
				stringBuilder.AppendLine(i + " -> " + simpleCurve.Evaluate((float)i));
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_TestPawnNames()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("------Testing parsing");
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick' Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick' von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick Hell' Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'Nick Hell' von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John Nick Hell von Smith"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John O'Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'O'Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'O'Farley' Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("John 'O'''Farley' Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("O'Shea 'O'Farley' O'Neil"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("Missing 'Lastname'"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("Missing 'Lastnamewithspace'     "));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("Unbalanc'ed 'De'limiters'     "));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult("\t"));
			stringBuilder.AppendLine(DataAnalysisLogger.PawnNameTestResult(string.Empty));
			stringBuilder.AppendLine("------Testing ResolveMissingPieces consistency");
			for (int i = 0; i < 20; i++)
			{
				NameTriple nameTriple = new NameTriple("John", null, "Last");
				nameTriple.ResolveMissingPieces(null);
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					nameTriple.ToString(),
					"       [",
					nameTriple.First,
					"] [",
					nameTriple.Nick,
					"] [",
					nameTriple.Last,
					"]"
				}));
			}
			Log.Message(stringBuilder.ToString());
		}

		private static string PawnNameTestResult(string rawName)
		{
			NameTriple nameTriple = NameTriple.FromString(rawName);
			nameTriple.ResolveMissingPieces(null);
			return string.Concat(new string[]
			{
				rawName,
				" -> ",
				nameTriple.ToString(),
				"       [",
				nameTriple.First,
				"] [",
				nameTriple.Nick,
				"] [",
				nameTriple.Last,
				"]"
			});
		}

		public static void DoLog_PassabilityFill()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				if (current.passability != Traversability.Standable || current.fillPercent > 0f)
				{
					stringBuilder.Append(string.Concat(new string[]
					{
						current.defName,
						" - pass=",
						current.passability.ToString(),
						", fill=",
						current.fillPercent.ToStringPercent()
					}));
					if (current.passability == Traversability.Impassable && current.fillPercent < 0.1f)
					{
						stringBuilder.Append("   ALERT, impassable with low fill");
					}
					if (current.passability != Traversability.Impassable && current.fillPercent > 0.8f)
					{
						stringBuilder.Append("    ALERT, passabile with very high fill");
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_AnimalsPerBiome()
		{
			IEnumerable<BiomeDef> enumerable = from d in DefDatabase<BiomeDef>.AllDefs
			where d.animalDensity > 0f
			select d;
			IOrderedEnumerable<PawnKindDef> orderedEnumerable = from d in DefDatabase<PawnKindDef>.AllDefs
			where d.race.race.Animal
			orderby (!d.race.race.predator) ? 0 : 1
			select d;
			string text = string.Empty;
			text += "name      commonality     commonalityShare\n\n";
			foreach (BiomeDef b in enumerable)
			{
				float num = orderedEnumerable.Sum((PawnKindDef a) => b.CommonalityOfAnimal(a));
				float f = (from a in orderedEnumerable
				where a.race.race.predator
				select a).Sum((PawnKindDef a) => b.CommonalityOfAnimal(a)) / num;
				string text2 = text;
				text = string.Concat(new string[]
				{
					text2,
					b.label,
					"   (predators: ",
					f.ToStringPercent("F2"),
					")"
				});
				foreach (PawnKindDef current in orderedEnumerable)
				{
					float num2 = b.CommonalityOfAnimal(current);
					if (num2 > 0f)
					{
						text2 = text;
						text = string.Concat(new string[]
						{
							text2,
							"\n    ",
							current.label,
							"       ",
							num2.ToString("F3"),
							"       ",
							(num2 / num).ToStringPercent("F2")
						});
					}
				}
				text += "\n\n";
			}
			Log.Message(text);
		}

		public static void DoLog_SmeltProducts()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
			{
				Thing thing = ThingMaker.MakeThing(current, GenStuff.DefaultStuffFor(current));
				if (thing.SmeltProducts(1f).Any<Thing>())
				{
					stringBuilder.Append(thing.LabelCap + ": ");
					foreach (Thing current2 in thing.SmeltProducts(1f))
					{
						stringBuilder.Append(" " + current2.Label);
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_PawnArrivalCandidates()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(IncidentDefOf.RaidEnemy.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidEnemy.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.RaidFriendly.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidFriendly.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.VisitorGroup.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.VisitorGroup.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.TravelerGroup.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TravelerGroup.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.TraderCaravanArrival.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TraderCaravanArrival.Worker).DebugListingOfGroupSources());
			Log.Message(stringBuilder.ToString());
		}

		public static void DoLog_SpecificTaleDescs()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TaleDef current in DefDatabase<TaleDef>.AllDefs)
			{
				TaleDef localDef = current;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					TaleTester.LogSpecificTale(localDef, 40);
				}, MenuOptionPriority.Default, null, null, 0f, null, null);
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
