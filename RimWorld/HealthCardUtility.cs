using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class HealthCardUtility
	{
		private const float ThoughtLevelHeight = 25f;

		private const float ThoughtLevelSpacing = 4f;

		private const float IconSize = 20f;

		private static Vector2 scrollPosition = Vector2.zero;

		private static float scrollViewHeight = 0f;

		private static bool highlight = true;

		private static bool onOperationTab = false;

		private static Vector2 billsScrollPosition = Vector2.zero;

		private static float billsScrollHeight = 1000f;

		private static bool showAllHediffs = false;

		private static bool showHediffsDebugInfo = false;

		private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		private static readonly Color StaticHighlightColor = new Color(0.75f, 0.75f, 0.85f, 1f);

		private static readonly Color VeryPoorColor = new Color(0.4f, 0.4f, 0.4f);

		private static readonly Color PoorColor = new Color(0.55f, 0.55f, 0.55f);

		private static readonly Color WeakenedColor = new Color(0.7f, 0.7f, 0.7f);

		private static readonly Color EnhancedColor = new Color(0.5f, 0.5f, 0.9f);

		private static readonly Color MediumPainColor = new Color(0.9f, 0.9f, 0f);

		private static readonly Color SeverePainColor = new Color(0.9f, 0.5f, 0f);

		private static readonly Texture2D BleedingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding", true);

		private static readonly Texture2D TendedPoorIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedPoorly", true);

		private static readonly Texture2D TendedWellIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedWell", true);

		private static List<ThingDef> tmpMedicineBestToWorst = new List<ThingDef>();

		public static void DrawPawnHealthCard(Rect outRect, Pawn pawn, bool allowOperations, bool showBloodLoss, Thing thingForMedBills)
		{
			if (pawn.Dead && allowOperations)
			{
				Log.Error("Called DrawPawnHealthCard with a dead pawn and allowOperations=true. Operations are disallowed on corpses.");
				allowOperations = false;
			}
			outRect = outRect.Rounded();
			Rect rect = new Rect(outRect.x, outRect.y, outRect.width * 0.375f, outRect.height).Rounded();
			Rect rect2 = new Rect(rect.xMax, outRect.y, outRect.width - rect.width, outRect.height);
			rect.yMin += 11f;
			HealthCardUtility.DrawHealthSummary(rect, pawn, allowOperations, thingForMedBills);
			HealthCardUtility.DrawHediffListing(rect2.ContractedBy(10f), pawn, showBloodLoss);
		}

		public static void DrawHealthSummary(Rect rect, Pawn pawn, bool allowOperations, Thing thingForMedBills)
		{
			GUI.color = Color.white;
			if (!allowOperations)
			{
				HealthCardUtility.onOperationTab = false;
			}
			Widgets.DrawMenuSection(rect, true);
			List<TabRecord> list = new List<TabRecord>();
			list.Add(new TabRecord("HealthOverview".Translate(), delegate
			{
				HealthCardUtility.onOperationTab = false;
			}, !HealthCardUtility.onOperationTab));
			if (allowOperations)
			{
				string label = (!pawn.RaceProps.IsMechanoid) ? "MedicalOperationsShort".Translate(new object[]
				{
					pawn.BillStack.Count
				}) : "MedicalOperationsMechanoidsShort".Translate(new object[]
				{
					pawn.BillStack.Count
				});
				list.Add(new TabRecord(label, delegate
				{
					HealthCardUtility.onOperationTab = true;
				}, HealthCardUtility.onOperationTab));
			}
			TabDrawer.DrawTabs(rect, list);
			rect = rect.ContractedBy(9f);
			GUI.BeginGroup(rect);
			float curY = 0f;
			Text.Font = GameFont.Medium;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperCenter;
			if (HealthCardUtility.onOperationTab)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.MedicalOperations, KnowledgeAmount.FrameDisplayed);
				curY = HealthCardUtility.DrawMedOperationsTab(rect, pawn, thingForMedBills, curY);
			}
			else
			{
				curY = HealthCardUtility.DrawOverviewTab(rect, pawn, curY);
			}
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
		}

		public static void DrawHediffListing(Rect rect, Pawn pawn, bool showBloodLoss)
		{
			GUI.color = Color.white;
			if (Prefs.DevMode && Current.ProgramState == ProgramState.MapPlaying)
			{
				HealthCardUtility.DoDebugOptions(rect, pawn);
			}
			GUI.BeginGroup(rect);
			float lineHeight = Text.LineHeight;
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height - lineHeight);
			Rect viewRect = new Rect(0f, 0f, rect.width - 16f, HealthCardUtility.scrollViewHeight);
			Rect rect2 = rect;
			if (viewRect.height > outRect.height)
			{
				rect2.width -= 16f;
			}
			Widgets.BeginScrollView(outRect, ref HealthCardUtility.scrollPosition, viewRect);
			GUI.color = Color.white;
			float num = 0f;
			HealthCardUtility.highlight = true;
			bool flag = false;
			foreach (IGrouping<BodyPartRecord, Hediff> current in HealthCardUtility.VisibleHediffGroupsInOrder(pawn, showBloodLoss))
			{
				flag = true;
				HealthCardUtility.DrawHediffRow(rect2, pawn, current, ref num);
			}
			if (!flag)
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Rect rect3 = new Rect(0f, 0f, viewRect.width, 30f);
				Widgets.Label(rect3, "NoInjuries".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
			}
			if (Event.current.type == EventType.Layout)
			{
				HealthCardUtility.scrollViewHeight = num;
			}
			Widgets.EndScrollView();
			float bleedingRate = pawn.health.hediffSet.BleedingRate;
			if (bleedingRate > 0.01f)
			{
				Rect rect4 = new Rect(0f, rect.height - lineHeight, rect.width, lineHeight);
				Widgets.Label(rect4, string.Concat(new string[]
				{
					"BleedingRate".Translate(),
					": ",
					bleedingRate.ToStringPercent(),
					"/",
					"LetterDay".Translate()
				}));
			}
			GUI.EndGroup();
			GUI.color = Color.white;
		}

		[DebuggerHidden]
		private static IEnumerable<IGrouping<BodyPartRecord, Hediff>> VisibleHediffGroupsInOrder(Pawn pawn, bool showBloodLoss)
		{
			foreach (IGrouping<BodyPartRecord, Hediff> group in from x in HealthCardUtility.VisibleHediffs(pawn, showBloodLoss)
			group x by x.Part into x
			orderby HealthCardUtility.GetListPriority(x.First<Hediff>().Part) descending
			select x)
			{
				yield return group;
			}
		}

		private static float GetListPriority(BodyPartRecord rec)
		{
			if (rec == null)
			{
				return 9999999f;
			}
			return (float)((int)rec.height * 10000) + rec.absoluteCoverage;
		}

		[DebuggerHidden]
		private static IEnumerable<Hediff> VisibleHediffs(Pawn pawn, bool showBloodLoss)
		{
			if (!HealthCardUtility.showAllHediffs)
			{
				List<Hediff_MissingPart> mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < mpca.Count; i++)
				{
					yield return mpca[i];
				}
				IEnumerable<Hediff> visibleDiffs = from d in pawn.health.hediffSet.hediffs
				where !(d is Hediff_MissingPart) && d.Visible && (this.showBloodLoss || d.def != HediffDefOf.BloodLoss) && (!d.hiddenOffMap || Current.ProgramState == ProgramState.MapPlaying)
				select d;
				foreach (Hediff diff in visibleDiffs)
				{
					yield return diff;
				}
			}
			else
			{
				foreach (Hediff diff2 in pawn.health.hediffSet.hediffs)
				{
					yield return diff2;
				}
			}
		}

		private static float DrawMedOperationsTab(Rect leftRect, Pawn pawn, Thing thingForMedBills, float curY)
		{
			curY += 2f;
			Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (RecipeDef current in thingForMedBills.def.AllRecipes)
				{
					if (current.AvailableNow)
					{
						IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients(null);
						if (!enumerable.Any((ThingDef x) => x.isBodyPartOrImplant))
						{
							IEnumerable<BodyPartRecord> partsToApplyOn = current.Worker.GetPartsToApplyOn(pawn, current);
							if (partsToApplyOn.Any<BodyPartRecord>())
							{
								foreach (BodyPartRecord current2 in partsToApplyOn)
								{
									RecipeDef localRecipe = current;
									BodyPartRecord localPart = current2;
									string text;
									if (localRecipe == RecipeDefOf.RemoveBodyPart)
									{
										text = HealthCardUtility.RemoveBodyPartSpecialLabel(pawn, current2);
									}
									else
									{
										text = localRecipe.LabelCap;
									}
									if (!current.hideBodyPartNames)
									{
										text = text + " (" + current2.def.label + ")";
									}
									Action action = null;
									if (enumerable.Any<ThingDef>())
									{
										text += " (";
										bool flag = true;
										foreach (ThingDef current3 in enumerable)
										{
											if (!flag)
											{
												text += ", ";
											}
											flag = false;
											text += "MissingMedicalBillIngredient".Translate(new object[]
											{
												current3.label
											});
										}
										text += ")";
									}
									else
									{
										action = delegate
										{
											if (!Find.MapPawns.FreeColonists.Any((Pawn col) => localRecipe.PawnSatisfiesSkillRequirements(col)))
											{
												Bill.CreateNoPawnsWithSkillDialog(localRecipe);
											}
											Pawn pawn2 = thingForMedBills as Pawn;
											if (pawn2 == null)
											{
												return;
											}
											if (!pawn2.InBed() && pawn2.RaceProps.IsFlesh)
											{
												if (pawn2.RaceProps.Humanlike)
												{
													if (!Find.ListerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(pawn, x.def) && ((Building_Bed)x).Medical))
													{
														Messages.Message("MessageNoMedicalBeds".Translate(), pawn2, MessageSound.Negative);
													}
												}
												else if (!Find.ListerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(pawn, x.def)))
												{
													Messages.Message("MessageNoAnimalBeds".Translate(), pawn2, MessageSound.Negative);
												}
											}
											Bill_Medical bill_Medical = new Bill_Medical(localRecipe);
											pawn2.BillStack.AddBill(bill_Medical);
											bill_Medical.Part = localPart;
											if (localRecipe.conceptLearned != null)
											{
												PlayerKnowledgeDatabase.KnowledgeDemonstrated(localRecipe.conceptLearned, KnowledgeAmount.Total);
											}
											if (pawn2.Faction != null && !pawn2.Faction.def.hidden && !pawn2.Faction.HostileTo(Faction.OfPlayer) && localRecipe.Worker.IsViolationOnPawn(pawn2, localPart, Faction.OfPlayer))
											{
												Messages.Message("MessageMedicalOperationWillAngerFaction".Translate(new object[]
												{
													pawn2.Faction
												}), pawn2, MessageSound.Negative);
											}
											ThingDef minRequiredMedicine = HealthCardUtility.GetMinRequiredMedicine(localRecipe);
											if (minRequiredMedicine != null && pawn2.playerSettings != null && !pawn2.playerSettings.medCare.AllowsMedicine(minRequiredMedicine))
											{
												Messages.Message("MessageTooLowMedCare".Translate(new object[]
												{
													minRequiredMedicine.label,
													pawn2.LabelShort,
													pawn2.playerSettings.medCare.GetLabel()
												}), pawn2, MessageSound.Negative);
											}
										};
									}
									list.Add(new FloatMenuOption(text, action, MenuOptionPriority.Medium, null, null, 0f, null));
								}
							}
						}
					}
				}
				return list;
			};
			Rect rect = new Rect(leftRect.x - 9f, curY, leftRect.width, leftRect.height - curY - 20f);
			((IBillGiver)thingForMedBills).BillStack.DrawListing(rect, recipeOptionsMaker, ref HealthCardUtility.billsScrollPosition, ref HealthCardUtility.billsScrollHeight);
			return curY;
		}

		public static string RemoveBodyPartSpecialLabel(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.RaceProps.IsMechanoid || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
			{
				return RecipeDefOf.RemoveBodyPart.LabelCap;
			}
			BodyPartRemovalIntent bodyPartRemovalIntent = HealthUtility.PartRemovalIntent(pawn, part);
			if (bodyPartRemovalIntent == BodyPartRemovalIntent.Harvest)
			{
				return "Harvest".Translate();
			}
			if (bodyPartRemovalIntent != BodyPartRemovalIntent.Amputate)
			{
				throw new InvalidOperationException();
			}
			if (part.depth == BodyPartDepth.Inside)
			{
				return "RemoveOrgan".Translate();
			}
			return "Amputate".Translate();
		}

		private static ThingDef GetMinRequiredMedicine(RecipeDef recipe)
		{
			HealthCardUtility.tmpMedicineBestToWorst.Clear();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].IsMedicine)
				{
					HealthCardUtility.tmpMedicineBestToWorst.Add(allDefsListForReading[i]);
				}
			}
			HealthCardUtility.tmpMedicineBestToWorst.SortByDescending((ThingDef x) => x.GetStatValueAbstract(StatDefOf.MedicalPotency, null));
			ThingDef thingDef = null;
			for (int j = 0; j < recipe.ingredients.Count; j++)
			{
				ThingDef thingDef2 = null;
				for (int k = 0; k < HealthCardUtility.tmpMedicineBestToWorst.Count; k++)
				{
					if (recipe.ingredients[j].filter.Allows(HealthCardUtility.tmpMedicineBestToWorst[k]))
					{
						thingDef2 = HealthCardUtility.tmpMedicineBestToWorst[k];
					}
				}
				if (thingDef2 != null && (thingDef == null || thingDef2.GetStatValueAbstract(StatDefOf.MedicalPotency, null) > thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency, null)))
				{
					thingDef = thingDef2;
				}
			}
			HealthCardUtility.tmpMedicineBestToWorst.Clear();
			return thingDef;
		}

		private static float DrawOverviewTab(Rect leftRect, Pawn pawn, float curY)
		{
			curY += 4f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = new Color(0.9f, 0.9f, 0.9f);
			string text = string.Empty;
			if (pawn.gender != Gender.None)
			{
				text = pawn.gender.GetLabel() + " ";
			}
			text = text + pawn.def.label + ", " + "AgeIndicator".Translate(new object[]
			{
				pawn.ageTracker.AgeNumberString
			});
			Rect rect = new Rect(0f, curY, leftRect.width, 34f);
			Widgets.Label(rect, text.CapitalizeFirst());
			TooltipHandler.TipRegion(rect, () => pawn.ageTracker.AgeTooltipString, 73412);
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			GUI.color = Color.white;
			curY += 34f;
			if (pawn.playerSettings != null && !pawn.Dead && Current.ProgramState == ProgramState.MapPlaying)
			{
				Rect rect2 = new Rect(0f, curY, 140f, 28f);
				MedicalCareUtility.MedicalCareSetter(rect2, ref pawn.playerSettings.medCare);
				curY += 32f;
			}
			Text.Font = GameFont.Small;
			if (pawn.def.race.IsFlesh)
			{
				Pair<string, Color> painLabel = HealthCardUtility.GetPainLabel(pawn);
				string tipLabel = "PainLevel".Translate() + ": " + (pawn.health.hediffSet.Pain * 100f).ToString("F0") + "%";
				curY = HealthCardUtility.DrawLeftRow(leftRect, curY, "PainLevel".Translate(), painLabel.First, painLabel.Second, tipLabel);
			}
			if (!pawn.Dead)
			{
				IEnumerable<PawnCapacityDef> source;
				if (pawn.def.race.Humanlike)
				{
					source = from x in DefDatabase<PawnCapacityDef>.AllDefs
					where x.showOnHumanlikes
					select x;
				}
				else if (pawn.def.race.Animal)
				{
					source = from x in DefDatabase<PawnCapacityDef>.AllDefs
					where x.showOnAnimals
					select x;
				}
				else
				{
					source = from x in DefDatabase<PawnCapacityDef>.AllDefs
					where x.showOnMechanoids
					select x;
				}
				foreach (PawnCapacityDef current in from act in source
				orderby act.listOrder
				select act)
				{
					if (PawnCapacityUtility.BodyCanEverDoActivity(pawn.RaceProps.body, current))
					{
						Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, current);
						string tipLabel2 = "Efficiency".Translate() + ": " + (pawn.health.capacities.GetEfficiency(current) * 100f).ToString("F0") + "%";
						curY = HealthCardUtility.DrawLeftRow(leftRect, curY, current.GetLabelFor(pawn.RaceProps.IsFlesh, pawn.RaceProps.Humanlike).CapitalizeFirst(), efficiencyLabel.First, efficiencyLabel.Second, tipLabel2);
					}
				}
			}
			return curY;
		}

		private static float DrawLeftRow(Rect leftRect, float curY, string leftLabel, string rightLabel, Color rightLabelColor, string tipLabel)
		{
			Rect rect = new Rect(0f, curY, leftRect.width, 20f);
			if (Mouse.IsOver(rect))
			{
				GUI.color = HealthCardUtility.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			GUI.color = Color.white;
			Widgets.Label(new Rect(0f, curY, leftRect.width * 0.65f, 30f), new GUIContent(leftLabel));
			GUI.color = rightLabelColor;
			Widgets.Label(new Rect(leftRect.width * 0.65f, curY, leftRect.width * 0.35f, 30f), new GUIContent(rightLabel));
			TooltipHandler.TipRegion(new Rect(0f, curY, leftRect.width, 20f), new TipSignal(tipLabel));
			curY += 20f;
			return curY;
		}

		private static void DrawHediffRow(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
		{
			float num = rect.width * 0.375f;
			float width = rect.width - num - 20f;
			float a;
			if (diffs.First<Hediff>().Part == null)
			{
				a = Text.CalcHeight("WholeBody".Translate(), num);
			}
			else
			{
				a = Text.CalcHeight(diffs.First<Hediff>().Part.def.LabelCap, num);
			}
			float b = 0f;
			float num2 = curY;
			float num3 = 0f;
			foreach (IGrouping<int, Hediff> current in from x in diffs
			group x by x.UIGroupKey)
			{
				int num4 = current.Count<Hediff>();
				string text = current.First<Hediff>().LabelCap;
				if (num4 != 1)
				{
					text = text + " x" + num4.ToString();
				}
				num3 += Text.CalcHeight(text, width);
			}
			b = num3;
			Rect rect2 = new Rect(0f, curY, rect.width, Mathf.Max(a, b));
			HealthCardUtility.DoRightRowHighlight(rect2);
			if (diffs.First<Hediff>().Part != null)
			{
				GUI.color = HealthUtility.GetPartConditionLabel(pawn, diffs.First<Hediff>().Part).Second;
				Widgets.Label(new Rect(0f, curY, num, 100f), new GUIContent(diffs.First<Hediff>().Part.def.LabelCap));
			}
			else
			{
				GUI.color = HealthUtility.DarkRedColor;
				Widgets.Label(new Rect(0f, curY, num, 100f), new GUIContent("WholeBody".Translate()));
			}
			GUI.color = Color.white;
			foreach (IGrouping<int, Hediff> current2 in from x in diffs
			group x by x.UIGroupKey)
			{
				Hediff hediff = current2.First<Hediff>();
				int num5 = 0;
				bool flag = false;
				Texture2D texture2D = null;
				foreach (Hediff current3 in current2)
				{
					num5++;
					Hediff_Injury hediff_Injury = current3 as Hediff_Injury;
					if (hediff_Injury != null && hediff_Injury.IsTended() && !hediff_Injury.IsOld())
					{
						if (hediff_Injury.IsTendedWell())
						{
							texture2D = HealthCardUtility.TendedWellIcon;
						}
						else
						{
							texture2D = HealthCardUtility.TendedPoorIcon;
						}
					}
					float bleedRate = current3.BleedRate;
					if (bleedRate > 1E-05f)
					{
						flag = true;
					}
				}
				string text2 = hediff.LabelCap;
				if (num5 != 1)
				{
					text2 = text2 + " x" + num5.ToString();
				}
				GUI.color = hediff.LabelColor;
				float num6 = Text.CalcHeight(text2, width);
				Rect rect3 = new Rect(num, curY, width, num6);
				Widgets.Label(rect3, text2);
				GUI.color = Color.white;
				Rect position = new Rect(rect2.xMax - 20f, curY, 20f, 20f);
				if (flag)
				{
					GUI.DrawTexture(position, HealthCardUtility.BleedingIcon);
				}
				if (texture2D != null)
				{
					GUI.DrawTexture(position, texture2D);
				}
				curY += num6;
			}
			GUI.color = Color.white;
			curY = num2 + Mathf.Max(a, b);
			TooltipHandler.TipRegion(rect2, new TipSignal(() => HealthCardUtility.GetTooltip(diffs, pawn), (int)curY + 7857));
		}

		private static string GetTooltip(IEnumerable<Hediff> diffs, Pawn pawn)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (diffs.First<Hediff>().Part != null)
			{
				stringBuilder.Append(diffs.First<Hediff>().Part.def.LabelCap + ": ");
				stringBuilder.Append(" " + pawn.health.hediffSet.GetPartHealth(diffs.First<Hediff>().Part).ToString() + " / " + diffs.First<Hediff>().Part.def.GetMaxHealth(pawn).ToString());
			}
			else
			{
				stringBuilder.Append("WholeBody".Translate());
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("------------------");
			foreach (IGrouping<int, Hediff> current in from x in diffs
			group x by x.UIGroupKey)
			{
				foreach (Hediff current2 in current)
				{
					string damageLabel = current2.DamageLabel;
					bool flag = HealthCardUtility.showHediffsDebugInfo && !current2.DebugString().NullOrEmpty();
					if (!current2.Label.NullOrEmpty() || !damageLabel.NullOrEmpty() || !current2.CapMods.NullOrEmpty<PawnCapacityModifier>() || flag)
					{
						stringBuilder.Append(current2.LabelCap);
						if (!damageLabel.NullOrEmpty())
						{
							stringBuilder.Append(": " + damageLabel);
						}
						stringBuilder.AppendLine();
						string tipStringExtra = current2.TipStringExtra;
						if (!tipStringExtra.NullOrEmpty())
						{
							stringBuilder.AppendLine(tipStringExtra.TrimEndNewlines().Indented());
						}
						if (flag)
						{
							stringBuilder.AppendLine(current2.DebugString().TrimEndNewlines());
						}
					}
				}
			}
			return stringBuilder.ToString().TrimEnd(new char[0]);
		}

		private static void DoRightRowHighlight(Rect rowRect)
		{
			if (HealthCardUtility.highlight)
			{
				GUI.color = HealthCardUtility.StaticHighlightColor;
				GUI.DrawTexture(rowRect, TexUI.HighlightTex);
			}
			HealthCardUtility.highlight = !HealthCardUtility.highlight;
			if (Mouse.IsOver(rowRect))
			{
				GUI.color = HealthCardUtility.HighlightColor;
				GUI.DrawTexture(rowRect, TexUI.HighlightTex);
			}
		}

		private static void DoDebugOptions(Rect rightRect, Pawn pawn)
		{
			Widgets.CheckboxLabeled(new Rect(rightRect.x, rightRect.y - 25f, 110f, 30f), "Dev: AllDiffs", ref HealthCardUtility.showAllHediffs, false);
			Widgets.CheckboxLabeled(new Rect(rightRect.x + 115f, rightRect.y - 25f, 120f, 30f), "DiffsDebugInfo", ref HealthCardUtility.showHediffsDebugInfo, false);
			if (Widgets.ButtonText(new Rect(rightRect.x + 240f, rightRect.y - 27f, 115f, 25f), "Debug info", true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				list.Add(new FloatMenuOption("Parts hit chance (this part or any child node)", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in from x in pawn.RaceProps.body.AllParts
					orderby x.absoluteCoverage descending
					select x)
					{
						stringBuilder.AppendLine(current.def.LabelCap + " " + current.absoluteCoverage.ToStringPercent());
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("Parts hit chance (exactly this part)", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					float num = 0f;
					foreach (BodyPartRecord current in from x in pawn.RaceProps.body.AllParts
					orderby x.absoluteFleshCoverage descending
					select x)
					{
						stringBuilder.AppendLine(current.def.LabelCap + " " + current.absoluteFleshCoverage.ToStringPercent());
						num += current.absoluteFleshCoverage;
					}
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Total " + num.ToStringPercent());
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("Per-part efficiency", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in pawn.RaceProps.body.AllParts)
					{
						stringBuilder.AppendLine(current.def.LabelCap + " " + PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, current, false).ToStringPercent());
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("BodyPartGroup efficiency (of only natural parts)", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartGroupDef current in from x in DefDatabase<BodyPartGroupDef>.AllDefs
					where pawn.RaceProps.body.AllParts.Any((BodyPartRecord y) => y.groups.Contains(x))
					select x)
					{
						stringBuilder.AppendLine(current.LabelCap + " " + PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, current).ToStringPercent());
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("IsSolid", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in pawn.health.hediffSet.GetNotMissingParts(null, null))
					{
						stringBuilder.AppendLine(current.def.LabelCap + " " + current.def.IsSolid(current, pawn.health.hediffSet.hediffs));
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("IsSkinCovered", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in pawn.health.hediffSet.GetNotMissingParts(null, null))
					{
						stringBuilder.AppendLine(current.def.LabelCap + " " + current.def.IsSkinCovered(current, pawn.health.hediffSet));
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("does have added parts", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in pawn.health.hediffSet.GetNotMissingParts(null, null))
					{
						stringBuilder.AppendLine(current.def.LabelCap + " " + pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(current));
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("GetNotMissingParts", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in pawn.health.hediffSet.GetNotMissingParts(null, null))
					{
						stringBuilder.AppendLine(current.def.LabelCap);
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("GetCoverageOfNotMissingNaturalParts", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in from x in pawn.RaceProps.body.AllParts
					orderby pawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(x) descending
					select x)
					{
						stringBuilder.AppendLine(current.def.LabelCap + ": " + pawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(current).ToStringPercent());
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("parts nutrition", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (BodyPartRecord current in from x in pawn.RaceProps.body.AllParts
					orderby FoodUtility.GetBodyPartNutrition(pawn, x) descending
					select x)
					{
						stringBuilder.AppendLine(current.def.LabelCap + ": " + FoodUtility.GetBodyPartNutrition(pawn, current));
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("test old injury pain factor probability", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					float num4 = 0f;
					int num5 = 10000;
					for (int i = 0; i < num5; i++)
					{
						float randomPainFactor = OldInjuryUtility.GetRandomPainFactor();
						if (randomPainFactor < 0f)
						{
							Log.Error("Pain factor < 0");
						}
						if (randomPainFactor == 0f)
						{
							num++;
						}
						if (randomPainFactor < 1f)
						{
							num2++;
						}
						if (randomPainFactor < 5f)
						{
							num3++;
						}
						if (randomPainFactor > num4)
						{
							num4 = randomPainFactor;
						}
					}
					stringBuilder.AppendLine("total: " + num5);
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"0: ",
						num,
						" (",
						((float)num / (float)num5).ToStringPercent(),
						")"
					}));
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"< 1: ",
						num2,
						" (",
						((float)num2 / (float)num5).ToStringPercent(),
						")"
					}));
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"< 5: ",
						num3,
						" (",
						((float)num3 / (float)num5).ToStringPercent(),
						")"
					}));
					stringBuilder.AppendLine("max: " + num4);
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("HediffGiver_Birthday chance at age", delegate
				{
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					foreach (HediffGiverSetDef current in pawn.RaceProps.hediffGiverSets)
					{
						foreach (HediffGiver_Birthday current2 in current.hediffGivers.OfType<HediffGiver_Birthday>())
						{
							HediffGiver_Birthday hLocal = current2;
							FloatMenuOption item = new FloatMenuOption(current.defName + " - " + current2.hediff.defName, delegate
							{
								StringBuilder stringBuilder = new StringBuilder();
								stringBuilder.AppendLine("% of pawns which will have at least 1 " + hLocal.hediff.label + " at age X:");
								stringBuilder.AppendLine();
								int num = 1;
								while ((float)num < pawn.RaceProps.lifeExpectancy + 20f)
								{
									stringBuilder.AppendLine(num + ": " + hLocal.DebugChanceToHaveAtAge(pawn, num).ToStringPercent());
									num++;
								}
								Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
							}, MenuOptionPriority.Medium, null, null, 0f, null);
							list2.Add(item);
						}
					}
					Find.WindowStack.Add(new FloatMenu(list2));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				list.Add(new FloatMenuOption("HediffGiver_Birthday count at age", delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Average hediffs count (from HediffGiver_Birthday) at age X:");
					stringBuilder.AppendLine();
					int num = 1;
					while ((float)num < pawn.RaceProps.lifeExpectancy + 20f)
					{
						float num2 = 0f;
						foreach (HediffGiverSetDef current in pawn.RaceProps.hediffGiverSets)
						{
							foreach (HediffGiver_Birthday current2 in current.hediffGivers.OfType<HediffGiver_Birthday>())
							{
								num2 += current2.DebugChanceToHaveAtAge(pawn, num);
							}
						}
						stringBuilder.AppendLine(num + ": " + num2.ToStringDecimalIfSmall());
						num++;
					}
					Find.WindowStack.Add(new Dialog_Message(stringBuilder.ToString(), null));
				}, MenuOptionPriority.Medium, null, null, 0f, null));
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public static Pair<string, Color> GetEfficiencyLabel(Pawn pawn, PawnCapacityDef activity)
		{
			float efficiency = pawn.health.capacities.GetEfficiency(activity);
			string first = string.Empty;
			Color second = Color.white;
			if (efficiency < 0.1f)
			{
				first = "None".Translate();
				second = HealthUtility.DarkRedColor;
			}
			else if (efficiency < 0.4f)
			{
				first = "VeryPoor".Translate();
				second = HealthCardUtility.VeryPoorColor;
			}
			else if (efficiency < 0.7f)
			{
				first = "Poor".Translate();
				second = HealthCardUtility.PoorColor;
			}
			else if (efficiency < 1f && !Mathf.Approximately(efficiency, 1f))
			{
				first = "Weakened".Translate();
				second = HealthCardUtility.WeakenedColor;
			}
			else if (Mathf.Approximately(efficiency, 1f))
			{
				first = "GoodCondition".Translate();
				second = HealthUtility.GoodConditionColor;
			}
			else
			{
				first = "Enhanced".Translate();
				second = HealthCardUtility.EnhancedColor;
			}
			return new Pair<string, Color>(first, second);
		}

		public static Pair<string, Color> GetPainLabel(Pawn pawn)
		{
			float pain = pawn.health.hediffSet.Pain;
			string first = string.Empty;
			Color second = Color.white;
			if (Mathf.Approximately(pain, 0f))
			{
				first = "NoPain".Translate();
				second = HealthUtility.GoodConditionColor;
			}
			else if (pain < 0.15f)
			{
				first = "LittlePain".Translate();
				second = Color.gray;
			}
			else if (pain < 0.4f)
			{
				first = "MediumPain".Translate();
				second = HealthCardUtility.MediumPainColor;
			}
			else if (pain < 0.8f)
			{
				first = "SeverePain".Translate();
				second = HealthCardUtility.SeverePainColor;
			}
			else
			{
				first = "ExtremePain".Translate();
				second = HealthUtility.DarkRedColor;
			}
			return new Pair<string, Color>(first, second);
		}
	}
}
