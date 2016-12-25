using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_RemoveFloor : Designator
	{
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public Designator_RemoveFloor()
		{
			this.defaultLabel = "DesignatorRemoveFloor".Translate();
			this.defaultDesc = "DesignatorRemoveFloorDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/RemoveFloor", true);
			this.useMouseIcon = true;
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.soundSucceeded = SoundDefOf.DesignateSmoothFloor;
			this.hotKey = KeyBindingDefOf.Misc1;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map) || c.Fogged(base.Map))
			{
				return false;
			}
			if (base.Map.designationManager.DesignationAt(c, DesignationDefOf.RemoveFloor) != null)
			{
				return false;
			}
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && edifice.def.Fillage == FillCategory.Full && edifice.def.passability == Traversability.Impassable)
			{
				return false;
			}
			if (!base.Map.terrainGrid.TerrainAt(c).layerable)
			{
				return "TerrainMustBeRemovable".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (DebugSettings.godMode)
			{
				base.Map.terrainGrid.RemoveTopLayer(c, true);
				return;
			}
			base.Map.designationManager.AddDesignation(new Designation(c, DesignationDefOf.RemoveFloor));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
