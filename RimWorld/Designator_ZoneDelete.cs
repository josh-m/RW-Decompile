using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_ZoneDelete : Designator_Zone
	{
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
			if (!sq.InBounds())
			{
				return false;
			}
			if (sq.Fogged() || !sq.InBounds())
			{
				return false;
			}
			if (Find.ZoneManager.ZoneAt(sq) == null)
			{
				return false;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Zone zone = Find.ZoneManager.ZoneAt(c);
			zone.RemoveCell(c);
			zone.CheckContiguous();
		}
	}
}
