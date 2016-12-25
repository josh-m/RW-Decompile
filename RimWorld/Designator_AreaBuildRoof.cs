using System;
using Verse;

namespace RimWorld
{
	public abstract class Designator_AreaBuildRoof : Designator
	{
		private DesignateMode mode;

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

		public Designator_AreaBuildRoof(DesignateMode mode)
		{
			this.mode = mode;
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
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
			if (this.mode == DesignateMode.Add)
			{
				return !flag;
			}
			return flag;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (this.mode == DesignateMode.Add)
			{
				base.Map.areaManager.BuildRoof[c] = true;
				base.Map.areaManager.NoRoof[c] = false;
			}
			else if (this.mode == DesignateMode.Remove)
			{
				base.Map.areaManager.BuildRoof[c] = false;
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}
	}
}
