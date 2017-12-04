using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaBuildRoof : Designator
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

		public Designator_AreaBuildRoof()
		{
			this.defaultLabel = "DesignatorAreaBuildRoofExpand".Translate();
			this.defaultDesc = "DesignatorAreaBuildRoofExpandDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/BuildRoofArea", true);
			this.hotKey = KeyBindingDefOf.Misc10;
			this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaAdd;
			this.useMouseIcon = true;
			this.tutorTag = "AreaBuildRoofExpand";
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
			bool flag = base.Map.areaManager.BuildRoof[c];
			return !flag;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			base.Map.areaManager.BuildRoof[c] = true;
			base.Map.areaManager.NoRoof[c] = false;
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}
	}
}
