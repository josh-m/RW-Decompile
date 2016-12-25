using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Designator_ZoneAdd : Designator_Zone
	{
		protected Type zoneTypeToPlace;

		private Zone SelectedZone
		{
			get
			{
				return Find.Selector.SelectedZone;
			}
			set
			{
				Find.Selector.ClearSelection();
				if (value != null)
				{
					Find.Selector.Select(value, false, false);
				}
			}
		}

		protected abstract string NewZoneLabel
		{
			get;
		}

		public Designator_ZoneAdd()
		{
			this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
			this.soundSucceeded = SoundDefOf.DesignateZoneAdd;
			this.useMouseIcon = true;
		}

		protected abstract Zone MakeNewZone();

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			if (Find.Selector.SelectedZone != null && Find.Selector.SelectedZone.GetType() != this.zoneTypeToPlace)
			{
				Find.Selector.Deselect(Find.Selector.SelectedZone);
			}
		}

		public override void DrawMouseAttachments()
		{
			if (this.useMouseIcon)
			{
				string text = string.Empty;
				if (!Input.GetKey(KeyCode.Mouse0))
				{
					Zone selectedZone = Find.Selector.SelectedZone;
					if (selectedZone != null)
					{
						text = "ExpandOrCreateZone".Translate(new object[]
						{
							selectedZone.label,
							this.NewZoneLabel
						});
					}
					else
					{
						text = "CreateNewZone".Translate(new object[]
						{
							this.NewZoneLabel
						});
					}
				}
				GenUI.DrawMouseAttachment(this.icon, text);
			}
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (c.Fogged(base.Map))
			{
				return false;
			}
			if (c.InNoZoneEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
			Zone zone = base.Map.zoneManager.ZoneAt(c);
			if (zone != null && zone.GetType() != this.zoneTypeToPlace)
			{
				return false;
			}
			foreach (Thing current in base.Map.thingGrid.ThingsAt(c))
			{
				if (!current.def.CanOverlapZones)
				{
					return false;
				}
			}
			return true;
		}

		public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			List<IntVec3> list = cells.ToList<IntVec3>();
			if (list.Count == 1)
			{
				Zone zone = base.Map.zoneManager.ZoneAt(list[0]);
				if (zone != null)
				{
					if (zone.GetType() == this.zoneTypeToPlace)
					{
						this.SelectedZone = zone;
					}
					return;
				}
			}
			if (this.SelectedZone == null)
			{
				Zone zone2 = null;
				foreach (IntVec3 current in cells)
				{
					Zone zone3 = base.Map.zoneManager.ZoneAt(current);
					if (zone3 != null && zone3.GetType() == this.zoneTypeToPlace)
					{
						if (zone2 == null)
						{
							zone2 = zone3;
						}
						else if (zone3 != zone2)
						{
							zone2 = null;
							break;
						}
					}
				}
				this.SelectedZone = zone2;
			}
			list.RemoveAll((IntVec3 c) => base.Map.zoneManager.ZoneAt(c) != null);
			if (list.Count == 0)
			{
				return;
			}
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, list)))
			{
				return;
			}
			if (this.SelectedZone == null)
			{
				this.SelectedZone = this.MakeNewZone();
				this.SelectedZone.AddCell(list[0]);
				list.RemoveAt(0);
			}
			bool somethingSucceeded;
			while (true)
			{
				somethingSucceeded = true;
				int count = list.Count;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					bool flag = false;
					for (int j = 0; j < 4; j++)
					{
						IntVec3 c2 = list[i] + GenAdj.CardinalDirections[j];
						if (c2.InBounds(base.Map))
						{
							if (base.Map.zoneManager.ZoneAt(c2) == this.SelectedZone)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						this.SelectedZone.AddCell(list[i]);
						list.RemoveAt(i);
					}
				}
				if (list.Count == 0)
				{
					break;
				}
				if (list.Count == count)
				{
					this.SelectedZone = this.MakeNewZone();
					this.SelectedZone.AddCell(list[0]);
					list.RemoveAt(0);
				}
			}
			this.SelectedZone.CheckContiguous();
			base.Finalize(somethingSucceeded);
			TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, list));
		}
	}
}
