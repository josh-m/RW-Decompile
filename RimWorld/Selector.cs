using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Selector
	{
		private const float PawnSelectRadius = 1f;

		private const int MaxNumSelected = 80;

		public DragBox dragBox = new DragBox();

		private List<object> selected = new List<object>();

		private bool ShiftIsHeld
		{
			get
			{
				return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			}
		}

		public List<object> SelectedObjects
		{
			get
			{
				return this.selected;
			}
		}

		public List<object> SelectedObjectsListForReading
		{
			get
			{
				return this.selected;
			}
		}

		public Thing SingleSelectedThing
		{
			get
			{
				if (this.selected.Count != 1)
				{
					return null;
				}
				if (this.selected[0] is Thing)
				{
					return (Thing)this.selected[0];
				}
				return null;
			}
		}

		public object FirstSelectedObject
		{
			get
			{
				if (this.selected.Count == 0)
				{
					return null;
				}
				return this.selected[0];
			}
		}

		public object SingleSelectedObject
		{
			get
			{
				if (this.selected.Count != 1)
				{
					return null;
				}
				return this.selected[0];
			}
		}

		public int NumSelected
		{
			get
			{
				return this.selected.Count;
			}
		}

		public Zone SelectedZone
		{
			get
			{
				if (this.selected.Count == 0)
				{
					return null;
				}
				return this.selected[0] as Zone;
			}
			set
			{
				this.ClearSelection();
				if (value != null)
				{
					this.Select(value, true, true);
				}
			}
		}

		public void SelectorOnGUI()
		{
			this.HandleWorldClicks();
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && this.selected.Count > 0)
			{
				this.ClearSelection();
				Event.current.Use();
			}
			if (this.NumSelected > 0 && Find.MainTabsRoot.OpenTab == null)
			{
				Find.MainTabsRoot.SetCurrentTab(MainTabDefOf.Inspect, false);
			}
		}

		private void HandleWorldClicks()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (Event.current.clickCount == 1)
					{
						this.dragBox.active = true;
						this.dragBox.start = Gen.MouseMapPosVector3();
					}
					if (Event.current.clickCount == 2)
					{
						this.SelectAllMatchingObjectUnderMouseOnScreen();
					}
					Event.current.Use();
				}
				if (Event.current.button == 1 && this.selected.Count > 0)
				{
					if (this.selected.Count == 1 && this.selected[0] is Pawn)
					{
						FloatMenuMakerMap.TryMakeFloatMenu((Pawn)this.selected[0]);
					}
					else
					{
						for (int i = 0; i < this.selected.Count; i++)
						{
							Pawn pawn = this.selected[i] as Pawn;
							if (pawn != null)
							{
								Selector.AutoOrderToCell(pawn, Gen.MouseCell());
							}
						}
					}
					Event.current.Use();
				}
			}
			if (Event.current.type == EventType.MouseUp)
			{
				if (Event.current.button == 0 && this.dragBox.active)
				{
					this.dragBox.active = false;
					if (!this.dragBox.IsValid)
					{
						this.SelectUnderMouse();
					}
					else
					{
						this.SelectInsideDragBox();
					}
				}
				Event.current.Use();
			}
		}

		public bool IsSelected(object obj)
		{
			return this.selected.Contains(obj);
		}

		public void ClearSelection()
		{
			SelectionDrawer.Clear();
			this.selected.Clear();
		}

		public void Deselect(object obj)
		{
			if (this.selected.Contains(obj))
			{
				this.selected.Remove(obj);
			}
		}

		public void Select(object obj, bool playSound = true, bool forceDesignatorDeselect = true)
		{
			if (obj == null)
			{
				Log.Error("Cannot select null.");
				return;
			}
			Thing thing = obj as Thing;
			if (thing != null && thing.Destroyed)
			{
				Log.Error("Cannot select destroyed thing.");
				return;
			}
			if (forceDesignatorDeselect)
			{
				DesignatorManager.Deselect();
			}
			if (this.SelectedZone != null && !(obj is Zone))
			{
				this.ClearSelection();
			}
			if (obj is Zone && this.SelectedZone == null)
			{
				this.ClearSelection();
			}
			if (this.selected.Count >= 80)
			{
				return;
			}
			if (!this.IsSelected(obj))
			{
				if (playSound)
				{
					this.PlaySelectionSoundFor(obj);
				}
				this.selected.Add(obj);
				SelectionDrawer.Notify_Selected(obj);
			}
		}

		public void Notify_DialogOpened()
		{
			this.dragBox.active = false;
		}

		private void PlaySelectionSoundFor(object obj)
		{
			if (obj is Pawn && ((Pawn)obj).Faction == Faction.OfPlayer && ((Pawn)obj).RaceProps.Humanlike)
			{
				SoundDefOf.ColonistSelected.PlayOneShotOnCamera();
			}
			else if (obj is Thing)
			{
				SoundDefOf.ThingSelected.PlayOneShotOnCamera();
			}
			else if (obj is Zone)
			{
				SoundDefOf.ZoneSelected.PlayOneShotOnCamera();
			}
			else
			{
				Log.Warning("Can't determine selection sound for " + obj);
			}
		}

		private void SelectInsideDragBox()
		{
			if (!this.ShiftIsHeld)
			{
				this.ClearSelection();
			}
			bool flag = false;
			List<Thing> list = Find.ColonistBar.ColonistsInScreenRect(this.dragBox.ScreenRect);
			if (list.Any<Thing>())
			{
				flag = true;
				for (int i = 0; i < list.Count; i++)
				{
					this.Select(list[i], true, true);
				}
			}
			if (!flag)
			{
				List<Thing> list2 = ThingSelectionUtility.MultiSelectableThingsInScreenRectDistinct(this.dragBox.ScreenRect).ToList<Thing>();
				Predicate<Thing> isColonist = (Thing t) => t.Faction == Faction.OfPlayer && t.def.category == ThingCategory.Pawn && ((Pawn)t).RaceProps.Humanlike;
				if (list2.Any((Thing t) => isColonist(t)))
				{
					list2.RemoveAll((Thing t) => !isColonist(t));
				}
				else
				{
					Predicate<Thing> isRaiderHumanlike = (Thing t) => t.HostileTo(Faction.OfPlayer) && t.def.category == ThingCategory.Pawn && ((Pawn)t).RaceProps.Humanlike;
					if (list2.Any((Thing t) => isRaiderHumanlike(t)))
					{
						list2.RemoveAll((Thing t) => !isRaiderHumanlike(t));
					}
					else
					{
						Predicate<Thing> isResource = (Thing t) => t.def.CountAsResource;
						if (list2.Any((Thing t) => isResource(t)))
						{
							list2.RemoveAll((Thing t) => !isResource(t));
						}
						else
						{
							Predicate<Thing> isAnimal = (Thing t) => t.def.category == ThingCategory.Pawn && !((Pawn)t).RaceProps.Humanlike;
							if (list2.Any((Thing t) => isAnimal(t)))
							{
								list2.RemoveAll((Thing t) => !isAnimal(t));
							}
						}
					}
				}
				foreach (Thing current in list2)
				{
					flag = true;
					this.Select(current, true, true);
				}
			}
			if (!flag)
			{
				List<Zone> list3 = ThingSelectionUtility.MultiSelectableZonesInScreenRectDistinct(this.dragBox.ScreenRect).ToList<Zone>();
				foreach (Zone current2 in list3)
				{
					flag = true;
					this.Select(current2, true, true);
				}
			}
			if (!flag)
			{
				this.SelectUnderMouse();
			}
		}

		[DebuggerHidden]
		private IEnumerable<object> SelectableObjectsUnderMouse()
		{
			Vector3 mousePos = Input.mousePosition;
			mousePos.y = (float)Screen.height - mousePos.y;
			Thing colonist = Find.ColonistBar.ColonistAt(mousePos);
			if (colonist != null)
			{
				yield return colonist;
			}
			else if (Gen.MouseCell().InBounds())
			{
				TargetingParameters selectParams = new TargetingParameters();
				selectParams.mustBeSelectable = true;
				selectParams.canTargetPawns = true;
				selectParams.canTargetBuildings = true;
				selectParams.canTargetItems = true;
				selectParams.worldObjectTargetsMustBeAutoAttackable = false;
				List<Thing> selectableList = GenUI.ThingsUnderMouse(Gen.MouseMapPosVector3(), 1f, selectParams);
				if (selectableList.Count > 0 && selectableList[0] is Pawn && (selectableList[0].DrawPos - Gen.MouseMapPosVector3()).MagnitudeHorizontal() < 0.4f)
				{
					for (int i = selectableList.Count - 1; i >= 0; i--)
					{
						Thing t = selectableList[i];
						if (t.def.category == ThingCategory.Pawn && (t.DrawPos - Gen.MouseMapPosVector3()).MagnitudeHorizontal() > 0.4f)
						{
							selectableList.Remove(t);
						}
					}
				}
				for (int j = 0; j < selectableList.Count; j++)
				{
					yield return selectableList[j];
				}
				Zone z = Find.ZoneManager.ZoneAt(Gen.MouseCell());
				if (z != null)
				{
					yield return z;
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<object> SelectableObjectsAt(IntVec3 c)
		{
			foreach (Thing t in Find.ThingGrid.ThingsAt(c))
			{
				if (t.SelectableNow())
				{
					yield return t;
				}
			}
			Zone z = Find.ZoneManager.ZoneAt(c);
			if (z != null)
			{
				yield return z;
			}
		}

		private void SelectUnderMouse()
		{
			List<object> list = this.SelectableObjectsUnderMouse().ToList<object>();
			if (list.Count == 0)
			{
				if (!this.ShiftIsHeld)
				{
					this.ClearSelection();
				}
			}
			else if (list.Count == 1)
			{
				object obj3 = list[0];
				if (!this.ShiftIsHeld)
				{
					this.ClearSelection();
					this.Select(obj3, true, true);
				}
				else if (!this.selected.Contains(obj3))
				{
					this.Select(obj3, true, true);
				}
				else
				{
					this.Deselect(obj3);
				}
			}
			else if (list.Count > 1)
			{
				object obj2 = (from obj in list
				where this.selected.Contains(obj)
				select obj).FirstOrDefault<object>();
				if (obj2 != null)
				{
					if (!this.ShiftIsHeld)
					{
						int num = list.IndexOf(obj2) + 1;
						if (num >= list.Count)
						{
							num -= list.Count;
						}
						this.ClearSelection();
						this.Select(list[num], true, true);
					}
					else
					{
						foreach (object current in list)
						{
							if (this.selected.Contains(current))
							{
								this.Deselect(current);
							}
						}
					}
				}
				else
				{
					if (!this.ShiftIsHeld)
					{
						this.ClearSelection();
					}
					this.Select(list[0], true, true);
				}
			}
		}

		public void SelectNextAt(IntVec3 c)
		{
			if (this.SelectedObjects.Count<object>() != 1)
			{
				Log.Error("Cannot select next at with < or > 1 selected.");
				return;
			}
			List<object> list = Selector.SelectableObjectsAt(c).ToList<object>();
			int num = list.IndexOf(this.SingleSelectedThing) + 1;
			if (num >= list.Count)
			{
				num -= list.Count;
			}
			this.ClearSelection();
			this.Select(list[num], true, true);
		}

		private void SelectAllMatchingObjectUnderMouseOnScreen()
		{
			List<object> list = this.SelectableObjectsUnderMouse().ToList<object>();
			if (list.Count == 0)
			{
				return;
			}
			Thing clickedThing = list.FirstOrDefault((object o) => o is Pawn && ((Pawn)o).Faction == Faction.OfPlayer && !((Pawn)o).IsPrisoner) as Thing;
			clickedThing = (list.FirstOrDefault((object o) => o is Pawn) as Thing);
			if (clickedThing == null)
			{
				clickedThing = ((from o in list
				where o is Thing && !((Thing)o).def.neverMultiSelect
				select o).FirstOrDefault<object>() as Thing);
			}
			Rect rect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
			if (clickedThing != null)
			{
				IEnumerable enumerable = ThingSelectionUtility.MultiSelectableThingsInScreenRectDistinct(rect);
				Predicate<Thing> predicate = delegate(Thing t)
				{
					if (t.def != clickedThing.def || t.Faction != clickedThing.Faction || this.IsSelected(t))
					{
						return false;
					}
					Pawn pawn = clickedThing as Pawn;
					if (pawn != null)
					{
						Pawn pawn2 = t as Pawn;
						if (pawn2.RaceProps != pawn.RaceProps)
						{
							return false;
						}
						if (pawn2.HostFaction != pawn.HostFaction)
						{
							return false;
						}
					}
					return true;
				};
				foreach (Thing obj in enumerable)
				{
					if (predicate(obj))
					{
						this.Select(obj, true, true);
					}
				}
				return;
			}
			if (list.FirstOrDefault((object o) => o is Zone && ((Zone)o).IsMultiselectable) == null)
			{
				return;
			}
			IEnumerable<Zone> enumerable2 = ThingSelectionUtility.MultiSelectableZonesInScreenRectDistinct(rect);
			foreach (Zone current in enumerable2)
			{
				if (!this.IsSelected(current))
				{
					this.Select(current, true, true);
				}
			}
		}

		public void SelectNextColonist()
		{
			List<Pawn> list = (from c in Find.ColonistBar.ColonistsInOrder
			where !c.Dead
			select c).ToList<Pawn>();
			if (list.Count == 0)
			{
				return;
			}
			int num = -1;
			for (int i = 0; i < this.selected.Count; i++)
			{
				Pawn pawn = this.selected[i] as Pawn;
				if (pawn != null)
				{
					num = Mathf.Max(num, list.IndexOf(pawn));
				}
			}
			if (num == -1)
			{
				this.SingleSelectAndJumpTo(list[0]);
			}
			else
			{
				this.SingleSelectAndJumpTo(list[(num + 1) % list.Count]);
			}
		}

		public void SelectPreviousColonist()
		{
			List<Pawn> list = (from c in Find.ColonistBar.ColonistsInOrder
			where !c.Dead
			select c).ToList<Pawn>();
			if (list.Count == 0)
			{
				return;
			}
			int num = -1;
			for (int i = 0; i < this.selected.Count; i++)
			{
				Pawn pawn = this.selected[i] as Pawn;
				if (pawn != null)
				{
					num = Mathf.Max(num, list.IndexOf(pawn));
				}
			}
			if (num == -1)
			{
				this.SingleSelectAndJumpTo(list[list.Count - 1]);
			}
			else
			{
				num--;
				if (num < 0)
				{
					num += list.Count;
				}
				this.SingleSelectAndJumpTo(list[num]);
			}
		}

		private void SingleSelectAndJumpTo(Thing t)
		{
			if (t == null)
			{
				return;
			}
			this.ClearSelection();
			this.Select(t, true, true);
			Find.CameraDriver.JumpTo(t.Position);
		}

		private static void AutoOrderToCell(Pawn pawn, IntVec3 dest)
		{
			foreach (FloatMenuOption current in FloatMenuMakerMap.ChoicesAtFor(dest.ToVector3Shifted(), pawn))
			{
				if (current.autoTakeable)
				{
					current.Chosen(true);
					break;
				}
			}
		}
	}
}
