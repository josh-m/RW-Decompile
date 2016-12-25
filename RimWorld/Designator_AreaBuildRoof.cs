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
			if (!c.InBounds())
			{
				return false;
			}
			if (c.Fogged())
			{
				return false;
			}
			bool flag = Find.AreaBuildRoof[c];
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
				Find.AreaBuildRoof.Set(c);
				Find.AreaNoRoof.Clear(c);
			}
			else if (this.mode == DesignateMode.Remove)
			{
				Find.AreaBuildRoof.Clear(c);
			}
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
			Find.AreaNoRoof.MarkForDraw();
			Find.AreaBuildRoof.MarkForDraw();
		}
	}
}
