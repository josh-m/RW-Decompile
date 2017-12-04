using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FloatMenuMakerMap
	{
		public static Pawn makingFor;

		private static bool CanTakeOrder(Pawn pawn)
		{
			return pawn.IsColonistPlayerControlled;
		}

		public static void TryMakeFloatMenu(Pawn pawn)
		{
			if (!FloatMenuMakerMap.CanTakeOrder(pawn))
			{
				return;
			}
			if (pawn.Downed)
			{
				Messages.Message("IsIncapped".Translate(new object[]
				{
					pawn.LabelCap
				}), pawn, MessageTypeDefOf.RejectInput);
				return;
			}
			if (pawn.Map != Find.VisibleMap)
			{
				return;
			}
			List<FloatMenuOption> list = FloatMenuMakerMap.ChoicesAtFor(UI.MouseMapPosition(), pawn);
			if (list.Count == 0)
			{
				return;
			}
			if (list.Count == 1 && list[0].autoTakeable)
			{
				list[0].Chosen(true);
				return;
			}
			FloatMenuMap floatMenuMap = new FloatMenuMap(list, pawn.LabelCap, UI.MouseMapPosition());
			floatMenuMap.givesColonistOrders = true;
			Find.WindowStack.Add(floatMenuMap);
		}

		public static List<FloatMenuOption> ChoicesAtFor(Vector3 clickPos, Pawn pawn)
		{
			IntVec3 intVec = IntVec3.FromVector3(clickPos);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (!intVec.InBounds(pawn.Map) || !FloatMenuMakerMap.CanTakeOrder(pawn))
			{
				return list;
			}
			if (pawn.Map != Find.VisibleMap)
			{
				return list;
			}
			FloatMenuMakerMap.makingFor = pawn;
			try
			{
				if (intVec.Fogged(pawn.Map))
				{
					FloatMenuOption floatMenuOption = FloatMenuMakerMap.GotoLocationOption(intVec, pawn);
					if (floatMenuOption != null && !floatMenuOption.Disabled)
					{
						list.Add(floatMenuOption);
					}
				}
				else
				{
					if (pawn.Drafted)
					{
						FloatMenuMakerMap.AddDraftedOrders(clickPos, pawn, list);
					}
					if (pawn.RaceProps.Humanlike)
					{
						FloatMenuMakerMap.AddHumanlikeOrders(clickPos, pawn, list);
					}
					if (!pawn.Drafted)
					{
						FloatMenuMakerMap.AddUndraftedOrders(clickPos, pawn, list);
					}
					foreach (FloatMenuOption current in pawn.GetExtraFloatMenuOptionsFor(intVec))
					{
						list.Add(current);
					}
				}
			}
			finally
			{
				FloatMenuMakerMap.makingFor = null;
			}
			return list;
		}

		private static void AddDraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 clickCell = IntVec3.FromVector3(clickPos);
			foreach (LocalTargetInfo current in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true))
			{
				LocalTargetInfo attackTarg = current;
				if (pawn.equipment.Primary != null && !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.MeleeRange)
				{
					string str;
					Action rangedAct = FloatMenuUtility.GetRangedAttackAction(pawn, attackTarg, out str);
					string text = "FireAt".Translate(new object[]
					{
						attackTarg.Thing.Label
					});
					FloatMenuOption floatMenuOption = new FloatMenuOption(MenuOptionPriority.High);
					if (rangedAct == null)
					{
						text = text + " (" + str + ")";
					}
					else
					{
						floatMenuOption.autoTakeable = true;
						floatMenuOption.action = delegate
						{
							MoteMaker.MakeStaticMote(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, ThingDefOf.Mote_FeedbackAttack, 1f);
							rangedAct();
						};
					}
					floatMenuOption.Label = text;
					opts.Add(floatMenuOption);
				}
				string str2;
				Action meleeAct = FloatMenuUtility.GetMeleeAttackAction(pawn, attackTarg, out str2);
				Pawn pawn2 = attackTarg.Thing as Pawn;
				string text2;
				if (pawn2 != null && pawn2.Downed)
				{
					text2 = "MeleeAttackToDeath".Translate(new object[]
					{
						attackTarg.Thing.Label
					});
				}
				else
				{
					text2 = "MeleeAttack".Translate(new object[]
					{
						attackTarg.Thing.Label
					});
				}
				MenuOptionPriority menuOptionPriority = (!attackTarg.HasThing || !pawn.HostileTo(attackTarg.Thing)) ? MenuOptionPriority.VeryLow : MenuOptionPriority.AttackEnemy;
				string label = string.Empty;
				Action action = null;
				MenuOptionPriority priority = menuOptionPriority;
				Thing thing = attackTarg.Thing;
				FloatMenuOption floatMenuOption2 = new FloatMenuOption(label, action, priority, null, thing, 0f, null, null);
				if (meleeAct == null)
				{
					text2 = text2 + " (" + str2 + ")";
				}
				else
				{
					floatMenuOption2.action = delegate
					{
						MoteMaker.MakeStaticMote(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, ThingDefOf.Mote_FeedbackAttack, 1f);
						meleeAct();
					};
				}
				floatMenuOption2.Label = text2;
				opts.Add(floatMenuOption2);
			}
			if (pawn.RaceProps.Humanlike && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				foreach (LocalTargetInfo current2 in GenUI.TargetsAt(clickPos, TargetingParameters.ForArrest(pawn), true))
				{
					LocalTargetInfo dest = current2;
					if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						opts.Add(new FloatMenuOption("CannotArrest".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
					else
					{
						Pawn pTarg = (Pawn)dest.Thing;
						Action action2 = delegate
						{
							Building_Bed building_Bed = RestUtility.FindBedFor(pTarg, pawn, true, false, false);
							if (building_Bed == null)
							{
								building_Bed = RestUtility.FindBedFor(pTarg, pawn, true, false, true);
							}
							if (building_Bed == null)
							{
								Messages.Message("CannotArrest".Translate() + ": " + "NoPrisonerBed".Translate(), pTarg, MessageTypeDefOf.RejectInput);
								return;
							}
							Job job = new Job(JobDefOf.Arrest, pTarg, building_Bed);
							job.count = 1;
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
							TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.ArrestingCreatesEnemies);
						};
						string label = "TryToArrest".Translate(new object[]
						{
							dest.Thing.LabelCap
						});
						Action action = action2;
						MenuOptionPriority priority = MenuOptionPriority.High;
						Thing thing = dest.Thing;
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority, null, thing, 0f, null, null), pawn, pTarg, "ReservedBy"));
					}
				}
			}
			FloatMenuOption floatMenuOption3 = FloatMenuMakerMap.GotoLocationOption(clickCell, pawn);
			if (floatMenuOption3 != null)
			{
				opts.Add(floatMenuOption3);
			}
		}

		private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 c = IntVec3.FromVector3(clickPos);
			foreach (Thing current in c.GetThingList(pawn.Map))
			{
				Thing t = current;
				if (t.def.ingestible != null && pawn.RaceProps.CanEverEat(t) && t.IngestibleNow)
				{
					string text;
					if (t.def.ingestible.ingestCommandString.NullOrEmpty())
					{
						text = "ConsumeThing".Translate(new object[]
						{
							t.LabelShort
						});
					}
					else
					{
						text = string.Format(t.def.ingestible.ingestCommandString, t.LabelShort);
					}
					if (!t.IsSociallyProper(pawn))
					{
						text = text + " (" + "ReservedForPrisoners".Translate() + ")";
					}
					FloatMenuOption item5;
					if (t.def.IsNonMedicalDrug && pawn.IsTeetotaler())
					{
						item5 = new FloatMenuOption(text + " (" + TraitDefOf.DrugDesire.DataAtDegree(-1).label + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						item5 = new FloatMenuOption(text + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else
					{
						MenuOptionPriority priority = (!(t is Corpse)) ? MenuOptionPriority.Default : MenuOptionPriority.Low;
						item5 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
						{
							t.SetForbidden(false, true);
							Job job = new Job(JobDefOf.Ingest, t);
							job.count = FoodUtility.WillIngestStackCountOf(pawn, t.def);
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}, priority, null, null, 0f, null, null), pawn, t, "ReservedBy");
					}
					opts.Add(item5);
				}
			}
			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				foreach (LocalTargetInfo current2 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
				{
					Pawn victim = (Pawn)current2.Thing;
					if (!victim.InBed() && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true))
					{
						if (!victim.IsPrisonerOfColony && !victim.InMentalState && (victim.Faction == Faction.OfPlayer || victim.Faction == null || !victim.Faction.HostileTo(Faction.OfPlayer)))
						{
							string label = "Rescue".Translate(new object[]
							{
								victim.LabelCap
							});
							Action action = delegate
							{
								Building_Bed building_Bed = RestUtility.FindBedFor(victim, pawn, false, false, false);
								if (building_Bed == null)
								{
									building_Bed = RestUtility.FindBedFor(victim, pawn, false, false, true);
								}
								if (building_Bed == null)
								{
									string str2;
									if (victim.RaceProps.Animal)
									{
										str2 = "NoAnimalBed".Translate();
									}
									else
									{
										str2 = "NoNonPrisonerBed".Translate();
									}
									Messages.Message("CannotRescue".Translate() + ": " + str2, victim, MessageTypeDefOf.RejectInput);
									return;
								}
								Job job = new Job(JobDefOf.Rescue, victim, building_Bed);
								job.count = 1;
								pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
							};
							MenuOptionPriority priority2 = MenuOptionPriority.RescueOrCapture;
							Pawn victim2 = victim;
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority2, null, victim2, 0f, null, null), pawn, victim, "ReservedBy"));
						}
						if (!victim.NonHumanlikeOrWildMan() && (victim.InMentalState || victim.Faction != Faction.OfPlayer || (victim.Downed && (victim.guilt.IsGuilty || victim.IsPrisonerOfColony))))
						{
							string label = "Capture".Translate(new object[]
							{
								victim.LabelCap
							});
							Action action = delegate
							{
								Building_Bed building_Bed = RestUtility.FindBedFor(victim, pawn, true, false, false);
								if (building_Bed == null)
								{
									building_Bed = RestUtility.FindBedFor(victim, pawn, true, false, true);
								}
								if (building_Bed == null)
								{
									Messages.Message("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate(), victim, MessageTypeDefOf.RejectInput);
									return;
								}
								Job job = new Job(JobDefOf.Capture, victim, building_Bed);
								job.count = 1;
								pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
							};
							MenuOptionPriority priority2 = MenuOptionPriority.RescueOrCapture;
							Pawn victim2 = victim;
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority2, null, victim2, 0f, null, null), pawn, victim, "ReservedBy"));
						}
					}
				}
				foreach (LocalTargetInfo current3 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
				{
					LocalTargetInfo localTargetInfo = current3;
					Pawn victim = (Pawn)localTargetInfo.Thing;
					if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn, true) != null)
					{
						string text2 = "CarryToCryptosleepCasket".Translate(new object[]
						{
							localTargetInfo.Thing.LabelCap
						});
						JobDef jDef = JobDefOf.CarryToCryptosleepCasket;
						Action action2 = delegate
						{
							Building_CryptosleepCasket building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn, false);
							if (building_CryptosleepCasket == null)
							{
								building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim, pawn, true);
							}
							if (building_CryptosleepCasket == null)
							{
								Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), victim, MessageTypeDefOf.RejectInput);
								return;
							}
							Job job = new Job(jDef, victim, building_CryptosleepCasket);
							job.count = 1;
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						};
						string label = text2;
						Action action = action2;
						Pawn victim2 = victim;
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, victim2, 0f, null, null), pawn, victim, "ReservedBy"));
					}
				}
			}
			foreach (LocalTargetInfo current4 in GenUI.TargetsAt(clickPos, TargetingParameters.ForStrip(pawn), true))
			{
				LocalTargetInfo stripTarg = current4;
				FloatMenuOption item2;
				if (!pawn.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					item2 = new FloatMenuOption("CannotStrip".Translate(new object[]
					{
						stripTarg.Thing.LabelCap
					}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
				}
				else
				{
					item2 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Strip".Translate(new object[]
					{
						stripTarg.Thing.LabelCap
					}), delegate
					{
						stripTarg.Thing.SetForbidden(false, false);
						pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Strip, stripTarg), JobTag.Misc);
					}, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, stripTarg, "ReservedBy");
				}
				opts.Add(item2);
			}
			if (pawn.equipment != null)
			{
				ThingWithComps equipment = null;
				List<Thing> thingList = c.GetThingList(pawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].TryGetComp<CompEquippable>() != null)
					{
						equipment = (ThingWithComps)thingList[i];
						break;
					}
				}
				if (equipment != null)
				{
					string labelShort = equipment.LabelShort;
					FloatMenuOption item3;
					if (equipment.def.IsWeapon && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
					{
						item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
						{
							labelShort
						}) + " (" + "IsIncapableOfViolenceLower".Translate(new object[]
						{
							pawn.LabelShort
						}) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
						{
							labelShort
						}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
						{
							labelShort
						}) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else
					{
						string text3 = "Equip".Translate(new object[]
						{
							labelShort
						});
						if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
						{
							text3 = text3 + " " + "EquipWarningBrawler".Translate();
						}
						item3 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text3, delegate
						{
							equipment.SetForbidden(false, true);
							pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Equip, equipment), JobTag.Misc);
							MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f);
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
						}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
					}
					opts.Add(item3);
				}
			}
			if (pawn.apparel != null)
			{
				Apparel apparel = pawn.Map.thingGrid.ThingAt<Apparel>(c);
				if (apparel != null)
				{
					FloatMenuOption item4;
					if (!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						item4 = new FloatMenuOption("CannotWear".Translate(new object[]
						{
							apparel.Label
						}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
					{
						item4 = new FloatMenuOption("CannotWear".Translate(new object[]
						{
							apparel.Label
						}) + " (" + "CannotWearBecauseOfMissingBodyParts".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					else
					{
						item4 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ForceWear".Translate(new object[]
						{
							apparel.LabelShort
						}), delegate
						{
							apparel.SetForbidden(false, true);
							Job job = new Job(JobDefOf.Wear, apparel);
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, apparel, "ReservedBy");
					}
					opts.Add(item4);
				}
			}
			if (!pawn.Map.IsPlayerHome)
			{
				Thing item = c.GetFirstItem(pawn.Map);
				if (item != null && item.def.EverHaulable)
				{
					if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
						{
							item.Label
						}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
					else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, 1))
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
						{
							item.Label
						}) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
					else if (item.stackCount == 1)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUp".Translate(new object[]
						{
							item.Label
						}), delegate
						{
							item.SetForbidden(false, false);
							Job job = new Job(JobDefOf.TakeInventory, item);
							job.count = 1;
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
					}
					else
					{
						if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, item.stackCount))
						{
							opts.Add(new FloatMenuOption("CannotPickUpAll".Translate(new object[]
							{
								item.Label
							}) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
						}
						else
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(new object[]
							{
								item.Label
							}), delegate
							{
								item.SetForbidden(false, false);
								Job job = new Job(JobDefOf.TakeInventory, item);
								job.count = item.stackCount;
								pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
							}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
						}
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpSome".Translate(new object[]
						{
							item.Label
						}), delegate
						{
							int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(pawn, item), item.stackCount);
							Dialog_Slider window = new Dialog_Slider("PickUpCount".Translate(new object[]
							{
								item.LabelShort
							}), 1, to, delegate(int count)
							{
								item.SetForbidden(false, false);
								Job job = new Job(JobDefOf.TakeInventory, item);
								job.count = count;
								pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
							}, -2147483648);
							Find.WindowStack.Add(window);
						}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
					}
				}
			}
			if (!pawn.Map.IsPlayerHome)
			{
				Thing item = c.GetFirstItem(pawn.Map);
				if (item != null && item.def.EverHaulable)
				{
					Pawn bestPackAnimal = GiveToPackAnimalUtility.PackAnimalWithTheMostFreeSpace(pawn.Map, pawn.Faction);
					if (bestPackAnimal != null)
					{
						if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
						{
							opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(new object[]
							{
								item.Label
							}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
						}
						else if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, 1))
						{
							opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(new object[]
							{
								item.Label
							}) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
						}
						else if (item.stackCount == 1)
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimal".Translate(new object[]
							{
								item.Label
							}), delegate
							{
								item.SetForbidden(false, false);
								Job job = new Job(JobDefOf.GiveToPackAnimal, item);
								job.count = 1;
								pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
							}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
						}
						else
						{
							if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, item.stackCount))
							{
								opts.Add(new FloatMenuOption("CannotGiveToPackAnimalAll".Translate(new object[]
								{
									item.Label
								}) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
							}
							else
							{
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalAll".Translate(new object[]
								{
									item.Label
								}), delegate
								{
									item.SetForbidden(false, false);
									Job job = new Job(JobDefOf.GiveToPackAnimal, item);
									job.count = item.stackCount;
									pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
							}
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalSome".Translate(new object[]
							{
								item.Label
							}), delegate
							{
								int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(bestPackAnimal, item), item.stackCount);
								Dialog_Slider window = new Dialog_Slider("GiveToPackAnimalCount".Translate(new object[]
								{
									item.LabelShort
								}), 1, to, delegate(int count)
								{
									item.SetForbidden(false, false);
									Job job = new Job(JobDefOf.GiveToPackAnimal, item);
									job.count = count;
									pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								}, -2147483648);
								Find.WindowStack.Add(window);
							}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
						}
					}
				}
			}
			if (!pawn.Map.IsPlayerHome && pawn.Map.exitMapGrid.MapUsesExitGrid)
			{
				foreach (LocalTargetInfo current5 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
				{
					Pawn p = (Pawn)current5.Thing;
					if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
					{
						if (!pawn.CanReach(p, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
						{
							opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(new object[]
							{
								p.Label
							}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
						}
						else
						{
							IntVec3 exitSpot;
							if (!RCellFinder.TryFindBestExitSpot(pawn, out exitSpot, TraverseMode.ByPawn))
							{
								opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(new object[]
								{
									p.Label
								}) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
							}
							else
							{
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToExit".Translate(new object[]
								{
									p.Label
								}), delegate
								{
									Job job = new Job(JobDefOf.CarryDownedPawnToExit, p, exitSpot);
									job.count = 1;
									pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								}, MenuOptionPriority.High, null, null, 0f, null, null), pawn, current5, "ReservedBy"));
							}
						}
					}
				}
			}
			if (pawn.equipment != null && pawn.equipment.Primary != null && GenUI.TargetsAt(clickPos, TargetingParameters.ForSelf(pawn), true).Any<LocalTargetInfo>())
			{
				Action action3 = delegate
				{
					pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, pawn.equipment.Primary), JobTag.Misc);
				};
				opts.Add(new FloatMenuOption("Drop".Translate(new object[]
				{
					pawn.equipment.Primary.Label
				}), action3, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			foreach (LocalTargetInfo current6 in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), true))
			{
				LocalTargetInfo dest = current6;
				if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					opts.Add(new FloatMenuOption("CannotTrade".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				else if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
				{
					opts.Add(new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(new object[]
					{
						SkillDefOf.Social.LabelCap
					}), null, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				else
				{
					Pawn pTarg = (Pawn)dest.Thing;
					Action action4 = delegate
					{
						Job job = new Job(JobDefOf.TradeWithPawn, pTarg);
						job.playerForced = true;
						pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InteractingWithTraders, KnowledgeAmount.Total);
					};
					string str = string.Empty;
					if (pTarg.Faction != null)
					{
						str = " (" + pTarg.Faction.Name + ")";
					}
					string label = "TradeWith".Translate(new object[]
					{
						pTarg.LabelShort + ", " + pTarg.TraderKind.label
					}) + str;
					Action action = action4;
					MenuOptionPriority priority2 = MenuOptionPriority.InitiateSocial;
					Thing thing = dest.Thing;
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, priority2, null, thing, 0f, null, null), pawn, pTarg, "ReservedBy"));
				}
			}
			foreach (Thing current7 in pawn.Map.thingGrid.ThingsAt(c))
			{
				foreach (FloatMenuOption current8 in current7.GetFloatMenuOptions(pawn))
				{
					opts.Add(current8);
				}
			}
		}

		private static void AddUndraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 clickCell = IntVec3.FromVector3(clickPos);
			bool flag = false;
			bool flag2 = false;
			foreach (Thing current in pawn.Map.thingGrid.ThingsAt(clickCell))
			{
				flag2 = true;
				if (pawn.CanReach(current, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
				{
					flag = true;
					break;
				}
			}
			if (flag2 && !flag)
			{
				opts.Add(new FloatMenuOption("(" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
				return;
			}
			JobGiver_Work jobGiver_Work = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>();
			if (jobGiver_Work != null)
			{
				foreach (Thing current2 in pawn.Map.thingGrid.ThingsAt(clickCell))
				{
					foreach (WorkTypeDef current3 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
					{
						for (int i = 0; i < current3.workGiversByPriority.Count; i++)
						{
							WorkGiver_Scanner workGiver_Scanner = current3.workGiversByPriority[i].Worker as WorkGiver_Scanner;
							if (workGiver_Scanner != null && workGiver_Scanner.def.directOrderable && !workGiver_Scanner.ShouldSkip(pawn))
							{
								JobFailReason.Clear();
								if (workGiver_Scanner.PotentialWorkThingRequest.Accepts(current2) || (workGiver_Scanner.PotentialWorkThingsGlobal(pawn) != null && workGiver_Scanner.PotentialWorkThingsGlobal(pawn).Contains(current2)))
								{
									string label = null;
									Action action = null;
									PawnCapacityDef pawnCapacityDef = workGiver_Scanner.MissingRequiredCapacity(pawn);
									if (pawnCapacityDef != null)
									{
										label = "CannotMissingHealthActivities".Translate(new object[]
										{
											pawnCapacityDef.label
										});
									}
									else
									{
										Job job;
										if (!workGiver_Scanner.HasJobOnThing(pawn, current2, true))
										{
											job = null;
										}
										else
										{
											job = workGiver_Scanner.JobOnThing(pawn, current2, true);
										}
										if (job == null)
										{
											if (!JobFailReason.HaveReason)
											{
												goto IL_5DB;
											}
											label = "CannotGenericWork".Translate(new object[]
											{
												workGiver_Scanner.def.verb,
												current2.LabelShort
											}) + " (" + JobFailReason.Reason + ")";
										}
										else
										{
											WorkTypeDef workType = workGiver_Scanner.def.workType;
											if (pawn.story != null && pawn.story.WorkTagIsDisabled(workGiver_Scanner.def.workTags))
											{
												label = "CannotPrioritizeWorkGiverDisabled".Translate(new object[]
												{
													workGiver_Scanner.def.label
												});
											}
											else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job))
											{
												label = "CannotGenericAlreadyAm".Translate(new object[]
												{
													workType.gerundLabel,
													current2.LabelShort
												});
											}
											else if (pawn.workSettings.GetPriority(workType) == 0)
											{
												if (pawn.story.WorkTypeIsDisabled(workType))
												{
													label = "CannotPrioritizeWorkTypeDisabled".Translate(new object[]
													{
														workType.gerundLabel
													});
												}
												else if ("CannotPrioritizeNotAssignedToWorkType".CanTranslate())
												{
													label = "CannotPrioritizeNotAssignedToWorkType".Translate(new object[]
													{
														workType.gerundLabel
													});
												}
												else
												{
													label = "CannotPrioritizeIsNotA".Translate(new object[]
													{
														pawn.NameStringShort,
														workType.pawnLabel
													});
												}
											}
											else if (job.def == JobDefOf.Research && current2 is Building_ResearchBench)
											{
												label = "CannotPrioritizeResearch".Translate();
											}
											else if (current2.IsForbidden(pawn))
											{
												if (!current2.Position.InAllowedArea(pawn))
												{
													label = "CannotPrioritizeForbiddenOutsideAllowedArea".Translate(new object[]
													{
														current2.Label
													});
												}
												else
												{
													label = "CannotPrioritizeForbidden".Translate(new object[]
													{
														current2.Label
													});
												}
											}
											else if (!pawn.CanReach(current2, workGiver_Scanner.PathEndMode, Danger.Deadly, false, TraverseMode.ByPawn))
											{
												label = (current2.Label + ": " + "NoPath".Translate()).CapitalizeFirst();
											}
											else
											{
												label = "PrioritizeGeneric".Translate(new object[]
												{
													workGiver_Scanner.def.gerund,
													current2.Label
												});
												Job localJob = job;
												WorkGiver_Scanner localScanner = workGiver_Scanner;
												action = delegate
												{
													pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, clickCell);
												};
											}
										}
									}
									if (!opts.Any((FloatMenuOption op) => op.Label == label.TrimEnd(new char[0])))
									{
										opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, current2, "ReservedBy"));
									}
								}
							}
							IL_5DB:;
						}
					}
				}
				foreach (WorkTypeDef current4 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
				{
					for (int j = 0; j < current4.workGiversByPriority.Count; j++)
					{
						WorkGiver_Scanner workGiver_Scanner2 = current4.workGiversByPriority[j].Worker as WorkGiver_Scanner;
						if (workGiver_Scanner2 != null && workGiver_Scanner2.def.directOrderable && !workGiver_Scanner2.ShouldSkip(pawn))
						{
							JobFailReason.Clear();
							if (workGiver_Scanner2.PotentialWorkCellsGlobal(pawn).Contains(clickCell))
							{
								Action action2 = null;
								string label = null;
								PawnCapacityDef pawnCapacityDef2 = workGiver_Scanner2.MissingRequiredCapacity(pawn);
								if (pawnCapacityDef2 != null)
								{
									label = "CannotMissingHealthActivities".Translate(new object[]
									{
										pawnCapacityDef2.label
									});
								}
								else
								{
									Job job2;
									if (!workGiver_Scanner2.HasJobOnCell(pawn, clickCell))
									{
										job2 = null;
									}
									else
									{
										job2 = workGiver_Scanner2.JobOnCell(pawn, clickCell);
									}
									if (job2 == null)
									{
										if (!JobFailReason.HaveReason)
										{
											goto IL_9E5;
										}
										label = "CannotGenericWork".Translate(new object[]
										{
											workGiver_Scanner2.def.verb,
											"AreaLower".Translate()
										}) + " (" + JobFailReason.Reason + ")";
									}
									else
									{
										WorkTypeDef workType2 = workGiver_Scanner2.def.workType;
										if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job2))
										{
											label = "CannotGenericAlreadyAm".Translate(new object[]
											{
												workType2.gerundLabel,
												"AreaLower".Translate()
											});
										}
										else if (pawn.workSettings.GetPriority(workType2) == 0)
										{
											if (pawn.story.WorkTypeIsDisabled(workType2))
											{
												label = "CannotPrioritizeWorkTypeDisabled".Translate(new object[]
												{
													workType2.gerundLabel
												});
											}
											else if ("CannotPrioritizeNotAssignedToWorkType".CanTranslate())
											{
												label = "CannotPrioritizeNotAssignedToWorkType".Translate(new object[]
												{
													workType2.gerundLabel
												});
											}
											else
											{
												label = "CannotPrioritizeIsNotA".Translate(new object[]
												{
													pawn.NameStringShort,
													workType2.pawnLabel
												});
											}
										}
										else if (!pawn.CanReach(clickCell, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
										{
											label = "AreaLower".Translate().CapitalizeFirst() + ": " + "NoPath".Translate();
										}
										else
										{
											label = "PrioritizeGeneric".Translate(new object[]
											{
												workGiver_Scanner2.def.gerund,
												"AreaLower".Translate()
											});
											Job localJob = job2;
											WorkGiver_Scanner localScanner = workGiver_Scanner2;
											action2 = delegate
											{
												pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, clickCell);
											};
										}
									}
								}
								if (!opts.Any((FloatMenuOption op) => op.Label == label.TrimEnd(new char[0])))
								{
									opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, clickCell, "ReservedBy"));
								}
							}
						}
						IL_9E5:;
					}
				}
			}
		}

		private static FloatMenuOption GotoLocationOption(IntVec3 clickCell, Pawn pawn)
		{
			int num = GenRadial.NumCellsInRadius(2.9f);
			int i = 0;
			IntVec3 curLoc;
			while (i < num)
			{
				curLoc = GenRadial.RadialPattern[i] + clickCell;
				if (curLoc.Standable(pawn.Map))
				{
					if (!(curLoc != pawn.Position))
					{
						return null;
					}
					if (!pawn.CanReach(curLoc, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
					{
						return new FloatMenuOption("CannotGoNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
					}
					Action action = delegate
					{
						IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(curLoc, pawn);
						Job job = new Job(JobDefOf.Goto, intVec);
						if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
						{
							job.exitMapOnArrival = true;
						}
						if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
						{
							MoteMaker.MakeStaticMote(intVec, pawn.Map, ThingDefOf.Mote_FeedbackGoto, 1f);
						}
					};
					return new FloatMenuOption("GoHere".Translate(), action, MenuOptionPriority.GoHere, null, null, 0f, null, null)
					{
						autoTakeable = true
					};
				}
				else
				{
					i++;
				}
			}
			return null;
		}
	}
}
