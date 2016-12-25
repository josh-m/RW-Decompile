using System;
using Verse;

namespace RimWorld
{
	public abstract class Designator_Plants : Designator
	{
		protected DesignationDef designationDef;

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Plants()
		{
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (t.def.plant == null)
			{
				return false;
			}
			if (Find.DesignationManager.DesignationOn(t, this.designationDef) != null)
			{
				return false;
			}
			return true;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds() || c.Fogged())
			{
				return false;
			}
			Plant plant = c.GetPlant();
			if (plant == null)
			{
				return "MessageMustDesignatePlants".Translate();
			}
			AcceptanceReport result = this.CanDesignateThing(plant);
			if (!result.Accepted)
			{
				return result;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			this.DesignateThing(c.GetPlant());
		}

		public override void DesignateThing(Thing t)
		{
			Find.DesignationManager.RemoveAllDesignationsOn(t, false);
			Find.DesignationManager.AddDesignation(new Designation(t, this.designationDef));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
