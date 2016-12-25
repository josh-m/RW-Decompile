using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class WITab_Caravan_Gear : WITab
	{
		private const float PawnRowHeight = 50f;

		private const float ItemRowHeight = 30f;

		private const float PawnLabelHeight = 18f;

		private const float PawnLabelColumnWidth = 100f;

		private const float GearLabelColumnWidth = 250f;

		private const float SpaceAroundIcon = 4f;

		private const float EquippedGearColumnWidth = 250f;

		private const float EquippedGearIconSize = 32f;

		private Vector2 leftPaneScrollPosition;

		private float leftPaneScrollViewHeight;

		private Vector2 rightPaneScrollPosition;

		private float rightPaneScrollViewHeight;

		private Thing draggedItem;

		private Vector2 draggedItemPosOffset;

		private bool droppedDraggedItem;

		private float leftPaneWidth;

		private float rightPaneWidth;

		private static List<Apparel> tmpApparel = new List<Apparel>();

		private static List<ThingWithComps> tmpExistingEquipment = new List<ThingWithComps>();

		private static List<Apparel> tmpExistingApparel = new List<Apparel>();

		private List<Pawn> Pawns
		{
			get
			{
				return base.SelCaravan.PawnsListForReading;
			}
		}

		public WITab_Caravan_Gear()
		{
			this.labelKey = "TabCaravanGear";
		}

		protected override void UpdateSize()
		{
			base.UpdateSize();
			this.leftPaneWidth = 469f;
			this.rightPaneWidth = 345f;
			this.size.x = this.leftPaneWidth + this.rightPaneWidth;
			this.size.y = Mathf.Min(550f, this.PaneTopY - 30f);
		}

		public override void OnOpen()
		{
			base.OnOpen();
			this.draggedItem = null;
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			this.CheckDraggedItemStillValid();
			this.CheckDropDraggedItem();
			Rect position = new Rect(0f, 0f, this.leftPaneWidth, this.size.y);
			GUI.BeginGroup(position);
			this.DoLeftPane();
			GUI.EndGroup();
			Rect position2 = new Rect(position.xMax, 0f, this.rightPaneWidth, this.size.y);
			GUI.BeginGroup(position2);
			this.DoRightPane();
			GUI.EndGroup();
			if (this.draggedItem != null && this.droppedDraggedItem)
			{
				this.droppedDraggedItem = false;
				this.draggedItem = null;
			}
		}

		private void DoLeftPane()
		{
			Rect rect = new Rect(0f, 0f, this.leftPaneWidth, this.size.y).ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f, this.leftPaneScrollViewHeight);
			float num = 0f;
			Widgets.BeginScrollView(rect, ref this.leftPaneScrollPosition, rect2);
			this.DoPawnRows(ref num, rect2, rect);
			if (Event.current.type == EventType.Layout)
			{
				this.leftPaneScrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
		}

		private void DoRightPane()
		{
			Rect rect = new Rect(0f, 0f, this.rightPaneWidth, this.size.y).ContractedBy(10f);
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f, this.rightPaneScrollViewHeight);
			bool flag = this.draggedItem != null && rect.Contains(Event.current.mousePosition) && this.CurrentWearerOf(this.draggedItem) != null;
			if (flag)
			{
				Widgets.DrawHighlight(rect);
				if (this.droppedDraggedItem)
				{
					this.MoveDraggedItemToInventory();
					SoundDefOf.TickTiny.PlayOneShotOnCamera();
				}
			}
			float num = 0f;
			Widgets.BeginScrollView(rect, ref this.rightPaneScrollPosition, rect2);
			this.DoInventoryRows(ref num, rect2, rect);
			if (Event.current.type == EventType.Layout)
			{
				this.rightPaneScrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
		}

		protected override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			if (this.draggedItem != null)
			{
				Vector2 mousePosition = Event.current.mousePosition;
				Rect rect = new Rect(mousePosition.x - this.draggedItemPosOffset.x, mousePosition.y - this.draggedItemPosOffset.y, 32f, 32f);
				Find.WindowStack.ImmediateWindow(1283641090, rect, WindowLayer.Super, delegate
				{
					if (this.draggedItem == null)
					{
						return;
					}
					Widgets.ThingIcon(rect.AtZero(), this.draggedItem, 1f);
				}, false, false, 0f);
			}
			this.CheckDropDraggedItem();
		}

		private void DoPawnRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
		{
			List<Pawn> pawns = this.Pawns;
			Text.Font = GameFont.Tiny;
			GUI.color = Color.gray;
			Widgets.Label(new Rect(135f, curY + 6f, 200f, 100f), "DragToRearrange".Translate());
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (pawn.IsColonist)
				{
					this.DoPawnRow(ref curY, scrollViewRect, scrollOutRect, pawn);
				}
			}
			bool flag = false;
			for (int j = 0; j < pawns.Count; j++)
			{
				Pawn pawn2 = pawns[j];
				if (pawn2.IsPrisoner)
				{
					if (!flag)
					{
						Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisoners".Translate());
						flag = true;
					}
					this.DoPawnRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
				}
			}
		}

		private void DoPawnRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
		{
			float num = this.leftPaneScrollPosition.y - 50f;
			float num2 = this.leftPaneScrollPosition.y + scrollOutRect.height;
			if (curY > num && curY < num2)
			{
				this.DoPawnRow(new Rect(0f, curY, viewRect.width, 50f), p);
			}
			curY += 50f;
		}

		private void DoPawnRow(Rect rect, Pawn p)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			CaravanPeopleAndItemsTabUtility.DoAbandonButton(rect2, p, base.SelCaravan);
			rect2.width -= 24f;
			Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
			rect2.width -= 24f;
			bool flag = this.draggedItem != null && rect2.Contains(Event.current.mousePosition) && this.CurrentWearerOf(this.draggedItem) != p;
			if ((Mouse.IsOver(rect2) && this.draggedItem == null) || flag)
			{
				Widgets.DrawHighlight(rect2);
			}
			if (flag && this.droppedDraggedItem)
			{
				this.TryEquipDraggedItem(p);
				SoundDefOf.TickTiny.PlayOneShotOnCamera();
			}
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, p, 1f);
			Rect bgRect = new Rect(rect3.xMax + 4f, 16f, 100f, 18f);
			GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, false, false);
			float xMax = bgRect.xMax;
			if (p.equipment != null)
			{
				List<ThingWithComps> allEquipment = p.equipment.AllEquipment;
				for (int i = 0; i < allEquipment.Count; i++)
				{
					this.DoEquippedGear(allEquipment[i], p, ref xMax);
				}
			}
			if (p.apparel != null)
			{
				WITab_Caravan_Gear.tmpApparel.Clear();
				WITab_Caravan_Gear.tmpApparel.AddRange(p.apparel.WornApparel);
				WITab_Caravan_Gear.tmpApparel.SortBy((Apparel x) => (int)x.def.apparel.LastLayer, (Apparel x) => -x.def.apparel.HumanBodyCoverage);
				for (int j = 0; j < WITab_Caravan_Gear.tmpApparel.Count; j++)
				{
					this.DoEquippedGear(WITab_Caravan_Gear.tmpApparel[j], p, ref xMax);
				}
			}
			if (p.Downed)
			{
				GUI.color = new Color(1f, 0f, 0f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
				GUI.color = Color.white;
			}
			GUI.EndGroup();
		}

		private void DoInventoryRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
		{
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(base.SelCaravan);
			Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanWeaponsAndApparel".Translate());
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (this.IsVisibleWeapon(thing.def))
				{
					if (!flag)
					{
						flag = true;
					}
					this.DoInventoryRow(ref curY, scrollViewRect, scrollOutRect, thing);
				}
			}
			bool flag2 = false;
			for (int j = 0; j < list.Count; j++)
			{
				Thing thing2 = list[j];
				if (thing2.def.IsApparel)
				{
					if (!flag2)
					{
						flag2 = true;
					}
					this.DoInventoryRow(ref curY, scrollViewRect, scrollOutRect, thing2);
				}
			}
			if (!flag && !flag2)
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(new Rect(0f, curY, scrollViewRect.width, 25f), "NoneBrackets".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				curY += 25f;
				GUI.color = Color.white;
			}
		}

		private void DoInventoryRow(ref float curY, Rect viewRect, Rect scrollOutRect, Thing t)
		{
			float num = this.rightPaneScrollPosition.y - 30f;
			float num2 = this.rightPaneScrollPosition.y + scrollOutRect.height;
			if (curY > num && curY < num2)
			{
				this.DoInventoryRow(new Rect(0f, curY, viewRect.width, 30f), t);
			}
			curY += 30f;
		}

		private void DoInventoryRow(Rect rect, Thing t)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = rect.AtZero();
			Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, t);
			rect2.width -= 24f;
			if (this.draggedItem == null && Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
			}
			float num = (t != this.draggedItem) ? 1f : 0.5f;
			Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
			Widgets.ThingIcon(rect3, t, num);
			GUI.color = new Color(1f, 1f, 1f, num);
			Rect rect4 = new Rect(rect3.xMax + 4f, 0f, 250f, 30f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Widgets.Label(rect4, t.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			GUI.color = Color.white;
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect2))
			{
				this.draggedItem = t;
				this.droppedDraggedItem = false;
				this.draggedItemPosOffset = new Vector2(16f, 16f);
				Event.current.Use();
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			GUI.EndGroup();
		}

		private void DoEquippedGear(Thing t, Pawn p, ref float curX)
		{
			Rect rect = new Rect(curX, 9f, 32f, 32f);
			bool flag = Mouse.IsOver(rect);
			float alpha;
			if (t == this.draggedItem)
			{
				alpha = 0.2f;
			}
			else if (flag && this.draggedItem == null)
			{
				alpha = 0.75f;
			}
			else
			{
				alpha = 1f;
			}
			Widgets.ThingIcon(rect, t, alpha);
			curX += 32f;
			TooltipHandler.TipRegion(rect, t.LabelCap);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && flag)
			{
				this.draggedItem = t;
				this.droppedDraggedItem = false;
				this.draggedItemPosOffset = Event.current.mousePosition - rect.position;
				Event.current.Use();
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
		}

		private void CheckDraggedItemStillValid()
		{
			if (this.draggedItem == null)
			{
				return;
			}
			if (this.draggedItem.Destroyed)
			{
				this.draggedItem = null;
				return;
			}
			if (this.CurrentWearerOf(this.draggedItem) != null)
			{
				return;
			}
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(base.SelCaravan);
			if (list.Contains(this.draggedItem))
			{
				return;
			}
			this.draggedItem = null;
		}

		private void CheckDropDraggedItem()
		{
			if (this.draggedItem == null)
			{
				return;
			}
			if (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseUp)
			{
				this.droppedDraggedItem = true;
			}
		}

		private bool IsVisibleWeapon(ThingDef t)
		{
			return t.IsWeapon && t != ThingDefOf.WoodLog && t != ThingDefOf.Beer;
		}

		private Pawn CurrentWearerOf(Thing t)
		{
			List<Pawn> pawns = this.Pawns;
			ThingWithComps thingWithComps = t as ThingWithComps;
			Apparel apparel = t as Apparel;
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (thingWithComps != null && pawn.equipment != null && pawn.equipment.AllEquipment.Contains(thingWithComps))
				{
					return pawn;
				}
				if (apparel != null && pawn.apparel != null && pawn.apparel.WornApparel.Contains(apparel))
				{
					return pawn;
				}
			}
			return null;
		}

		private bool TryRemoveFromCurrentWearer(Thing t)
		{
			Pawn pawn = this.CurrentWearerOf(t);
			if (pawn == null)
			{
				return false;
			}
			Apparel apparel = t as Apparel;
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (apparel != null && pawn.apparel != null && pawn.apparel.WornApparel.Contains(apparel))
			{
				pawn.apparel.Remove(apparel);
				return true;
			}
			if (thingWithComps != null && pawn.equipment != null && pawn.equipment.AllEquipment.Contains(thingWithComps))
			{
				pawn.equipment.Remove(thingWithComps);
				return true;
			}
			return false;
		}

		private void MoveDraggedItemToInventory()
		{
			this.droppedDraggedItem = false;
			if (!this.TryRemoveFromCurrentWearer(this.draggedItem))
			{
				Log.Warning("Could not remove dragged item from its source.");
				this.draggedItem = null;
				return;
			}
			Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(this.draggedItem, this.Pawns, null, null);
			if (pawn != null)
			{
				pawn.inventory.innerContainer.TryAdd(this.draggedItem, true);
			}
			else
			{
				Log.Warning("Could not find any pawn to move " + this.draggedItem + " to.");
			}
			this.draggedItem = null;
		}

		private void TryEquipDraggedItem(Pawn p)
		{
			this.droppedDraggedItem = false;
			if (this.draggedItem.def.IsWeapon)
			{
				if (p.story != null && p.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					Messages.Message("MessageCantEquipIncapableOfViolence".Translate(new object[]
					{
						p.LabelShort
					}), p, MessageSound.RejectInput);
					this.draggedItem = null;
					return;
				}
				if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					Messages.Message("MessageCantEquipIncapableOfManipulation".Translate(), p, MessageSound.RejectInput);
					this.draggedItem = null;
					return;
				}
			}
			if (!this.TryRemoveFromCurrentWearer(this.draggedItem))
			{
				Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(base.SelCaravan, this.draggedItem);
				if (ownerOf == null)
				{
					Log.Warning("Could not remove dragged item from its source.");
					this.draggedItem = null;
					return;
				}
				ownerOf.inventory.innerContainer.Remove(this.draggedItem);
			}
			Apparel apparel = this.draggedItem as Apparel;
			ThingWithComps thingWithComps = this.draggedItem as ThingWithComps;
			if (apparel != null && p.apparel != null)
			{
				WITab_Caravan_Gear.tmpExistingApparel.Clear();
				WITab_Caravan_Gear.tmpExistingApparel.AddRange(p.apparel.WornApparel);
				for (int i = 0; i < WITab_Caravan_Gear.tmpExistingApparel.Count; i++)
				{
					if (!ApparelUtility.CanWearTogether(apparel.def, WITab_Caravan_Gear.tmpExistingApparel[i].def))
					{
						Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(WITab_Caravan_Gear.tmpExistingApparel[i], this.Pawns, null, null);
						if (pawn != null)
						{
							pawn.inventory.innerContainer.TryAdd(WITab_Caravan_Gear.tmpExistingApparel[i], true);
						}
						else
						{
							Log.Warning("Could not find any pawn to move " + WITab_Caravan_Gear.tmpExistingApparel[i] + " to.");
						}
					}
				}
				p.apparel.Wear(apparel, false);
				if (p.outfits != null)
				{
					p.outfits.forcedHandler.SetForced(apparel, true);
				}
			}
			else if (thingWithComps != null && p.equipment != null)
			{
				WITab_Caravan_Gear.tmpExistingEquipment.Clear();
				WITab_Caravan_Gear.tmpExistingEquipment.AddRange(p.equipment.AllEquipment);
				for (int j = 0; j < WITab_Caravan_Gear.tmpExistingEquipment.Count; j++)
				{
					p.equipment.Remove(WITab_Caravan_Gear.tmpExistingEquipment[j]);
					Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(WITab_Caravan_Gear.tmpExistingEquipment[j], this.Pawns, null, null);
					if (pawn2 != null)
					{
						pawn2.inventory.innerContainer.TryAdd(WITab_Caravan_Gear.tmpExistingEquipment[j], true);
					}
					else
					{
						Log.Warning("Could not find any pawn to move " + WITab_Caravan_Gear.tmpExistingEquipment[j] + " to.");
					}
				}
				p.equipment.AddEquipment(thingWithComps);
			}
			else
			{
				Log.Warning(string.Concat(new object[]
				{
					"Could not make ",
					p,
					" equip or wear ",
					this.draggedItem
				}));
			}
			this.draggedItem = null;
		}
	}
}
