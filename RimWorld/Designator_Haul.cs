using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Haul : Designator
	{
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Haul()
		{
			this.defaultLabel = "DesignatorHaulThings".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul", true);
			this.defaultDesc = "DesignatorHaulThingsDesc".Translate();
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateHaul;
			this.hotKey = KeyBindingDefOf.Misc12;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds() || c.Fogged())
			{
				return false;
			}
			Thing firstHaulable = c.GetFirstHaulable();
			if (firstHaulable == null)
			{
				return "MessageMustDesignateHaulable".Translate();
			}
			AcceptanceReport result = this.CanDesignateThing(firstHaulable);
			if (!result.Accepted)
			{
				return result;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			this.DesignateThing(c.GetFirstHaulable());
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (!t.def.designateHaulable)
			{
				return false;
			}
			if (Find.DesignationManager.DesignationOn(t, DesignationDefOf.Haul) != null)
			{
				return false;
			}
			if (t.IsInValidStorage())
			{
				return "MessageAlreadyInStorage".Translate();
			}
			return true;
		}

		public override void DesignateThing(Thing t)
		{
			Find.DesignationManager.AddDesignation(new Designation(t, DesignationDefOf.Haul));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
