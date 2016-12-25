using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Cancel : Designator
	{
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Cancel()
		{
			this.defaultLabel = "DesignatorCancel".Translate();
			this.defaultDesc = "DesignatorCancelDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
			this.useMouseIcon = true;
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.soundSucceeded = SoundDefOf.DesignateCancel;
			this.hotKey = KeyBindingDefOf.DesignatorCancel;
			this.tutorTag = "Cancel";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (this.CancelableDesignationsAt(c).Count<Designation>() > 0)
			{
				return true;
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (this.CanDesignateThing(thingList[i]).Accepted)
				{
					return true;
				}
			}
			return false;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Designation current in this.CancelableDesignationsAt(c).ToList<Designation>())
			{
				if (current.def.designateCancelable)
				{
					base.Map.designationManager.RemoveDesignation(current);
				}
			}
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = thingList.Count - 1; i >= 0; i--)
			{
				if (this.CanDesignateThing(thingList[i]).Accepted)
				{
					this.DesignateThing(thingList[i]);
				}
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (base.Map.designationManager.DesignationOn(t) != null)
			{
				foreach (Designation current in base.Map.designationManager.AllDesignationsOn(t))
				{
					if (current.def.designateCancelable)
					{
						return true;
					}
				}
			}
			if (t.def.mineable && base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) != null)
			{
				return true;
			}
			return t.Faction == Faction.OfPlayer && (t is Frame || t is Blueprint);
		}

		public override void DesignateThing(Thing t)
		{
			if (t is Frame || t is Blueprint)
			{
				t.Destroy(DestroyMode.Cancel);
			}
			else
			{
				base.Map.designationManager.RemoveAllDesignationsOn(t, true);
				if (t.def.mineable)
				{
					Designation designation = base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine);
					if (designation != null)
					{
						base.Map.designationManager.RemoveDesignation(designation);
					}
				}
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		private IEnumerable<Designation> CancelableDesignationsAt(IntVec3 c)
		{
			return from x in base.Map.designationManager.AllDesignationsAt(c)
			where x.def != DesignationDefOf.Plan
			select x;
		}
	}
}
