using System;
using Verse;

namespace RimWorld
{
	public abstract class PawnColumnWorker_Designator : PawnColumnWorker_Checkbox
	{
		protected abstract DesignationDef DesignationType
		{
			get;
		}

		protected virtual void Notify_DesignationAdded(Pawn pawn)
		{
		}

		protected override bool GetValue(Pawn pawn)
		{
			return this.GetDesignation(pawn) != null;
		}

		protected override void SetValue(Pawn pawn, bool value)
		{
			if (value == this.GetValue(pawn))
			{
				return;
			}
			if (value)
			{
				pawn.MapHeld.designationManager.AddDesignation(new Designation(pawn, this.DesignationType));
				this.Notify_DesignationAdded(pawn);
			}
			else
			{
				Designation designation = this.GetDesignation(pawn);
				if (designation != null)
				{
					pawn.MapHeld.designationManager.RemoveDesignation(designation);
				}
			}
		}

		private Designation GetDesignation(Pawn pawn)
		{
			Map mapHeld = pawn.MapHeld;
			if (mapHeld == null)
			{
				return null;
			}
			return mapHeld.designationManager.DesignationOn(pawn, this.DesignationType);
		}
	}
}
