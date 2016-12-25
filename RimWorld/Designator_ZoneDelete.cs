using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneDelete : Designator_Zone
	{
		private List<Zone> justDesignated = new List<Zone>();

		public Designator_ZoneDelete()
		{
			this.defaultLabel = "DesignatorZoneDelete".Translate();
			this.defaultDesc = "DesignatorZoneDeleteDesc".Translate();
			this.soundDragSustain = SoundDefOf.DesignateDragAreaDelete;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaDeleteChanged;
			this.soundSucceeded = SoundDefOf.DesignateZoneDelete;
			this.useMouseIcon = true;
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneDelete", true);
			this.hotKey = KeyBindingDefOf.Misc4;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 sq)
		{
			if (!sq.InBounds(base.Map))
			{
				return false;
			}
			if (sq.Fogged(base.Map))
			{
				return false;
			}
			if (base.Map.zoneManager.ZoneAt(sq) == null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Zone zone = base.Map.zoneManager.ZoneAt(c);
			zone.RemoveCell(c);
			if (!this.justDesignated.Contains(zone))
			{
				this.justDesignated.Add(zone);
			}
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			for (int i = 0; i < this.justDesignated.Count; i++)
			{
				this.justDesignated[i].CheckContiguous();
			}
			this.justDesignated.Clear();
		}
	}
}
