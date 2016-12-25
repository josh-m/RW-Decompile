using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_FormCaravan : Window
	{
		private enum Tab
		{
			Pawns,
			Items,
			Config
		}

		private const float TitleRectHeight = 40f;

		private const float BottomAreaHeight = 55f;

		private const float ExitDirectionTitleHeight = 30f;

		private const float MaxDaysWorthOfFoodToShowWarningDialog = 5f;

		private Map map;

		private bool reform;

		private List<TransferableOneWay> transferables;

		private TransferableOneWayWidget pawnsTransfer;

		private TransferableOneWayWidget itemsTransfer;

		private Dialog_FormCaravan.Tab tab;

		private float lastMassFlashTime = -9999f;

		private int startingTile = -1;

		private bool massUsageDirty = true;

		private float cachedMassUsage;

		private bool massCapacityDirty = true;

		private float cachedMassCapacity;

		private bool daysWorthOfFoodDirty = true;

		private float cachedDaysWorthOfFood;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		private readonly Vector2 ExitDirectionRadioSize = new Vector2(250f, 30f);

		private static List<TabRecord> tabsList = new List<TabRecord>();

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(1024f, (float)UI.screenHeight);
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		private bool AutoStripCorpses
		{
			get
			{
				return this.reform;
			}
		}

		private float MassUsage
		{
			get
			{
				if (this.massUsageDirty)
				{
					this.massUsageDirty = false;
					bool autoStripCorpses = this.AutoStripCorpses;
					this.cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(this.transferables, false, false, autoStripCorpses);
				}
				return this.cachedMassUsage;
			}
		}

		private float MassCapacity
		{
			get
			{
				if (this.massCapacityDirty)
				{
					this.massCapacityDirty = false;
					this.cachedMassCapacity = CollectionsMassCalculator.CapacityTransferables(this.transferables);
				}
				return this.cachedMassCapacity;
			}
		}

		private float DaysWorthOfFood
		{
			get
			{
				if (this.daysWorthOfFoodDirty)
				{
					this.daysWorthOfFoodDirty = false;
					this.cachedDaysWorthOfFood = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables);
				}
				return this.cachedDaysWorthOfFood;
			}
		}

		public Dialog_FormCaravan(Map map, bool reform = false)
		{
			this.map = map;
			this.reform = reform;
			this.closeOnEscapeKey = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			this.CalculateAndRecacheTransferables();
			List<int> list = CaravanExitMapUtility.AvailableExitTilesAt(this.map);
			if (list.Any<int>())
			{
				this.startingTile = list.RandomElement<int>();
			}
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.FormCaravan, KnowledgeAmount.Total);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, inRect.width, 40f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, ((!this.reform) ? "FormCaravan" : "ReformCaravan").Translate());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Dialog_FormCaravan.tabsList.Clear();
			Dialog_FormCaravan.tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
			{
				this.tab = Dialog_FormCaravan.Tab.Pawns;
			}, this.tab == Dialog_FormCaravan.Tab.Pawns));
			Dialog_FormCaravan.tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
			{
				this.tab = Dialog_FormCaravan.Tab.Items;
			}, this.tab == Dialog_FormCaravan.Tab.Items));
			if (!this.reform)
			{
				Dialog_FormCaravan.tabsList.Add(new TabRecord("CaravanConfigTab".Translate(), delegate
				{
					this.tab = Dialog_FormCaravan.Tab.Config;
				}, this.tab == Dialog_FormCaravan.Tab.Config));
			}
			inRect.yMin += 72f;
			Widgets.DrawMenuSection(inRect, true);
			TabDrawer.DrawTabs(inRect, Dialog_FormCaravan.tabsList);
			inRect = inRect.ContractedBy(17f);
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			if (this.tab != Dialog_FormCaravan.Tab.Config)
			{
				Rect rect3 = rect2;
				rect3.xMin += rect2.width - this.pawnsTransfer.TotalNumbersColumnsWidths;
				rect3.y += 32f;
				TransferableUIUtility.DrawMassInfo(rect3, this.MassUsage, this.MassCapacity, "CaravanMassUsageTooltip".Translate(), this.lastMassFlashTime, true);
				CaravanUIUtility.DrawDaysWorthOfFoodInfo(new Rect(rect3.x, rect3.y + 22f, rect3.width, rect3.height), this.DaysWorthOfFood, true);
			}
			this.DoBottomButtons(rect2);
			Rect inRect2 = rect2;
			inRect2.yMax -= 59f;
			bool flag = false;
			switch (this.tab)
			{
			case Dialog_FormCaravan.Tab.Pawns:
				this.pawnsTransfer.OnGUI(inRect2, out flag);
				break;
			case Dialog_FormCaravan.Tab.Items:
				this.itemsTransfer.OnGUI(inRect2, out flag);
				break;
			case Dialog_FormCaravan.Tab.Config:
				this.DrawConfig(rect2);
				break;
			}
			if (flag)
			{
				this.CountToTransferChanged();
			}
			GUI.EndGroup();
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		private void AddToTransferables(Thing t, bool setToTransferMax = false)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.transferables);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				this.transferables.Add(transferableOneWay);
			}
			transferableOneWay.things.Add(t);
			if (setToTransferMax)
			{
				transferableOneWay.countToTransfer = transferableOneWay.MaxCount;
			}
		}

		private void DoBottomButtons(Rect rect)
		{
			Rect rect2 = new Rect(rect.width / 2f - this.BottomButtonSize.x / 2f, rect.height - 55f, this.BottomButtonSize.x, this.BottomButtonSize.y);
			if (Widgets.ButtonText(rect2, "AcceptButton".Translate(), true, false, true))
			{
				if (this.reform)
				{
					if (this.TryReformCaravan())
					{
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
						this.Close(false);
					}
				}
				else
				{
					float daysWorthOfFood = this.DaysWorthOfFood;
					if (daysWorthOfFood < 5f)
					{
						if (this.CheckForErrors(TransferableUtility.GetPawnsFromTransferables(this.transferables)))
						{
							string text = (daysWorthOfFood >= 0.1f) ? "DaysWorthOfFoodWarningDialog".Translate(new object[]
							{
								daysWorthOfFood.ToString("0.#")
							}) : "DaysWorthOfFoodWarningDialog_NoFood".Translate();
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
							{
								if (this.TryFormAndSendCaravan())
								{
									this.Close(false);
								}
							}, false, null));
						}
					}
					else if (this.TryFormAndSendCaravan())
					{
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
						this.Close(false);
					}
				}
			}
			Rect rect3 = new Rect(rect2.x - 10f - this.BottomButtonSize.x, rect2.y, this.BottomButtonSize.x, this.BottomButtonSize.y);
			if (Widgets.ButtonText(rect3, "ResetButton".Translate(), true, false, true))
			{
				SoundDefOf.TickLow.PlayOneShotOnCamera();
				this.CalculateAndRecacheTransferables();
			}
			Rect rect4 = new Rect(rect2.xMax + 10f, rect2.y, this.BottomButtonSize.x, this.BottomButtonSize.y);
			if (Widgets.ButtonText(rect4, "CancelButton".Translate(), true, false, true))
			{
				this.Close(true);
			}
			if (Prefs.DevMode)
			{
				float num = 200f;
				float num2 = this.BottomButtonSize.y / 2f;
				Rect rect5 = new Rect(rect.width - num, rect.height - 55f, num, num2);
				if (Widgets.ButtonText(rect5, "Dev: Send instantly", true, false, true) && this.DebugTryFormCaravanInstantly())
				{
					SoundDefOf.TickHigh.PlayOneShotOnCamera();
					this.Close(false);
				}
				Rect rect6 = new Rect(rect.width - num, rect.height - 55f + num2, num, num2);
				if (Widgets.ButtonText(rect6, "Dev: Select everything", true, false, true))
				{
					SoundDefOf.TickHigh.PlayOneShotOnCamera();
					this.SetToSendEverything();
				}
			}
		}

		private void CalculateAndRecacheTransferables()
		{
			this.transferables = new List<TransferableOneWay>();
			this.AddPawnsToTransferables();
			this.AddItemsToTransferables();
			string sourceLabel;
			if (this.reform && this.map.info.parent != null)
			{
				sourceLabel = this.map.info.parent.LabelCap;
			}
			else
			{
				sourceLabel = Faction.OfPlayer.Name;
			}
			CaravanUIUtility.CreateCaravanTransferableWidgets(this.transferables, out this.pawnsTransfer, out this.itemsTransfer, sourceLabel, WorldObjectDefOf.Caravan.LabelCap, "FormCaravanColonyThingCountTip".Translate(), false, () => this.MassCapacity - this.MassUsage, this.AutoStripCorpses);
			this.CountToTransferChanged();
		}

		private void DrawConfig(Rect rect)
		{
			Rect rect2 = new Rect(0f, rect.y, rect.width, 30f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect2, "ExitDirection".Translate());
			Text.Font = GameFont.Small;
			List<int> list = CaravanExitMapUtility.AvailableExitTilesAt(this.map);
			if (list.Any<int>())
			{
				for (int i = 0; i < list.Count; i++)
				{
					Direction8Way direction8WayFromTo = Find.WorldGrid.GetDirection8WayFromTo(this.map.Tile, list[i]);
					float y = rect.y + (float)i * this.ExitDirectionRadioSize.y + 30f + 4f;
					Rect rect3 = new Rect(rect.x, y, this.ExitDirectionRadioSize.x, this.ExitDirectionRadioSize.y);
					Vector2 vector = Find.WorldGrid.LongLatOf(list[i]);
					string labelText = "ExitDirectionRadioButtonLabel".Translate(new object[]
					{
						direction8WayFromTo.ToStringShort(),
						vector.y.ToStringLatitude(),
						vector.x.ToStringLongitude()
					});
					if (Widgets.RadioButtonLabeled(rect3, labelText, this.startingTile == list[i]))
					{
						this.startingTile = list[i];
					}
				}
			}
			else
			{
				GUI.color = Color.gray;
				Widgets.Label(new Rect(rect.x, rect.y + 30f + 4f, rect.width, 100f), "NoCaravanExitDirectionAvailable".Translate());
				GUI.color = Color.white;
			}
		}

		private bool DebugTryFormCaravanInstantly()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(this.transferables);
			if (!pawnsFromTransferables.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
			{
				Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), MessageSound.RejectInput);
				return false;
			}
			this.AddItemsFromTransferablesToRandomInventories(pawnsFromTransferables);
			int tile = this.startingTile;
			if (tile < 0)
			{
				tile = this.map.Tile;
			}
			CaravanFormingUtility.FormAndCreateCaravan(pawnsFromTransferables, Faction.OfPlayer, tile);
			return true;
		}

		private bool TryFormAndSendCaravan()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(this.transferables);
			if (!this.CheckForErrors(pawnsFromTransferables))
			{
				return false;
			}
			IntVec3 exitSpot;
			if (!this.TryFindExitSpot(pawnsFromTransferables, true, out exitSpot))
			{
				if (!this.TryFindExitSpot(pawnsFromTransferables, false, out exitSpot))
				{
					Messages.Message("CaravanCouldNotFindExitSpot".Translate(), MessageSound.RejectInput);
					return false;
				}
				Pawn t = pawnsFromTransferables.Find((Pawn x) => x.IsColonist && !x.CanReach(exitSpot, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn));
				Messages.Message("CaravanCouldNotFindReachableExitSpot".Translate(), t, MessageSound.Negative);
			}
			IntVec3 meetingPoint;
			if (!RCellFinder.TryFindRandomSpotJustOutsideColony(exitSpot, this.map, out meetingPoint))
			{
				Messages.Message("CaravanCouldNotFindExitSpot".Translate(), MessageSound.RejectInput);
				return false;
			}
			CaravanFormingUtility.StartFormingCaravan(pawnsFromTransferables, Faction.OfPlayer, this.transferables, meetingPoint, exitSpot, this.startingTile);
			Messages.Message("CaravanFormationProcessStarted".Translate(), pawnsFromTransferables[0], MessageSound.Benefit);
			return true;
		}

		private bool TryReformCaravan()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(this.transferables);
			if (!this.CheckForErrors(pawnsFromTransferables))
			{
				return false;
			}
			this.AddItemsFromTransferablesToRandomInventories(pawnsFromTransferables);
			Caravan o = CaravanExitMapUtility.ExitMapAndCreateCaravan(pawnsFromTransferables, Faction.OfPlayer, this.map.Tile);
			if (this.map.info.parent != null)
			{
				this.map.info.parent.CheckRemoveMapNow();
			}
			Messages.Message("MessageReformedCaravan".Translate(), o, MessageSound.Benefit);
			return true;
		}

		private void AddItemsFromTransferablesToRandomInventories(List<Pawn> pawns)
		{
			this.transferables.RemoveAll((TransferableOneWay x) => x.AnyThing is Pawn);
			for (int i = 0; i < this.transferables.Count; i++)
			{
				TransferableUtility.TransferNoSplit(this.transferables[i].things, this.transferables[i].countToTransfer, delegate(Thing originalThing, int numToTake)
				{
					Corpse corpse = originalThing as Corpse;
					if (corpse != null && corpse.MapHeld != null)
					{
						corpse.Strip();
					}
					Thing thing = originalThing.SplitOff(numToTake);
					this.RemoveFromCorpseIfPossible(thing);
					CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, pawns, null, null).inventory.innerContainer.TryAdd(thing, true);
				}, true, true);
			}
		}

		private void RemoveFromCorpseIfPossible(Thing thing)
		{
			for (int i = 0; i < this.transferables.Count; i++)
			{
				for (int j = 0; j < this.transferables[i].things.Count; j++)
				{
					Corpse corpse = this.transferables[i].things[j] as Corpse;
					if (corpse != null)
					{
						Pawn innerPawn = corpse.InnerPawn;
						Apparel apparel = thing as Apparel;
						ThingWithComps thingWithComps = thing as ThingWithComps;
						if (innerPawn.inventory.innerContainer.Contains(thing))
						{
							innerPawn.inventory.innerContainer.Remove(thing);
						}
						if (apparel != null && innerPawn.apparel != null && innerPawn.apparel.WornApparel.Contains(apparel))
						{
							apparel.Notify_Stripped(innerPawn);
							innerPawn.apparel.Remove(apparel);
						}
						if (thingWithComps != null && innerPawn.equipment != null && innerPawn.equipment.AllEquipment.Contains(thingWithComps))
						{
							innerPawn.equipment.Remove(thingWithComps);
						}
					}
				}
			}
		}

		private bool CheckForErrors(List<Pawn> pawns)
		{
			if (!this.reform && this.startingTile < 0)
			{
				Messages.Message("NoExitDirectionForCaravanChosen".Translate(), MessageSound.RejectInput);
				return false;
			}
			if (!pawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer) && !x.Downed))
			{
				Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), MessageSound.RejectInput);
				return false;
			}
			if (!this.reform && this.MassUsage > this.MassCapacity)
			{
				this.FlashMass();
				Messages.Message("TooBigCaravanMassUsage".Translate(), MessageSound.RejectInput);
				return false;
			}
			Pawn pawn = pawns.Find((Pawn x) => !x.IsColonist && !pawns.Any((Pawn y) => y.IsColonist && y.CanReach(x, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn)));
			if (pawn != null)
			{
				Messages.Message("CaravanPawnIsUnreachable".Translate(new object[]
				{
					pawn.LabelShort
				}).CapitalizeFirst(), pawn, MessageSound.RejectInput);
				return false;
			}
			for (int i = 0; i < this.transferables.Count; i++)
			{
				if (this.transferables[i].ThingDef.category == ThingCategory.Item)
				{
					int countToTransfer = this.transferables[i].countToTransfer;
					int num = 0;
					if (countToTransfer > 0)
					{
						for (int j = 0; j < this.transferables[i].things.Count; j++)
						{
							Thing t = this.transferables[i].things[j];
							if (!t.Spawned || pawns.Any((Pawn x) => x.IsColonist && x.CanReach(t, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn)))
							{
								num += t.stackCount;
								if (num >= countToTransfer)
								{
									break;
								}
							}
						}
						if (num < countToTransfer)
						{
							if (countToTransfer == 1)
							{
								Messages.Message("CaravanItemIsUnreachableSingle".Translate(new object[]
								{
									this.transferables[i].ThingDef.label
								}), MessageSound.RejectInput);
							}
							else
							{
								Messages.Message("CaravanItemIsUnreachableMulti".Translate(new object[]
								{
									countToTransfer,
									this.transferables[i].ThingDef.label
								}), MessageSound.RejectInput);
							}
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool TryFindExitSpot(List<Pawn> pawns, bool reachableForEveryColonist, out IntVec3 spot)
		{
			if (this.startingTile < 0)
			{
				Log.Error("Can't find exit spot because exitTile is not set.");
				spot = IntVec3.Invalid;
				return false;
			}
			Predicate<IntVec3> validator = (IntVec3 x) => !x.Fogged(this.map) && x.Standable(this.map);
			Rot4 rotFromTo = Find.WorldGrid.GetRotFromTo(this.map.Tile, this.startingTile);
			if (reachableForEveryColonist)
			{
				return CellFinder.TryFindRandomEdgeCellWith(delegate(IntVec3 x)
				{
					if (!validator(x))
					{
						return false;
					}
					for (int j = 0; j < pawns.Count; j++)
					{
						if (pawns[j].IsColonist && !pawns[j].CanReach(x, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
						{
							return false;
						}
					}
					return true;
				}, this.map, rotFromTo, out spot);
			}
			IntVec3 intVec = IntVec3.Invalid;
			int num = -1;
			foreach (IntVec3 current in CellRect.WholeMap(this.map).GetEdgeCells(rotFromTo))
			{
				if (validator(current))
				{
					int num2 = 0;
					for (int i = 0; i < pawns.Count; i++)
					{
						if (pawns[i].IsColonist && pawns[i].CanReach(current, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
						{
							num2++;
						}
					}
					if (num2 > num)
					{
						num = num2;
						intVec = current;
					}
				}
			}
			spot = intVec;
			return intVec.IsValid;
		}

		private void AddPawnsToTransferables()
		{
			List<Pawn> list = CaravanFormingUtility.AllSendablePawns(this.map, this.reform, this.reform);
			for (int i = 0; i < list.Count; i++)
			{
				this.AddToTransferables(list[i], this.reform);
			}
		}

		private void AddItemsToTransferables()
		{
			List<Thing> list = CaravanFormingUtility.AllReachableColonyItems(this.map, this.reform, this.reform);
			for (int i = 0; i < list.Count; i++)
			{
				this.AddToTransferables(list[i], false);
			}
			if (this.AutoStripCorpses)
			{
				for (int j = 0; j < list.Count; j++)
				{
					Corpse corpse = list[j] as Corpse;
					if (corpse != null)
					{
						this.AddCorpseInventoryAndGearToTransferables(corpse);
					}
				}
			}
		}

		private void AddCorpseInventoryAndGearToTransferables(Corpse corpse)
		{
			Pawn_InventoryTracker inventory = corpse.InnerPawn.inventory;
			Pawn_ApparelTracker apparel = corpse.InnerPawn.apparel;
			Pawn_EquipmentTracker equipment = corpse.InnerPawn.equipment;
			for (int i = 0; i < inventory.innerContainer.Count; i++)
			{
				this.AddToTransferables(inventory.innerContainer[i], false);
			}
			if (apparel != null)
			{
				List<Apparel> wornApparel = apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					this.AddToTransferables(wornApparel[j], false);
				}
			}
			if (equipment != null)
			{
				List<ThingWithComps> allEquipment = equipment.AllEquipment;
				for (int k = 0; k < allEquipment.Count; k++)
				{
					this.AddToTransferables(allEquipment[k], false);
				}
			}
		}

		private void FlashMass()
		{
			this.lastMassFlashTime = Time.time;
		}

		private void SetToSendEverything()
		{
			for (int i = 0; i < this.transferables.Count; i++)
			{
				this.transferables[i].SetToTransferMaxToDest();
				TransferableUIUtility.ClearEditBuffer(this.transferables[i]);
			}
			this.CountToTransferChanged();
		}

		private void CountToTransferChanged()
		{
			this.massUsageDirty = true;
			this.massCapacityDirty = true;
			this.daysWorthOfFoodDirty = true;
		}
	}
}
