using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class Dialog_SplitCaravan : Window
	{
		private enum Tab
		{
			Pawns,
			Items
		}

		private const float TitleRectHeight = 40f;

		private const float BottomAreaHeight = 55f;

		private Caravan caravan;

		private List<TransferableOneWay> transferables;

		private TransferableOneWayWidget pawnsTransfer;

		private TransferableOneWayWidget itemsTransfer;

		private Dialog_SplitCaravan.Tab tab;

		private float lastSourceMassFlashTime = -9999f;

		private float lastDestMassFlashTime = -9999f;

		private bool sourceMassUsageDirty = true;

		private float cachedSourceMassUsage;

		private bool sourceMassCapacityDirty = true;

		private float cachedSourceMassCapacity;

		private bool sourceDaysWorthOfFoodDirty = true;

		private float cachedSourceDaysWorthOfFood;

		private bool destMassUsageDirty = true;

		private float cachedDestMassUsage;

		private bool destMassCapacityDirty = true;

		private float cachedDestMassCapacity;

		private bool destDaysWorthOfFoodDirty = true;

		private float cachedDestDaysWorthOfFood;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

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

		private float SourceMassUsage
		{
			get
			{
				if (this.sourceMassUsageDirty)
				{
					this.sourceMassUsageDirty = false;
					this.cachedSourceMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTransfer(this.transferables, true, false, false);
				}
				return this.cachedSourceMassUsage;
			}
		}

		private float SourceMassCapacity
		{
			get
			{
				if (this.sourceMassCapacityDirty)
				{
					this.sourceMassCapacityDirty = false;
					this.cachedSourceMassCapacity = CollectionsMassCalculator.CapacityLeftAfterTransfer(this.transferables);
				}
				return this.cachedSourceMassCapacity;
			}
		}

		private float SourceDaysWorthOfFood
		{
			get
			{
				if (this.sourceDaysWorthOfFoodDirty)
				{
					this.sourceDaysWorthOfFoodDirty = false;
					this.cachedSourceDaysWorthOfFood = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTransfer(this.transferables);
				}
				return this.cachedSourceDaysWorthOfFood;
			}
		}

		private float DestMassUsage
		{
			get
			{
				if (this.destMassUsageDirty)
				{
					this.destMassUsageDirty = false;
					this.cachedDestMassUsage = CollectionsMassCalculator.MassUsageTransferables(this.transferables, true, false, false);
				}
				return this.cachedDestMassUsage;
			}
		}

		private float DestMassCapacity
		{
			get
			{
				if (this.destMassCapacityDirty)
				{
					this.destMassCapacityDirty = false;
					this.cachedDestMassCapacity = CollectionsMassCalculator.CapacityTransferables(this.transferables);
				}
				return this.cachedDestMassCapacity;
			}
		}

		private float DestDaysWorthOfFood
		{
			get
			{
				if (this.destDaysWorthOfFoodDirty)
				{
					this.destDaysWorthOfFoodDirty = false;
					this.cachedDestDaysWorthOfFood = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables);
				}
				return this.cachedDestDaysWorthOfFood;
			}
		}

		public Dialog_SplitCaravan(Caravan caravan)
		{
			this.caravan = caravan;
			this.closeOnEscapeKey = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			this.CalculateAndRecacheTransferables();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, inRect.width, 40f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "SplitCaravan".Translate());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Dialog_SplitCaravan.tabsList.Clear();
			Dialog_SplitCaravan.tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
			{
				this.tab = Dialog_SplitCaravan.Tab.Pawns;
			}, this.tab == Dialog_SplitCaravan.Tab.Pawns));
			Dialog_SplitCaravan.tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
			{
				this.tab = Dialog_SplitCaravan.Tab.Items;
			}, this.tab == Dialog_SplitCaravan.Tab.Items));
			inRect.yMin += 72f;
			Widgets.DrawMenuSection(inRect, true);
			TabDrawer.DrawTabs(inRect, Dialog_SplitCaravan.tabsList);
			inRect = inRect.ContractedBy(17f);
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			Rect rect3 = rect2;
			rect3.y += 32f;
			rect3.xMin += rect2.width - this.pawnsTransfer.TotalNumbersColumnsWidths;
			this.DrawMassAndFoodInfo(rect3);
			this.DoBottomButtons(rect2);
			Rect inRect2 = rect2;
			inRect2.yMax -= 59f;
			bool flag = false;
			Dialog_SplitCaravan.Tab tab = this.tab;
			if (tab != Dialog_SplitCaravan.Tab.Pawns)
			{
				if (tab == Dialog_SplitCaravan.Tab.Items)
				{
					this.itemsTransfer.OnGUI(inRect2, out flag);
				}
			}
			else
			{
				this.pawnsTransfer.OnGUI(inRect2, out flag);
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

		private void AddToTransferables(Thing t)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.transferables);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				this.transferables.Add(transferableOneWay);
			}
			transferableOneWay.things.Add(t);
		}

		private void DrawMassAndFoodInfo(Rect rect)
		{
			TransferableUIUtility.DrawMassInfo(rect, this.SourceMassUsage, this.SourceMassCapacity, "SplitCaravanMassUsageTooltip".Translate(), this.lastSourceMassFlashTime, false);
			CaravanUIUtility.DrawDaysWorthOfFoodInfo(new Rect(rect.x, rect.y + 22f, rect.width, rect.height), this.SourceDaysWorthOfFood, false);
			TransferableUIUtility.DrawMassInfo(rect, this.DestMassUsage, this.DestMassCapacity, "SplitCaravanMassUsageTooltip".Translate(), this.lastDestMassFlashTime, true);
			CaravanUIUtility.DrawDaysWorthOfFoodInfo(new Rect(rect.x, rect.y + 22f, rect.width, rect.height), this.DestDaysWorthOfFood, true);
		}

		private void DoBottomButtons(Rect rect)
		{
			Rect rect2 = new Rect(rect.width / 2f - this.BottomButtonSize.x / 2f, rect.height - 55f, this.BottomButtonSize.x, this.BottomButtonSize.y);
			if (Widgets.ButtonText(rect2, "AcceptButton".Translate(), true, false, true) && this.TrySplitCaravan())
			{
				SoundDefOf.TickHigh.PlayOneShotOnCamera();
				this.Close(false);
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
		}

		private void CalculateAndRecacheTransferables()
		{
			this.transferables = new List<TransferableOneWay>();
			this.AddPawnsToTransferables();
			this.AddItemsToTransferables();
			CaravanUIUtility.CreateCaravanTransferableWidgets(this.transferables, out this.pawnsTransfer, out this.itemsTransfer, "CaravanSplitSourceLabel".Translate(), "CaravanSplitDestLabel".Translate(), "SplitCaravanThingCountTip".Translate(), true, () => this.DestMassCapacity - this.DestMassUsage, false);
			this.CountToTransferChanged();
		}

		private bool TrySplitCaravan()
		{
			List<Pawn> pawns = TransferableUtility.GetPawnsFromTransferables(this.transferables);
			if (!this.CheckForErrors(pawns))
			{
				return false;
			}
			for (int i = 0; i < pawns.Count; i++)
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawns[i], this.caravan.PawnsListForReading, pawns);
			}
			Caravan caravan = (Caravan)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Caravan);
			caravan.Tile = this.caravan.Tile;
			caravan.SetFaction(this.caravan.Faction);
			caravan.Name = CaravanNameGenerator.GenerateCaravanName(caravan);
			Find.WorldObjects.Add(caravan);
			for (int j = 0; j < pawns.Count; j++)
			{
				this.caravan.RemovePawn(pawns[j]);
				caravan.AddPawn(pawns[j], true);
			}
			this.transferables.RemoveAll((TransferableOneWay x) => x.AnyThing is Pawn);
			for (int k = 0; k < this.transferables.Count; k++)
			{
				TransferableUtility.TransferNoSplit(this.transferables[k].things, this.transferables[k].countToTransfer, delegate(Thing thing, int numToTake)
				{
					Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(this.caravan, thing);
					if (ownerOf == null)
					{
						Log.Error("Error while splitting a caravan: Thing " + thing + " has no owner. Where did it come from then?");
						return;
					}
					CaravanInventoryUtility.MoveInventoryToSomeoneElse(ownerOf, thing, pawns, null, numToTake);
				}, true, true);
			}
			return true;
		}

		private bool CheckForErrors(List<Pawn> pawns)
		{
			if (!pawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer) && !x.Downed))
			{
				Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), this.caravan, MessageSound.RejectInput);
				return false;
			}
			if (!this.AnyNonDownedColonistLeftInSourceCaravan(pawns))
			{
				Messages.Message("SourceCaravanMustHaveAtLeastOneColonist".Translate(), this.caravan, MessageSound.RejectInput);
				return false;
			}
			return true;
		}

		private void AddPawnsToTransferables()
		{
			List<Pawn> pawnsListForReading = this.caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				this.AddToTransferables(pawnsListForReading[i]);
			}
		}

		private void AddItemsToTransferables()
		{
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(this.caravan);
			for (int i = 0; i < list.Count; i++)
			{
				this.AddToTransferables(list[i]);
			}
		}

		private void FlashSourceMass()
		{
			this.lastSourceMassFlashTime = Time.time;
		}

		private void FlashDestMass()
		{
			this.lastDestMassFlashTime = Time.time;
		}

		private bool AnyNonDownedColonistLeftInSourceCaravan(List<Pawn> pawnsToTransfer)
		{
			return this.transferables.Any((TransferableOneWay x) => x.things.Any(delegate(Thing y)
			{
				Pawn pawn = y as Pawn;
				return pawn != null && CaravanUtility.IsOwner(pawn, Faction.OfPlayer) && !pawn.Downed && !pawnsToTransfer.Contains(pawn);
			}));
		}

		private void CountToTransferChanged()
		{
			this.sourceMassUsageDirty = true;
			this.sourceMassCapacityDirty = true;
			this.sourceDaysWorthOfFoodDirty = true;
			this.destMassUsageDirty = true;
			this.destMassCapacityDirty = true;
			this.destDaysWorthOfFoodDirty = true;
		}
	}
}
