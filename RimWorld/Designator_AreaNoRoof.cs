using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class Designator_AreaNoRoof : Designator
	{
		private DesignateMode mode;

		private static List<IntVec3> justRemovedCells = new List<IntVec3>();

		private static List<IntVec3> justAddedCells = new List<IntVec3>();

		private static List<Room> requestedRooms = new List<Room>();

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

		public Designator_AreaNoRoof(DesignateMode mode)
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
			RoofDef roofDef = base.Map.roofGrid.RoofAt(c);
			if (roofDef != null && roofDef.isThickRoof)
			{
				return "MessageNothingCanRemoveThickRoofs".Translate();
			}
			bool flag = base.Map.areaManager.NoRoof[c];
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
				base.Map.areaManager.NoRoof[c] = true;
				Designator_AreaNoRoof.justAddedCells.Add(c);
			}
			else if (this.mode == DesignateMode.Remove)
			{
				base.Map.areaManager.NoRoof[c] = false;
				Designator_AreaNoRoof.justRemovedCells.Add(c);
			}
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			if (this.mode == DesignateMode.Add)
			{
				for (int i = 0; i < Designator_AreaNoRoof.justAddedCells.Count; i++)
				{
					base.Map.areaManager.BuildRoof[Designator_AreaNoRoof.justAddedCells[i]] = false;
				}
				Designator_AreaNoRoof.justAddedCells.Clear();
			}
			else if (this.mode == DesignateMode.Remove)
			{
				for (int j = 0; j < Designator_AreaNoRoof.justRemovedCells.Count; j++)
				{
					IntVec3 intVec = Designator_AreaNoRoof.justRemovedCells[j];
					Room room = intVec.GetRoom(base.Map);
					if (room == null)
					{
						base.Map.autoBuildRoofAreaSetter.TryGenerateAreaOnImpassable(intVec);
					}
					else if (!Designator_AreaNoRoof.requestedRooms.Contains(room))
					{
						base.Map.autoBuildRoofAreaSetter.TryGenerateAreaFor(room);
						Designator_AreaNoRoof.requestedRooms.Add(room);
					}
				}
				Designator_AreaNoRoof.justRemovedCells.Clear();
				Designator_AreaNoRoof.requestedRooms.Clear();
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
